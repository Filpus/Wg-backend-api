using System.Text.Json;
using System.Text.Json.Serialization;
using Wg_backend_api.Logic.Modifiers.Interfaces;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;

namespace Wg_backend_api.Serialization
{
    public class IBaseModifierConditionsConverter : JsonConverter<IBaseModifierConditions>
    {
        public override IBaseModifierConditions Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var root = jsonDoc.RootElement;
                var json = root.GetRawText();

                // Jeśli jest "$type" - użyj go
                if (root.TryGetProperty("$type", out var typeProperty))
                {
                    var typeName = typeProperty.GetString();
                    return DeserializeByTypeName(typeName, json, options);
                }

                // Jeśli NIE ma "$type" - spróbuj odgadnąć na podstawie properties
                // To jest fallback dla frontenda które nie wysyła "$type"
                return GuessAndDeserialize(root, options);
            }
        }

        private IBaseModifierConditions DeserializeByTypeName(string typeName, string json, JsonSerializerOptions options)
        {
            return typeName switch
            {
                "resource" => JsonSerializer.Deserialize<ResourceConditions>(json, options),
                "population" => JsonSerializer.Deserialize<PopulationConditions>(json, options),
                "population_resource" => JsonSerializer.Deserialize<PopulationResourceConditions>(json, options),
                "faction" => JsonSerializer.Deserialize<FactionConditions>(json, options),
                _ => throw new JsonException($"Unknown type discriminator: {typeName}")
            };
        }

        private IBaseModifierConditions GuessAndDeserialize(JsonElement root, JsonSerializerOptions options)
        {
            var json = root.GetRawText();

            // Heurystyka - czym jest to Conditions?
            bool hasResourceId = root.TryGetProperty("resourceId", out _);
            bool hasCultureId = root.TryGetProperty("cultureId", out _);
            bool hasSocialGroupId = root.TryGetProperty("socialGroupId", out _);
            bool hasReligionId = root.TryGetProperty("religionId", out _);
            bool hasFactionId = root.TryGetProperty("factionId", out _);

            // ResourceConditions
            if (hasResourceId && !hasCultureId && !hasSocialGroupId && !hasReligionId && !hasFactionId)
            {
                return JsonSerializer.Deserialize<ResourceConditions>(json, options);
            }

            // PopulationResourceConditions
            if (hasResourceId && (hasCultureId || hasSocialGroupId || hasReligionId))
            {
                return JsonSerializer.Deserialize<PopulationResourceConditions>(json, options);
            }

            // FactionConditions
            if (hasFactionId)
            {
                return JsonSerializer.Deserialize<FactionConditions>(json, options);
            }

            // PopulationConditions (domyślnie jeśli są culture/social/religion bez resource)
            if ((hasCultureId || hasSocialGroupId || hasReligionId) && !hasResourceId)
            {
                return JsonSerializer.Deserialize<PopulationConditions>(json, options);
            }

            // Empty PopulationConditions (wszystkie pola optional)
            if (!hasResourceId && !hasFactionId &&
                !hasCultureId && !hasSocialGroupId && !hasReligionId)
            {
                return new PopulationConditions();
            }

            throw new JsonException(
                $"Cannot determine Conditions type from JSON properties. " +
                $"Expected one of: resourceId, factionId, or culture/social/religion IDs");
        }

        public override void Write(
            Utf8JsonWriter writer,
            IBaseModifierConditions value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var json = JsonSerializer.Serialize(value, value.GetType(), options);
            using (var doc = JsonDocument.Parse(json))
            {
                doc.RootElement.WriteTo(writer);
            }
        }
    }
}
