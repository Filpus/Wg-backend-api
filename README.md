### generate sql seeder

```bash
psql -U username -d databse_name -f wg-init-db-seeder.sql
```
Example :
```bash
psql -U postgres -d wg -f wg-init-db-seeder.sql
```
game-schema-init.sql file, is for creating the schema of game_1, without global schema, or any data.

### before push changes

Format main project
```bash
dotnet format .\Wg-backend-api\
```
For more info check: [Microsoft dotnet-format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
