--
-- PostgreSQL database dump
--



-- Dumped from database version 17.6
-- Dumped by pg_dump version 18.0

-- Started on 2025-12-05 21:40:20

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 6 (class 2615 OID 75072)
-- Name: Global; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA "Global";


ALTER SCHEMA "Global" OWNER TO postgres;

--
-- TOC entry 7 (class 2615 OID 75073)
-- Name: game_1; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA game_1;


ALTER SCHEMA game_1 OWNER TO postgres;

--
-- TOC entry 289 (class 1255 OID 75074)
-- Name: add_nation_to_all_resources(); Type: FUNCTION; Schema: game_1; Owner: postgres
--

CREATE FUNCTION game_1.add_nation_to_all_resources() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    nationId INT := NEW.id;
BEGIN
    INSERT INTO game_1."ownedResources" (fk_nation, fk_resource, amount)
    SELECT nationId, r.id, 0
    FROM game_1.resources r;

    RETURN NEW;
END;
$$;


ALTER FUNCTION game_1.add_nation_to_all_resources() OWNER TO postgres;

--
-- TOC entry 290 (class 1255 OID 75075)
-- Name: add_population_relations(); Type: FUNCTION; Schema: game_1; Owner: postgres
--

CREATE FUNCTION game_1.add_population_relations() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    socialGroupId INT := NEW.fk_socialgroups;
    popId INT := NEW.id;
BEGIN
    -- Tworzenie rekordów w populationproductionshares
    INSERT INTO game_1.populationproductionshares (fk_population, fk_resources, coefficient)
    SELECT popId, ps."fk_Resources", ps.coefficient
    FROM game_1."productionShares" ps
    WHERE ps."fk_SocialGroups" = socialGroupId;

    -- Tworzenie rekordów w populationusedresource
    INSERT INTO game_1.populationusedresource (fk_population, fk_resources, amount)
    SELECT popId, ur."fk_Resources", ur.amount
    FROM game_1."usedResources" ur
    WHERE ur."fk_SocialGroups" = socialGroupId;

    RETURN NEW;
END;
$$;


ALTER FUNCTION game_1.add_population_relations() OWNER TO postgres;

--
-- TOC entry 291 (class 1255 OID 75076)
-- Name: add_production_shares_to_populations(); Type: FUNCTION; Schema: game_1; Owner: postgres
--

CREATE FUNCTION game_1.add_production_shares_to_populations() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    socialGroupId INT := NEW."fk_SocialGroups";
BEGIN
    INSERT INTO game_1.populationproductionshares (fk_population, fk_resources, coefficient)
    SELECT p.id, NEW."fk_Resources", NEW.coefficient
    FROM game_1.populations p
    WHERE p.fk_socialgroups = socialGroupId;

    RETURN NEW;
END;
$$;


ALTER FUNCTION game_1.add_production_shares_to_populations() OWNER TO postgres;

--
-- TOC entry 292 (class 1255 OID 75077)
-- Name: add_resource_to_all_nations(); Type: FUNCTION; Schema: game_1; Owner: postgres
--

CREATE FUNCTION game_1.add_resource_to_all_nations() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    resourceId INT := NEW.id;
BEGIN
    INSERT INTO game_1."ownedResources" (fk_nation, fk_resource, amount)
    SELECT n.id, resourceId, 0
    FROM game_1.nations n;

    RETURN NEW;
END;
$$;


ALTER FUNCTION game_1.add_resource_to_all_nations() OWNER TO postgres;

--
-- TOC entry 293 (class 1255 OID 75078)
-- Name: add_used_resources_to_populations(); Type: FUNCTION; Schema: game_1; Owner: postgres
--

CREATE FUNCTION game_1.add_used_resources_to_populations() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    socialGroupId INT := NEW."fk_SocialGroups";
BEGIN
    INSERT INTO game_1.populationusedresource (fk_population, fk_resources, amount)
    SELECT p.id, NEW."fk_Resources", NEW.amount
    FROM game_1.populations p
    WHERE p.fk_socialgroups = socialGroupId;

    RETURN NEW;
END;
$$;


ALTER FUNCTION game_1.add_used_resources_to_populations() OWNER TO postgres;

--
-- TOC entry 294 (class 1255 OID 75079)
-- Name: create_default_armies(); Type: FUNCTION; Schema: game_1; Owner: postgres
--

CREATE FUNCTION game_1.create_default_armies() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    -- Create Barracks (Land army)
    INSERT INTO game_1.armies (name, "fk_Nations", fk_localisations, is_naval)
    VALUES ('Baraki', NEW.id, NULL, FALSE);

    -- Create Docks (Naval army)
    INSERT INTO game_1.armies (name, "fk_Nations", fk_localisations, is_naval)
    VALUES ('Doki', NEW.id, NULL, TRUE);

    RETURN NEW;
END;
$$;


ALTER FUNCTION game_1.create_default_armies() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 219 (class 1259 OID 75080)
-- Name: gameaccess; Type: TABLE; Schema: Global; Owner: postgres
--

CREATE TABLE "Global".gameaccess (
    id integer NOT NULL,
    "fk_Users" integer NOT NULL,
    "fk_Games" integer NOT NULL,
    "accessType" integer NOT NULL,
    "isArchived" boolean NOT NULL
);


ALTER TABLE "Global".gameaccess OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 75083)
-- Name: gameaccess_id_seq; Type: SEQUENCE; Schema: Global; Owner: postgres
--

ALTER TABLE "Global".gameaccess ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "Global".gameaccess_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 221 (class 1259 OID 75084)
-- Name: games; Type: TABLE; Schema: Global; Owner: postgres
--

CREATE TABLE "Global".games (
    id integer NOT NULL,
    name text NOT NULL,
    description text,
    image text,
    "ownerId" integer NOT NULL,
    game_code text DEFAULT upper(substr(md5((random())::text), 1, 6))
);


ALTER TABLE "Global".games OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 75090)
-- Name: games_id_seq; Type: SEQUENCE; Schema: Global; Owner: postgres
--

ALTER TABLE "Global".games ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "Global".games_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 223 (class 1259 OID 75091)
-- Name: refresh_tokens; Type: TABLE; Schema: Global; Owner: postgres
--

CREATE TABLE "Global".refresh_tokens (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id integer,
    token text NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    revoked_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT now()
);


ALTER TABLE "Global".refresh_tokens OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 75098)
-- Name: users; Type: TABLE; Schema: Global; Owner: postgres
--

CREATE TABLE "Global".users (
    id integer NOT NULL,
    name text NOT NULL,
    email text NOT NULL,
    password text NOT NULL,
    issso boolean NOT NULL,
    isarchived boolean NOT NULL,
    image text
);


ALTER TABLE "Global".users OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 75103)
-- Name: users_id_seq; Type: SEQUENCE; Schema: Global; Owner: postgres
--

ALTER TABLE "Global".users ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME "Global".users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 226 (class 1259 OID 75104)
-- Name: accessToUnits; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."accessToUnits" (
    id integer NOT NULL,
    "fk_Nation" integer NOT NULL,
    "fk_UnitTypes" integer NOT NULL
);


ALTER TABLE game_1."accessToUnits" OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 75107)
-- Name: accessToUnits_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."accessToUnits" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."accessToUnits_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 228 (class 1259 OID 75108)
-- Name: accessestonations; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.accessestonations (
    id integer NOT NULL,
    fk_nations integer NOT NULL,
    fk_users integer NOT NULL,
    dateacquired timestamp with time zone NOT NULL,
    isactive boolean NOT NULL
);


ALTER TABLE game_1.accessestonations OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 75111)
-- Name: accessestonations_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.accessestonations ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.accessestonations_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 230 (class 1259 OID 75112)
-- Name: actions; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.actions (
    id integer NOT NULL,
    "fk_Nations" integer NOT NULL,
    name text,
    description text NOT NULL,
    result text,
    "isSettled" boolean NOT NULL
);


ALTER TABLE game_1.actions OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 75117)
-- Name: actions_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.actions ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.actions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 232 (class 1259 OID 75118)
-- Name: armies; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.armies (
    id integer NOT NULL,
    name text NOT NULL,
    "fk_Nations" integer NOT NULL,
    fk_localisations integer,
    is_naval boolean NOT NULL
);


ALTER TABLE game_1.armies OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 75123)
-- Name: armies_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.armies ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.armies_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 234 (class 1259 OID 75124)
-- Name: cultures; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.cultures (
    id integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE game_1.cultures OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 75129)
-- Name: cultures_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.cultures ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.cultures_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 236 (class 1259 OID 75130)
-- Name: events; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.events (
    id integer NOT NULL,
    name text NOT NULL,
    description text,
    isactive boolean NOT NULL,
    picture text
);


ALTER TABLE game_1.events OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 75135)
-- Name: events_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.events ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.events_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 238 (class 1259 OID 75136)
-- Name: factions; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.factions (
    id integer NOT NULL,
    name text NOT NULL,
    "fk_Nations" integer NOT NULL,
    power integer NOT NULL,
    agenda text NOT NULL,
    contentment integer NOT NULL,
    color text NOT NULL,
    description text
);


ALTER TABLE game_1.factions OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 75141)
-- Name: factions_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.factions ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.factions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 240 (class 1259 OID 75142)
-- Name: localisations; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.localisations (
    id integer NOT NULL,
    name text NOT NULL,
    size integer NOT NULL,
    fortifications integer NOT NULL,
    fk_nations integer NOT NULL
);


ALTER TABLE game_1.localisations OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 75147)
-- Name: localisationsResources; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."localisationsResources" (
    id integer NOT NULL,
    fk_localisations integer NOT NULL,
    "fk_Resources" integer NOT NULL,
    amount double precision NOT NULL
);


ALTER TABLE game_1."localisationsResources" OWNER TO postgres;

--
-- TOC entry 242 (class 1259 OID 75150)
-- Name: localisationsResources_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."localisationsResources" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."localisationsResources_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 243 (class 1259 OID 75151)
-- Name: localisations_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.localisations ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.localisations_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 244 (class 1259 OID 75152)
-- Name: maintenanceCosts; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."maintenanceCosts" (
    id integer NOT NULL,
    "fk_UnitTypes" integer NOT NULL,
    "fk_Resources" integer NOT NULL,
    amount double precision NOT NULL
);


ALTER TABLE game_1."maintenanceCosts" OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 75155)
-- Name: maintenanceCosts_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."maintenanceCosts" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."maintenanceCosts_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 246 (class 1259 OID 75156)
-- Name: map; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.map (
    id integer NOT NULL,
    name text NOT NULL,
    "mapLocation" text NOT NULL,
    "mapIconLocation" text NOT NULL
);


ALTER TABLE game_1.map OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 75161)
-- Name: mapAccess; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."mapAccess" (
    "fk_Nations" integer NOT NULL,
    "fk_Maps" integer NOT NULL
);


ALTER TABLE game_1."mapAccess" OWNER TO postgres;

--
-- TOC entry 248 (class 1259 OID 75164)
-- Name: map_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.map ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 249 (class 1259 OID 75165)
-- Name: modifiers; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.modifiers (
    id integer NOT NULL,
    event_id integer NOT NULL,
    modifier_type integer NOT NULL,
    effects jsonb NOT NULL
);


ALTER TABLE game_1.modifiers OWNER TO postgres;

--
-- TOC entry 250 (class 1259 OID 75170)
-- Name: modifiers_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

CREATE SEQUENCE game_1.modifiers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE game_1.modifiers_id_seq OWNER TO postgres;

--
-- TOC entry 5313 (class 0 OID 0)
-- Dependencies: 250
-- Name: modifiers_id_seq; Type: SEQUENCE OWNED BY; Schema: game_1; Owner: postgres
--

ALTER SEQUENCE game_1.modifiers_id_seq OWNED BY game_1.modifiers.id;


--
-- TOC entry 251 (class 1259 OID 75171)
-- Name: nations; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.nations (
    id integer NOT NULL,
    name text NOT NULL,
    fk_religions integer NOT NULL,
    fk_cultures integer NOT NULL,
    flag text,
    color text
);


ALTER TABLE game_1.nations OWNER TO postgres;

--
-- TOC entry 252 (class 1259 OID 75176)
-- Name: nations_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.nations ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.nations_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 253 (class 1259 OID 75177)
-- Name: offeredresources; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.offeredresources (
    id integer NOT NULL,
    fk_resource integer NOT NULL,
    fk_tradeagreement integer NOT NULL,
    quantity integer NOT NULL
);


ALTER TABLE game_1.offeredresources OWNER TO postgres;

--
-- TOC entry 254 (class 1259 OID 75180)
-- Name: offeredresources_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.offeredresources ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.offeredresources_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 255 (class 1259 OID 75181)
-- Name: ownedResources; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."ownedResources" (
    id integer NOT NULL,
    fk_nation integer NOT NULL,
    fk_resource integer NOT NULL,
    amount real DEFAULT 0 NOT NULL
);


ALTER TABLE game_1."ownedResources" OWNER TO postgres;

--
-- TOC entry 256 (class 1259 OID 75185)
-- Name: ownedResources_Id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

CREATE SEQUENCE game_1."ownedResources_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE game_1."ownedResources_Id_seq" OWNER TO postgres;

--
-- TOC entry 5314 (class 0 OID 0)
-- Dependencies: 256
-- Name: ownedResources_Id_seq; Type: SEQUENCE OWNED BY; Schema: game_1; Owner: postgres
--

ALTER SEQUENCE game_1."ownedResources_Id_seq" OWNED BY game_1."ownedResources".id;


--
-- TOC entry 257 (class 1259 OID 75186)
-- Name: players; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.players (
    id integer NOT NULL,
    "fk_User" integer NOT NULL,
    "playerType" integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE game_1.players OWNER TO postgres;

--
-- TOC entry 258 (class 1259 OID 75191)
-- Name: players_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.players ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.players_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 259 (class 1259 OID 75192)
-- Name: populationproductionshares; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.populationproductionshares (
    id integer NOT NULL,
    fk_population integer NOT NULL,
    fk_resources integer NOT NULL,
    coefficient double precision NOT NULL
);


ALTER TABLE game_1.populationproductionshares OWNER TO postgres;

--
-- TOC entry 260 (class 1259 OID 75195)
-- Name: populationproductionshares_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.populationproductionshares ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME game_1.populationproductionshares_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 261 (class 1259 OID 75196)
-- Name: populations; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.populations (
    id integer NOT NULL,
    fk_religions integer NOT NULL,
    fk_cultures integer NOT NULL,
    fk_socialgroups integer NOT NULL,
    fk_localisations integer NOT NULL,
    happiness real NOT NULL,
    volunteers integer
);


ALTER TABLE game_1.populations OWNER TO postgres;

--
-- TOC entry 262 (class 1259 OID 75199)
-- Name: populations_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.populations ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.populations_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 263 (class 1259 OID 75200)
-- Name: populationusedresource; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.populationusedresource (
    id integer NOT NULL,
    fk_population integer NOT NULL,
    fk_resources integer NOT NULL,
    amount double precision NOT NULL
);


ALTER TABLE game_1.populationusedresource OWNER TO postgres;

--
-- TOC entry 264 (class 1259 OID 75203)
-- Name: populationusedresource_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.populationusedresource ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME game_1.populationusedresource_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 265 (class 1259 OID 75204)
-- Name: productionCost; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."productionCost" (
    id integer NOT NULL,
    "fk_UnitTypes" integer NOT NULL,
    "fk_Resources" integer NOT NULL,
    amount double precision NOT NULL
);


ALTER TABLE game_1."productionCost" OWNER TO postgres;

--
-- TOC entry 266 (class 1259 OID 75207)
-- Name: productionCost_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."productionCost" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."productionCost_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 267 (class 1259 OID 75208)
-- Name: productionShares; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."productionShares" (
    id integer NOT NULL,
    "fk_SocialGroups" integer NOT NULL,
    "fk_Resources" integer NOT NULL,
    coefficient double precision NOT NULL
);


ALTER TABLE game_1."productionShares" OWNER TO postgres;

--
-- TOC entry 268 (class 1259 OID 75211)
-- Name: productionShares_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."productionShares" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."productionShares_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 269 (class 1259 OID 75212)
-- Name: relatedEvents; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."relatedEvents" (
    id integer NOT NULL,
    "fk_Events" integer NOT NULL,
    "fk_Nations" integer NOT NULL
);


ALTER TABLE game_1."relatedEvents" OWNER TO postgres;

--
-- TOC entry 270 (class 1259 OID 75215)
-- Name: relatedEvents_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."relatedEvents" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."relatedEvents_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 271 (class 1259 OID 75216)
-- Name: religions; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.religions (
    id integer NOT NULL,
    name text NOT NULL,
    icon text
);


ALTER TABLE game_1.religions OWNER TO postgres;

--
-- TOC entry 272 (class 1259 OID 75221)
-- Name: religions_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.religions ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.religions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 273 (class 1259 OID 75222)
-- Name: resources; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.resources (
    id integer NOT NULL,
    name text NOT NULL,
    ismain boolean NOT NULL,
    icon text
);


ALTER TABLE game_1.resources OWNER TO postgres;

--
-- TOC entry 274 (class 1259 OID 75227)
-- Name: resources_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.resources ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.resources_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 275 (class 1259 OID 75228)
-- Name: socialgroups; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.socialgroups (
    id integer NOT NULL,
    name text NOT NULL,
    basehappiness real NOT NULL,
    volunteers integer NOT NULL,
    icon text
);


ALTER TABLE game_1.socialgroups OWNER TO postgres;

--
-- TOC entry 276 (class 1259 OID 75233)
-- Name: socialgroups_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.socialgroups ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.socialgroups_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 277 (class 1259 OID 75234)
-- Name: tradeagreements; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.tradeagreements (
    id integer NOT NULL,
    fk_nationoffering integer NOT NULL,
    fk_nationreceiving integer NOT NULL,
    status integer NOT NULL,
    duration integer NOT NULL,
    description text NOT NULL
);


ALTER TABLE game_1.tradeagreements OWNER TO postgres;

--
-- TOC entry 278 (class 1259 OID 75239)
-- Name: tradeagreements_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.tradeagreements ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.tradeagreements_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 279 (class 1259 OID 75240)
-- Name: troops; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.troops (
    id integer NOT NULL,
    "fk_UnitTypes" integer NOT NULL,
    "fk_Armies" integer NOT NULL,
    quantity integer NOT NULL
);


ALTER TABLE game_1.troops OWNER TO postgres;

--
-- TOC entry 280 (class 1259 OID 75243)
-- Name: troops_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.troops ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.troops_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 281 (class 1259 OID 75244)
-- Name: unitOrders; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."unitOrders" (
    id integer NOT NULL,
    "fk_UnitTypes" integer NOT NULL,
    "fk_Nations" integer NOT NULL,
    quantity integer NOT NULL
);


ALTER TABLE game_1."unitOrders" OWNER TO postgres;

--
-- TOC entry 282 (class 1259 OID 75247)
-- Name: unitOrders_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."unitOrders" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."unitOrders_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 283 (class 1259 OID 75248)
-- Name: unitTypes; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."unitTypes" (
    id integer NOT NULL,
    name text NOT NULL,
    description text NOT NULL,
    melee integer NOT NULL,
    range integer NOT NULL,
    defense integer NOT NULL,
    speed integer NOT NULL,
    morale integer NOT NULL,
    "volunteersNeeded" integer NOT NULL,
    "isNaval" boolean NOT NULL
);


ALTER TABLE game_1."unitTypes" OWNER TO postgres;

--
-- TOC entry 284 (class 1259 OID 75253)
-- Name: unitTypes_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."unitTypes" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."unitTypes_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 285 (class 1259 OID 75254)
-- Name: usedResources; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."usedResources" (
    id integer NOT NULL,
    "fk_SocialGroups" integer NOT NULL,
    "fk_Resources" integer NOT NULL,
    amount double precision NOT NULL
);


ALTER TABLE game_1."usedResources" OWNER TO postgres;

--
-- TOC entry 286 (class 1259 OID 75257)
-- Name: usedResources_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."usedResources" ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1."usedResources_id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 287 (class 1259 OID 75258)
-- Name: wantedresources; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.wantedresources (
    id integer NOT NULL,
    fk_resource integer NOT NULL,
    fk_tradeagreement integer NOT NULL,
    amount double precision NOT NULL
);


ALTER TABLE game_1.wantedresources OWNER TO postgres;

--
-- TOC entry 288 (class 1259 OID 75261)
-- Name: wantedresources_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1.wantedresources ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME game_1.wantedresources_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 4926 (class 2604 OID 80249)
-- Name: modifiers id; Type: DEFAULT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.modifiers ALTER COLUMN id SET DEFAULT nextval('game_1.modifiers_id_seq'::regclass);


--
-- TOC entry 4927 (class 2604 OID 80250)
-- Name: ownedResources id; Type: DEFAULT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."ownedResources" ALTER COLUMN id SET DEFAULT nextval('game_1."ownedResources_Id_seq"'::regclass);


--
-- TOC entry 5238 (class 0 OID 75080)
-- Dependencies: 219
-- Data for Name: gameaccess; Type: TABLE DATA; Schema: Global; Owner: postgres
--

INSERT INTO "Global".gameaccess (id, "fk_Users", "fk_Games", "accessType", "isArchived") VALUES (1, 1, 1, 1, false);
INSERT INTO "Global".gameaccess (id, "fk_Users", "fk_Games", "accessType", "isArchived") VALUES (2, 2, 1, 0, false);
INSERT INTO "Global".gameaccess (id, "fk_Users", "fk_Games", "accessType", "isArchived") VALUES (3, 3, 1, 1, false);
INSERT INTO "Global".gameaccess (id, "fk_Users", "fk_Games", "accessType", "isArchived") VALUES (4, 4, 1, 1, false);


--
-- TOC entry 5240 (class 0 OID 75084)
-- Dependencies: 221
-- Data for Name: games; Type: TABLE DATA; Schema: Global; Owner: postgres
--

INSERT INTO "Global".games (id, name, description, image, "ownerId", game_code) VALUES (1, 'default_schema', 'Demo testowe
', NULL, 2, 'XIWDFW');


--
-- TOC entry 5242 (class 0 OID 75091)
-- Dependencies: 223
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: Global; Owner: postgres
--

INSERT INTO "Global".refresh_tokens (id, user_id, token, expires_at, revoked_at, created_at) VALUES ('9dcb8ab8-1ffb-4e93-a5b9-5816d839e941', 2, 'o4Sc/QlsnoBqZn3Nookts90UYzDsqAq8AZzkfDkha5xSRsXIloH9U5a5Dxgv5IfVyA/Tg8JEM0QgjmcdRFs3OQ==', '2025-12-12 14:17:52.302358+01', NULL, '2025-12-05 14:17:52.302357+01');


--
-- TOC entry 5243 (class 0 OID 75098)
-- Dependencies: 224
-- Data for Name: users; Type: TABLE DATA; Schema: Global; Owner: postgres
--

INSERT INTO "Global".users (id, name, email, password, issso, isarchived, image) VALUES (1, 'Test', 'test@test', '$2a$11$s/J0zefb5amFzjdjllmuZ.AXuziyjVRcYTeEhxyemaxsJJyKnzxU2', false, false, NULL);
INSERT INTO "Global".users (id, name, email, password, issso, isarchived, image) VALUES (2, 'admin', 'admin@admin', '$2a$11$t8DGLO5spPxXzpyRb5j0vuuk54ycsFEo9scO7xswpGCH9WvMcwive', false, false, NULL);
INSERT INTO "Global".users (id, name, email, password, issso, isarchived, image) VALUES (3, 'tomek', 'tomek@tomek', '$2a$11$t8DGLO5spPxXzpyRb5j0vuuk54ycsFEo9scO7xswpGCH9WvMcwive', false, false, NULL);
INSERT INTO "Global".users (id, name, email, password, issso, isarchived, image) VALUES (4, 'jakub', 'jakub@jakub', '$2a$11$t8DGLO5spPxXzpyRb5j0vuuk54ycsFEo9scO7xswpGCH9WvMcwive', false, false, NULL);


--
-- TOC entry 5245 (class 0 OID 75104)
-- Dependencies: 226
-- Data for Name: accessToUnits; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (1, 1, 1);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (2, 1, 2);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (3, 1, 3);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (4, 2, 1);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (5, 2, 3);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (6, 2, 4);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (7, 3, 1);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (8, 3, 5);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (9, 4, 1);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (10, 4, 2);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (11, 5, 1);
INSERT INTO game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") VALUES (12, 5, 3);


--
-- TOC entry 5247 (class 0 OID 75108)
-- Dependencies: 228
-- Data for Name: accessestonations; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.accessestonations (id, fk_nations, fk_users, dateacquired, isactive) VALUES (1, 1, 1, '2025-01-01 00:00:00+01', true);
INSERT INTO game_1.accessestonations (id, fk_nations, fk_users, dateacquired, isactive) VALUES (2, 2, 2, '2025-01-02 00:00:00+01', true);


--
-- TOC entry 5249 (class 0 OID 75112)
-- Dependencies: 230
-- Data for Name: actions; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.actions (id, "fk_Nations", name, description, result, "isSettled") VALUES (1, 1, 'Ekspedycja', 'Wysłanie ekspedycji na niezbadane tereny', NULL, false);
INSERT INTO game_1.actions (id, "fk_Nations", name, description, result, "isSettled") VALUES (2, 2, 'Budowa Świątyni', 'Rozpoczęcie budowy wielkiej świątyni', NULL, false);
INSERT INTO game_1.actions (id, "fk_Nations", name, description, result, "isSettled") VALUES (3, 3, 'Szlak Handlowy', 'Otwarcie nowego szlaku handlowego', 'Zwiększenie przychodów o 10%', true);
INSERT INTO game_1.actions (id, "fk_Nations", name, description, result, "isSettled") VALUES (4, 4, 'Reformy', 'Wprowadzenie reform społecznych', NULL, false);
INSERT INTO game_1.actions (id, "fk_Nations", name, description, result, "isSettled") VALUES (5, 5, 'Mobilizacja', 'Mobilizacja sił zbrojnych', 'Wzrost liczebności armii o 20%', true);


--
-- TOC entry 5251 (class 0 OID 75118)
-- Dependencies: 232
-- Data for Name: armies; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (1, 'Armia Północy', 1, 1, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (2, 'Legiony Cesarskie', 2, 2, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (3, 'Flota Republiki', 3, 3, true);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (4, 'Drużyna Księcia', 4, 4, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (5, 'Jeźdźcy Pustyni', 5, 5, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (6, 'Baraki', 1, NULL, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (7, 'Baraki', 2, NULL, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (8, 'Baraki', 3, NULL, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (9, 'Baraki', 4, NULL, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (10, 'Baraki', 5, NULL, false);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (11, 'Doki', 1, NULL, true);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (12, 'Doki', 2, NULL, true);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (13, 'Doki', 3, NULL, true);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (14, 'Doki', 4, NULL, true);
INSERT INTO game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) VALUES (15, 'Doki', 5, NULL, true);


--
-- TOC entry 5253 (class 0 OID 75124)
-- Dependencies: 234
-- Data for Name: cultures; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.cultures (id, name) VALUES (1, 'Nordycka');
INSERT INTO game_1.cultures (id, name) VALUES (2, 'Słowiańska');
INSERT INTO game_1.cultures (id, name) VALUES (3, 'Germańska');
INSERT INTO game_1.cultures (id, name) VALUES (4, 'Romańska');
INSERT INTO game_1.cultures (id, name) VALUES (5, 'Grecka');


--
-- TOC entry 5255 (class 0 OID 75130)
-- Dependencies: 236
-- Data for Name: events; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.events (id, name, description, isactive, picture) VALUES (1, 'Wielka Bitwa', 'Epiczna bitwa, która zmieniła losy świata', true, 'battle.jpg');
INSERT INTO game_1.events (id, name, description, isactive, picture) VALUES (2, 'Plaga', 'Śmiertelna zaraza dziesiątkująca populację', true, 'plague.jpg');
INSERT INTO game_1.events (id, name, description, isactive, picture) VALUES (3, 'Odkrycie', 'Odkrycie nowych terenów i technologii', true, 'discovery.jpg');
INSERT INTO game_1.events (id, name, description, isactive, picture) VALUES (4, 'Rewolta', 'Rewolta społeczeństwa przeciwko władcy', true, 'revolt.jpg');
INSERT INTO game_1.events (id, name, description, isactive, picture) VALUES (5, 'Sojusz', 'Zawarcie sojuszu między narodami', true, 'alliance.jpg');


--
-- TOC entry 5257 (class 0 OID 75136)
-- Dependencies: 238
-- Data for Name: factions; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.factions (id, name, "fk_Nations", power, agenda, contentment, color, description) VALUES (1, 'Konserwatyści', 1, 70, 'Utrzymanie tradycji', 60, '#0000FF', NULL);
INSERT INTO game_1.factions (id, name, "fk_Nations", power, agenda, contentment, color, description) VALUES (2, 'Reformatorzy', 1, 30, 'Wprowadzenie zmian', 40, '#00FF00', NULL);
INSERT INTO game_1.factions (id, name, "fk_Nations", power, agenda, contentment, color, description) VALUES (3, 'Militaryści', 2, 60, 'Ekspansja militarna', 50, '#FF0000', NULL);
INSERT INTO game_1.factions (id, name, "fk_Nations", power, agenda, contentment, color, description) VALUES (4, 'Handlarze', 2, 40, 'Rozwój handlu', 70, '#FFFF00', NULL);
INSERT INTO game_1.factions (id, name, "fk_Nations", power, agenda, contentment, color, description) VALUES (5, 'Zjednoczeni', 3, 90, 'Jedność narodu', 80, '#800080', NULL);


--
-- TOC entry 5259 (class 0 OID 75142)
-- Dependencies: 240
-- Data for Name: localisations; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.localisations (id, name, size, fortifications, fk_nations) VALUES (1, 'Stolica Północy', 5, 4, 1);
INSERT INTO game_1.localisations (id, name, size, fortifications, fk_nations) VALUES (2, 'Twierdza Cesarska', 6, 5, 2);
INSERT INTO game_1.localisations (id, name, size, fortifications, fk_nations) VALUES (3, 'Port Republiki', 4, 3, 3);
INSERT INTO game_1.localisations (id, name, size, fortifications, fk_nations) VALUES (4, 'Wschodni Gród', 3, 2, 4);
INSERT INTO game_1.localisations (id, name, size, fortifications, fk_nations) VALUES (5, 'Oaza Południowa', 4, 3, 5);
INSERT INTO game_1.localisations (id, name, size, fortifications, fk_nations) VALUES (6, 'Górska Osada', 2, 1, 1);
INSERT INTO game_1.localisations (id, name, size, fortifications, fk_nations) VALUES (7, 'Cesarskie Tereny', 3, 2, 2);
INSERT INTO game_1.localisations (id, name, size, fortifications, fk_nations) VALUES (8, 'Nadmorska Wioska', 2, 1, 3);


--
-- TOC entry 5260 (class 0 OID 75147)
-- Dependencies: 241
-- Data for Name: localisationsResources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (1, 1, 1, 1000);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (2, 1, 3, 2000);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (3, 2, 2, 1500);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (4, 2, 5, 3000);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (5, 3, 1, 800);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (6, 3, 8, 500);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (7, 4, 3, 2500);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (8, 4, 4, 3500);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (9, 5, 7, 600);
INSERT INTO game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) VALUES (10, 5, 6, 400);


--
-- TOC entry 5263 (class 0 OID 75152)
-- Dependencies: 244
-- Data for Name: maintenanceCosts; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (1, 1, 1, 0.5);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (2, 1, 4, 1);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (3, 2, 1, 0.3);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (4, 2, 3, 0.5);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (5, 3, 1, 1);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (6, 3, 4, 1.5);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (7, 4, 1, 2);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (8, 4, 3, 1);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (9, 5, 1, 3);
INSERT INTO game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (10, 5, 3, 2);


--
-- TOC entry 5265 (class 0 OID 75156)
-- Dependencies: 246
-- Data for Name: map; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.map (id, name, "mapLocation", "mapIconLocation") VALUES (1, 'Europa Środkowa', 'maps/central_europe.jpg', 'maps/central_europe.jpg');
INSERT INTO game_1.map (id, name, "mapLocation", "mapIconLocation") VALUES (2, 'Wyspy Brytyjskie', 'maps/british_isles.jpg', 'maps/central_europe.jpg');
INSERT INTO game_1.map (id, name, "mapLocation", "mapIconLocation") VALUES (3, 'Półwysep Iberyjski', 'maps/iberia.jpg', 'maps/central_europe.jpg');
INSERT INTO game_1.map (id, name, "mapLocation", "mapIconLocation") VALUES (4, 'Skandynawia', 'maps/scandinavia.jpg', 'maps/central_europe.jpg');
INSERT INTO game_1.map (id, name, "mapLocation", "mapIconLocation") VALUES (5, 'Bałkany', 'maps/balkans.jpg', 'maps/central_europe.jpg');


--
-- TOC entry 5266 (class 0 OID 75161)
-- Dependencies: 247
-- Data for Name: mapAccess; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."mapAccess" ("fk_Nations", "fk_Maps") VALUES (1, 1);
INSERT INTO game_1."mapAccess" ("fk_Nations", "fk_Maps") VALUES (1, 4);
INSERT INTO game_1."mapAccess" ("fk_Nations", "fk_Maps") VALUES (2, 2);
INSERT INTO game_1."mapAccess" ("fk_Nations", "fk_Maps") VALUES (2, 5);
INSERT INTO game_1."mapAccess" ("fk_Nations", "fk_Maps") VALUES (3, 3);
INSERT INTO game_1."mapAccess" ("fk_Nations", "fk_Maps") VALUES (4, 1);
INSERT INTO game_1."mapAccess" ("fk_Nations", "fk_Maps") VALUES (4, 5);


--
-- TOC entry 5268 (class 0 OID 75165)
-- Dependencies: 249
-- Data for Name: modifiers; Type: TABLE DATA; Schema: game_1; Owner: postgres
--



--
-- TOC entry 5270 (class 0 OID 75171)
-- Dependencies: 251
-- Data for Name: nations; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.nations (id, name, fk_religions, fk_cultures, flag, color) VALUES (1, 'Królestwo Północy', 1, 1, NULL, 'red');
INSERT INTO game_1.nations (id, name, fk_religions, fk_cultures, flag, color) VALUES (2, 'Cesarstwo Centralne', 2, 3, NULL, 'yellow');
INSERT INTO game_1.nations (id, name, fk_religions, fk_cultures, flag, color) VALUES (3, 'Republika Nadmorska', 2, 4, NULL, 'red');
INSERT INTO game_1.nations (id, name, fk_religions, fk_cultures, flag, color) VALUES (4, 'Księstwo Wschodnie', 1, 2, NULL, 'green');
INSERT INTO game_1.nations (id, name, fk_religions, fk_cultures, flag, color) VALUES (5, 'Kalifat Południowy', 3, 5, NULL, 'blue');


--
-- TOC entry 5272 (class 0 OID 75177)
-- Dependencies: 253
-- Data for Name: offeredresources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.offeredresources (id, fk_resource, fk_tradeagreement, quantity) VALUES (1, 2, 1, 100);
INSERT INTO game_1.offeredresources (id, fk_resource, fk_tradeagreement, quantity) VALUES (2, 3, 1, 200);
INSERT INTO game_1.offeredresources (id, fk_resource, fk_tradeagreement, quantity) VALUES (3, 1, 3, 50);
INSERT INTO game_1.offeredresources (id, fk_resource, fk_tradeagreement, quantity) VALUES (4, 4, 3, 300);
INSERT INTO game_1.offeredresources (id, fk_resource, fk_tradeagreement, quantity) VALUES (5, 3, 4, 150);


--
-- TOC entry 5274 (class 0 OID 75181)
-- Dependencies: 255
-- Data for Name: ownedResources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (1, 1, 1, 4201);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (2, 1, 2, 4202);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (3, 1, 3, 4203);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (4, 1, 4, 4204);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (5, 1, 5, 4205);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (6, 1, 6, 4206);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (7, 1, 7, 4207);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (8, 1, 8, 4208);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (9, 1, 9, 4209);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (10, 2, 1, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (11, 2, 2, 2173);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (12, 2, 3, 1237);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (13, 2, 4, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (14, 2, 5, 1273);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (15, 2, 6, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (16, 2, 7, 1237);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (17, 2, 8, 2173);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (18, 2, 9, 1237);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (19, 3, 1, 4201);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (20, 3, 2, 4202);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (21, 3, 3, 4203);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (22, 3, 4, 4204);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (23, 3, 5, 4205);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (24, 3, 6, 4206);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (25, 3, 7, 4207);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (26, 3, 8, 4208);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (27, 3, 9, 4209);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (28, 4, 1, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (29, 4, 2, 2173);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (30, 4, 3, 1237);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (31, 4, 4, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (32, 4, 5, 1273);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (33, 4, 6, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (34, 4, 7, 1237);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (35, 4, 8, 2173);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (36, 4, 9, 1237);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (37, 5, 1, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (38, 5, 2, 2173);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (39, 5, 3, 1237);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (40, 5, 4, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (41, 5, 5, 1273);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (42, 5, 6, 2137);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (43, 5, 7, 1237);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (44, 5, 8, 2173);
INSERT INTO game_1."ownedResources" (id, fk_nation, fk_resource, amount) VALUES (45, 5, 9, 1237);


--
-- TOC entry 5276 (class 0 OID 75186)
-- Dependencies: 257
-- Data for Name: players; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.players (id, "fk_User", "playerType", name) VALUES (1, 1, 1, 'Test');
INSERT INTO game_1.players (id, "fk_User", "playerType", name) VALUES (2, 2, 0, 'admin');
INSERT INTO game_1.players (id, "fk_User", "playerType", name) VALUES (3, 3, 1, 'tomek');
INSERT INTO game_1.players (id, "fk_User", "playerType", name) VALUES (4, 4, 1, 'jakub');


--
-- TOC entry 5278 (class 0 OID 75192)
-- Dependencies: 259
-- Data for Name: populationproductionshares; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) OVERRIDING SYSTEM VALUE VALUES (1, 1, 1, 1.1);
INSERT INTO game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) OVERRIDING SYSTEM VALUE VALUES (2, 1, 2, 2.1);
INSERT INTO game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) OVERRIDING SYSTEM VALUE VALUES (3, 2, 1, 3.7);
INSERT INTO game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) OVERRIDING SYSTEM VALUE VALUES (4, 3, 3, 6.9);
INSERT INTO game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) OVERRIDING SYSTEM VALUE VALUES (5, 4, 4, 1.2);
INSERT INTO game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) OVERRIDING SYSTEM VALUE VALUES (6, 5, 5, 1.2);
INSERT INTO game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) OVERRIDING SYSTEM VALUE VALUES (7, 6, 6, 0.9);
INSERT INTO game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) OVERRIDING SYSTEM VALUE VALUES (8, 7, 7, 1.3);


--
-- TOC entry 5280 (class 0 OID 75196)
-- Dependencies: 261
-- Data for Name: populations; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) VALUES (1, 1, 1, 1, 1, 5.5, 3);
INSERT INTO game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) VALUES (2, 2, 3, 2, 2, 6, 3);
INSERT INTO game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) VALUES (3, 2, 4, 3, 3, 7.2, 1);
INSERT INTO game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) VALUES (4, 1, 2, 1, 4, 4.8, 2);
INSERT INTO game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) VALUES (5, 3, 5, 5, 5, 5.7, 2);
INSERT INTO game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) VALUES (6, 1, 1, 2, 6, 5.2, 1);
INSERT INTO game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) VALUES (7, 2, 3, 4, 7, 6.8, 1);
INSERT INTO game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) VALUES (8, 2, 4, 5, 8, 6.3, 1);


--
-- TOC entry 5282 (class 0 OID 75200)
-- Dependencies: 263
-- Data for Name: populationusedresource; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.populationusedresource (id, fk_population, fk_resources, amount) OVERRIDING SYSTEM VALUE VALUES (1, 1, 1, 0.1);
INSERT INTO game_1.populationusedresource (id, fk_population, fk_resources, amount) OVERRIDING SYSTEM VALUE VALUES (2, 1, 2, 1.1);
INSERT INTO game_1.populationusedresource (id, fk_population, fk_resources, amount) OVERRIDING SYSTEM VALUE VALUES (3, 1, 3, 2.7);
INSERT INTO game_1.populationusedresource (id, fk_population, fk_resources, amount) OVERRIDING SYSTEM VALUE VALUES (4, 3, 3, 0.9);
INSERT INTO game_1.populationusedresource (id, fk_population, fk_resources, amount) OVERRIDING SYSTEM VALUE VALUES (5, 4, 4, 1.2);
INSERT INTO game_1.populationusedresource (id, fk_population, fk_resources, amount) OVERRIDING SYSTEM VALUE VALUES (6, 5, 5, 1.4);
INSERT INTO game_1.populationusedresource (id, fk_population, fk_resources, amount) OVERRIDING SYSTEM VALUE VALUES (7, 6, 6, 0.7);
INSERT INTO game_1.populationusedresource (id, fk_population, fk_resources, amount) OVERRIDING SYSTEM VALUE VALUES (8, 7, 7, 1.3);


--
-- TOC entry 5284 (class 0 OID 75204)
-- Dependencies: 265
-- Data for Name: productionCost; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (1, 1, 1, 10);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (2, 1, 2, 5);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (3, 2, 1, 8);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (4, 2, 3, 10);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (5, 3, 1, 20);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (6, 3, 2, 15);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (7, 4, 1, 30);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (8, 4, 3, 25);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (9, 5, 1, 50);
INSERT INTO game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) VALUES (10, 5, 2, 30);


--
-- TOC entry 5286 (class 0 OID 75208)
-- Dependencies: 267
-- Data for Name: productionShares; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (1, 1, 3, 2);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (2, 1, 4, 3);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (3, 2, 1, 1.5);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (4, 2, 6, 2);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (5, 3, 2, 1);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (6, 3, 5, 1.5);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (7, 4, 1, 1);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (8, 4, 7, 0.5);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (9, 5, 1, 2.5);
INSERT INTO game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) VALUES (10, 5, 8, 2);


--
-- TOC entry 5288 (class 0 OID 75212)
-- Dependencies: 269
-- Data for Name: relatedEvents; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (1, 1, 1);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (2, 1, 2);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (3, 2, 3);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (4, 2, 4);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (5, 3, 1);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (6, 3, 5);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (7, 4, 2);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (8, 4, 4);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (9, 5, 1);
INSERT INTO game_1."relatedEvents" (id, "fk_Events", "fk_Nations") VALUES (10, 5, 3);


--
-- TOC entry 5290 (class 0 OID 75216)
-- Dependencies: 271
-- Data for Name: religions; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.religions (id, name, icon) VALUES (1, 'Pogaństwo', NULL);
INSERT INTO game_1.religions (id, name, icon) VALUES (2, 'Chrześcijaństwo', NULL);
INSERT INTO game_1.religions (id, name, icon) VALUES (3, 'Islam', NULL);
INSERT INTO game_1.religions (id, name, icon) VALUES (4, 'Judaizm', NULL);
INSERT INTO game_1.religions (id, name, icon) VALUES (5, 'Zoroastrianizm', NULL);


--
-- TOC entry 5292 (class 0 OID 75222)
-- Dependencies: 273
-- Data for Name: resources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (1, 'Złoto', true, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (2, 'Żelazo', true, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (3, 'Drewno', true, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (4, 'Żywność', true, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (5, 'Kamień', true, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (6, 'Tkaniny', false, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (7, 'Przyprawy', false, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (8, 'Wino', false, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (9, 'Drewno', true, NULL);
INSERT INTO game_1.resources (id, name, ismain, icon) VALUES (11, 'Miód', true, NULL);


--
-- TOC entry 5294 (class 0 OID 75228)
-- Dependencies: 275
-- Data for Name: socialgroups; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.socialgroups (id, name, basehappiness, volunteers, icon) VALUES (1, 'Chłopi', 5, 10, NULL);
INSERT INTO game_1.socialgroups (id, name, basehappiness, volunteers, icon) VALUES (2, 'Mieszczanie', 6, 20, NULL);
INSERT INTO game_1.socialgroups (id, name, basehappiness, volunteers, icon) VALUES (3, 'Szlachta', 7, 30, NULL);
INSERT INTO game_1.socialgroups (id, name, basehappiness, volunteers, icon) VALUES (4, 'Duchowieństwo', 8, 5, NULL);
INSERT INTO game_1.socialgroups (id, name, basehappiness, volunteers, icon) VALUES (5, 'Kupcy', 6.5, 15, NULL);


--
-- TOC entry 5296 (class 0 OID 75234)
-- Dependencies: 277
-- Data for Name: tradeagreements; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.tradeagreements (id, fk_nationoffering, fk_nationreceiving, status, duration, description) VALUES (1, 1, 2, 0, 10, 'I am description');
INSERT INTO game_1.tradeagreements (id, fk_nationoffering, fk_nationreceiving, status, duration, description) VALUES (2, 1, 3, 0, 5, 'I am also description');
INSERT INTO game_1.tradeagreements (id, fk_nationoffering, fk_nationreceiving, status, duration, description) VALUES (3, 2, 4, 3, 8, 'But I m not description');
INSERT INTO game_1.tradeagreements (id, fk_nationoffering, fk_nationreceiving, status, duration, description) VALUES (4, 3, 5, 1, 12, 'What about me?');
INSERT INTO game_1.tradeagreements (id, fk_nationoffering, fk_nationreceiving, status, duration, description) VALUES (5, 4, 5, 2, 6, '');


--
-- TOC entry 5298 (class 0 OID 75240)
-- Dependencies: 279
-- Data for Name: troops; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (1, 1, 1, 500);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (2, 2, 1, 300);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (3, 3, 2, 400);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (4, 1, 2, 600);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (5, 5, 3, 20);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (6, 1, 4, 300);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (7, 3, 4, 150);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (10, 3, 5, 4);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (15, 3, 5, 120);
INSERT INTO game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) VALUES (18, 3, 5, 120);


--
-- TOC entry 5300 (class 0 OID 75244)
-- Dependencies: 281
-- Data for Name: unitOrders; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (1, 1, 1, 1);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (2, 1, 2, 1);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (3, 3, 2, 3);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (4, 4, 2, 1);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (5, 1, 3, 8);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (6, 5, 3, 5);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (7, 1, 4, 2);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (8, 2, 4, 4);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (9, 1, 5, 1);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (10, 3, 5, 4);
INSERT INTO game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) VALUES (12, 2, 1, 1);


--
-- TOC entry 5302 (class 0 OID 75248)
-- Dependencies: 283
-- Data for Name: unitTypes; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."unitTypes" (id, name, description, melee, range, defense, speed, morale, "volunteersNeeded", "isNaval") VALUES (1, 'Piechota', 'Podstawowa jednostka piechoty', 5, 0, 3, 3, 5, 100, false);
INSERT INTO game_1."unitTypes" (id, name, description, melee, range, defense, speed, morale, "volunteersNeeded", "isNaval") VALUES (2, 'Łucznicy', 'Jednostka łuczników', 1, 6, 2, 3, 4, 80, false);
INSERT INTO game_1."unitTypes" (id, name, description, melee, range, defense, speed, morale, "volunteersNeeded", "isNaval") VALUES (3, 'Kawaleria', 'Szybka jednostka kawalerii', 7, 0, 4, 6, 7, 120, false);
INSERT INTO game_1."unitTypes" (id, name, description, melee, range, defense, speed, morale, "volunteersNeeded", "isNaval") VALUES (4, 'Oblężnicza', 'Machiny oblężnicze', 1, 8, 1, 2, 3, 150, false);
INSERT INTO game_1."unitTypes" (id, name, description, melee, range, defense, speed, morale, "volunteersNeeded", "isNaval") VALUES (5, 'Okręty wojenne', 'Okręty bojowe', 6, 4, 5, 4, 6, 200, true);


--
-- TOC entry 5304 (class 0 OID 75254)
-- Dependencies: 285
-- Data for Name: usedResources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (1, 1, 4, 100);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (2, 1, 3, 50);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (3, 2, 4, 75);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (4, 2, 6, 25);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (5, 3, 4, 50);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (6, 3, 8, 30);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (7, 4, 4, 30);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (8, 4, 7, 10);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (9, 5, 4, 60);
INSERT INTO game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) VALUES (10, 5, 6, 40);


--
-- TOC entry 5306 (class 0 OID 75258)
-- Dependencies: 287
-- Data for Name: wantedresources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

INSERT INTO game_1.wantedresources (id, fk_resource, fk_tradeagreement, amount) VALUES (1, 1, 1, 50);
INSERT INTO game_1.wantedresources (id, fk_resource, fk_tradeagreement, amount) VALUES (2, 4, 1, 100);
INSERT INTO game_1.wantedresources (id, fk_resource, fk_tradeagreement, amount) VALUES (3, 2, 3, 75);
INSERT INTO game_1.wantedresources (id, fk_resource, fk_tradeagreement, amount) VALUES (4, 5, 3, 200);
INSERT INTO game_1.wantedresources (id, fk_resource, fk_tradeagreement, amount) VALUES (5, 7, 4, 25);


--
-- TOC entry 5315 (class 0 OID 0)
-- Dependencies: 220
-- Name: gameaccess_id_seq; Type: SEQUENCE SET; Schema: Global; Owner: postgres
--

SELECT pg_catalog.setval('"Global".gameaccess_id_seq', 4, true);


--
-- TOC entry 5316 (class 0 OID 0)
-- Dependencies: 222
-- Name: games_id_seq; Type: SEQUENCE SET; Schema: Global; Owner: postgres
--

SELECT pg_catalog.setval('"Global".games_id_seq', 1, true);


--
-- TOC entry 5317 (class 0 OID 0)
-- Dependencies: 225
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: Global; Owner: postgres
--

SELECT pg_catalog.setval('"Global".users_id_seq', 4, true);


--
-- TOC entry 5318 (class 0 OID 0)
-- Dependencies: 227
-- Name: accessToUnits_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."accessToUnits_id_seq"', 12, true);


--
-- TOC entry 5319 (class 0 OID 0)
-- Dependencies: 229
-- Name: accessestonations_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.accessestonations_id_seq', 5, true);


--
-- TOC entry 5320 (class 0 OID 0)
-- Dependencies: 231
-- Name: actions_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.actions_id_seq', 5, true);


--
-- TOC entry 5321 (class 0 OID 0)
-- Dependencies: 233
-- Name: armies_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.armies_id_seq', 15, true);


--
-- TOC entry 5322 (class 0 OID 0)
-- Dependencies: 235
-- Name: cultures_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.cultures_id_seq', 5, true);


--
-- TOC entry 5323 (class 0 OID 0)
-- Dependencies: 237
-- Name: events_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.events_id_seq', 5, true);


--
-- TOC entry 5324 (class 0 OID 0)
-- Dependencies: 239
-- Name: factions_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.factions_id_seq', 5, true);


--
-- TOC entry 5325 (class 0 OID 0)
-- Dependencies: 242
-- Name: localisationsResources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."localisationsResources_id_seq"', 10, true);


--
-- TOC entry 5326 (class 0 OID 0)
-- Dependencies: 243
-- Name: localisations_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.localisations_id_seq', 8, true);


--
-- TOC entry 5327 (class 0 OID 0)
-- Dependencies: 245
-- Name: maintenanceCosts_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."maintenanceCosts_id_seq"', 10, true);


--
-- TOC entry 5328 (class 0 OID 0)
-- Dependencies: 248
-- Name: map_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.map_id_seq', 5, true);


--
-- TOC entry 5329 (class 0 OID 0)
-- Dependencies: 250
-- Name: modifiers_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.modifiers_id_seq', 1, false);


--
-- TOC entry 5330 (class 0 OID 0)
-- Dependencies: 252
-- Name: nations_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.nations_id_seq', 5, true);


--
-- TOC entry 5331 (class 0 OID 0)
-- Dependencies: 254
-- Name: offeredresources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.offeredresources_id_seq', 5, true);


--
-- TOC entry 5332 (class 0 OID 0)
-- Dependencies: 256
-- Name: ownedResources_Id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."ownedResources_Id_seq"', 45, true);


--
-- TOC entry 5333 (class 0 OID 0)
-- Dependencies: 258
-- Name: players_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.players_id_seq', 4, true);


--
-- TOC entry 5334 (class 0 OID 0)
-- Dependencies: 260
-- Name: populationproductionshares_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.populationproductionshares_id_seq', 8, true);


--
-- TOC entry 5335 (class 0 OID 0)
-- Dependencies: 262
-- Name: populations_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.populations_id_seq', 8, true);


--
-- TOC entry 5336 (class 0 OID 0)
-- Dependencies: 264
-- Name: populationusedresource_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.populationusedresource_id_seq', 8, true);


--
-- TOC entry 5337 (class 0 OID 0)
-- Dependencies: 266
-- Name: productionCost_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."productionCost_id_seq"', 10, true);


--
-- TOC entry 5338 (class 0 OID 0)
-- Dependencies: 268
-- Name: productionShares_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."productionShares_id_seq"', 10, true);


--
-- TOC entry 5339 (class 0 OID 0)
-- Dependencies: 270
-- Name: relatedEvents_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."relatedEvents_id_seq"', 10, true);


--
-- TOC entry 5340 (class 0 OID 0)
-- Dependencies: 272
-- Name: religions_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.religions_id_seq', 5, true);


--
-- TOC entry 5341 (class 0 OID 0)
-- Dependencies: 274
-- Name: resources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.resources_id_seq', 11, true);


--
-- TOC entry 5342 (class 0 OID 0)
-- Dependencies: 276
-- Name: socialgroups_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.socialgroups_id_seq', 5, true);


--
-- TOC entry 5343 (class 0 OID 0)
-- Dependencies: 278
-- Name: tradeagreements_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.tradeagreements_id_seq', 5, true);


--
-- TOC entry 5344 (class 0 OID 0)
-- Dependencies: 280
-- Name: troops_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.troops_id_seq', 8, true);


--
-- TOC entry 5345 (class 0 OID 0)
-- Dependencies: 282
-- Name: unitOrders_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."unitOrders_id_seq"', 12, true);


--
-- TOC entry 5346 (class 0 OID 0)
-- Dependencies: 284
-- Name: unitTypes_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."unitTypes_id_seq"', 5, true);


--
-- TOC entry 5347 (class 0 OID 0)
-- Dependencies: 286
-- Name: usedResources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."usedResources_id_seq"', 10, true);


--
-- TOC entry 5348 (class 0 OID 0)
-- Dependencies: 288
-- Name: wantedresources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.wantedresources_id_seq', 5, true);


--
-- TOC entry 4931 (class 2606 OID 75265)
-- Name: gameaccess PK_gameaccess; Type: CONSTRAINT; Schema: Global; Owner: postgres
--

ALTER TABLE ONLY "Global".gameaccess
    ADD CONSTRAINT "PK_gameaccess" PRIMARY KEY ("fk_Users", "fk_Games");


--
-- TOC entry 4935 (class 2606 OID 75267)
-- Name: games PK_games; Type: CONSTRAINT; Schema: Global; Owner: postgres
--

ALTER TABLE ONLY "Global".games
    ADD CONSTRAINT "PK_games" PRIMARY KEY (id);


--
-- TOC entry 4941 (class 2606 OID 75269)
-- Name: users PK_users; Type: CONSTRAINT; Schema: Global; Owner: postgres
--

ALTER TABLE ONLY "Global".users
    ADD CONSTRAINT "PK_users" PRIMARY KEY (id);


--
-- TOC entry 4937 (class 2606 OID 75271)
-- Name: refresh_tokens refresh_tokens_pkey; Type: CONSTRAINT; Schema: Global; Owner: postgres
--

ALTER TABLE ONLY "Global".refresh_tokens
    ADD CONSTRAINT refresh_tokens_pkey PRIMARY KEY (id);


--
-- TOC entry 4945 (class 2606 OID 75273)
-- Name: accessToUnits PK_accessToUnits; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."accessToUnits"
    ADD CONSTRAINT "PK_accessToUnits" PRIMARY KEY (id);


--
-- TOC entry 4949 (class 2606 OID 75275)
-- Name: accessestonations PK_accessestonations; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.accessestonations
    ADD CONSTRAINT "PK_accessestonations" PRIMARY KEY (id);


--
-- TOC entry 4952 (class 2606 OID 75277)
-- Name: actions PK_actions; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.actions
    ADD CONSTRAINT "PK_actions" PRIMARY KEY (id);


--
-- TOC entry 4956 (class 2606 OID 75279)
-- Name: armies PK_armies; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.armies
    ADD CONSTRAINT "PK_armies" PRIMARY KEY (id);


--
-- TOC entry 4958 (class 2606 OID 75281)
-- Name: cultures PK_cultures; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.cultures
    ADD CONSTRAINT "PK_cultures" PRIMARY KEY (id);


--
-- TOC entry 4960 (class 2606 OID 75283)
-- Name: events PK_events; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.events
    ADD CONSTRAINT "PK_events" PRIMARY KEY (id);


--
-- TOC entry 4963 (class 2606 OID 75285)
-- Name: factions PK_factions; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.factions
    ADD CONSTRAINT "PK_factions" PRIMARY KEY (id);


--
-- TOC entry 4966 (class 2606 OID 75287)
-- Name: localisations PK_localisations; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.localisations
    ADD CONSTRAINT "PK_localisations" PRIMARY KEY (id);


--
-- TOC entry 4968 (class 2606 OID 75289)
-- Name: localisationsResources PK_localisationsResources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."localisationsResources"
    ADD CONSTRAINT "PK_localisationsResources" PRIMARY KEY (id);


--
-- TOC entry 4972 (class 2606 OID 75291)
-- Name: maintenanceCosts PK_maintenanceCosts; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."maintenanceCosts"
    ADD CONSTRAINT "PK_maintenanceCosts" PRIMARY KEY (id);


--
-- TOC entry 4974 (class 2606 OID 75293)
-- Name: map PK_map; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.map
    ADD CONSTRAINT "PK_map" PRIMARY KEY (id);


--
-- TOC entry 4977 (class 2606 OID 75295)
-- Name: mapAccess PK_mapAccess; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."mapAccess"
    ADD CONSTRAINT "PK_mapAccess" PRIMARY KEY ("fk_Nations", "fk_Maps");


--
-- TOC entry 4981 (class 2606 OID 75297)
-- Name: nations PK_nations; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.nations
    ADD CONSTRAINT "PK_nations" PRIMARY KEY (id);


--
-- TOC entry 4985 (class 2606 OID 75299)
-- Name: offeredresources PK_offeredresources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.offeredresources
    ADD CONSTRAINT "PK_offeredresources" PRIMARY KEY (id);


--
-- TOC entry 4989 (class 2606 OID 75301)
-- Name: players PK_players; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.players
    ADD CONSTRAINT "PK_players" PRIMARY KEY (id);


--
-- TOC entry 4997 (class 2606 OID 75303)
-- Name: populations PK_populations; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "PK_populations" PRIMARY KEY (id);


--
-- TOC entry 5003 (class 2606 OID 75305)
-- Name: productionCost PK_productionCost; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionCost"
    ADD CONSTRAINT "PK_productionCost" PRIMARY KEY (id);


--
-- TOC entry 5005 (class 2606 OID 75307)
-- Name: productionShares PK_productionShares; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionShares"
    ADD CONSTRAINT "PK_productionShares" PRIMARY KEY (id);


--
-- TOC entry 5009 (class 2606 OID 75309)
-- Name: relatedEvents PK_relatedEvents; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."relatedEvents"
    ADD CONSTRAINT "PK_relatedEvents" PRIMARY KEY (id);


--
-- TOC entry 5011 (class 2606 OID 75311)
-- Name: religions PK_religions; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.religions
    ADD CONSTRAINT "PK_religions" PRIMARY KEY (id);


--
-- TOC entry 5013 (class 2606 OID 75313)
-- Name: resources PK_resources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.resources
    ADD CONSTRAINT "PK_resources" PRIMARY KEY (id);


--
-- TOC entry 5015 (class 2606 OID 75315)
-- Name: socialgroups PK_socialgroups; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.socialgroups
    ADD CONSTRAINT "PK_socialgroups" PRIMARY KEY (id);


--
-- TOC entry 5019 (class 2606 OID 75317)
-- Name: tradeagreements PK_tradeagreements; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.tradeagreements
    ADD CONSTRAINT "PK_tradeagreements" PRIMARY KEY (id);


--
-- TOC entry 5023 (class 2606 OID 75319)
-- Name: troops PK_troops; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.troops
    ADD CONSTRAINT "PK_troops" PRIMARY KEY (id);


--
-- TOC entry 5027 (class 2606 OID 75321)
-- Name: unitOrders PK_unitOrders; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."unitOrders"
    ADD CONSTRAINT "PK_unitOrders" PRIMARY KEY (id);


--
-- TOC entry 5029 (class 2606 OID 75323)
-- Name: unitTypes PK_unitTypes; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."unitTypes"
    ADD CONSTRAINT "PK_unitTypes" PRIMARY KEY (id);


--
-- TOC entry 5031 (class 2606 OID 75325)
-- Name: usedResources PK_usedResources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."usedResources"
    ADD CONSTRAINT "PK_usedResources" PRIMARY KEY (id);


--
-- TOC entry 5035 (class 2606 OID 75327)
-- Name: wantedresources PK_wantedresources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.wantedresources
    ADD CONSTRAINT "PK_wantedresources" PRIMARY KEY (id);


--
-- TOC entry 4979 (class 2606 OID 75329)
-- Name: modifiers modifiers_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.modifiers
    ADD CONSTRAINT modifiers_pkey PRIMARY KEY (id);


--
-- TOC entry 4987 (class 2606 OID 75331)
-- Name: ownedResources ownedResources_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."ownedResources"
    ADD CONSTRAINT "ownedResources_pkey" PRIMARY KEY (id);


--
-- TOC entry 4991 (class 2606 OID 75333)
-- Name: populationproductionshares populationproductionshares_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationproductionshares
    ADD CONSTRAINT populationproductionshares_pkey PRIMARY KEY (id);


--
-- TOC entry 4999 (class 2606 OID 75335)
-- Name: populationusedresource populationusedresource_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationusedresource
    ADD CONSTRAINT populationusedresource_pkey PRIMARY KEY (id);


--
-- TOC entry 4929 (class 1259 OID 75336)
-- Name: IX_gameaccess_fk_Games; Type: INDEX; Schema: Global; Owner: postgres
--

CREATE INDEX "IX_gameaccess_fk_Games" ON "Global".gameaccess USING btree ("fk_Games");


--
-- TOC entry 4932 (class 1259 OID 75337)
-- Name: IX_games_name; Type: INDEX; Schema: Global; Owner: postgres
--

CREATE UNIQUE INDEX "IX_games_name" ON "Global".games USING btree (name);


--
-- TOC entry 4933 (class 1259 OID 75338)
-- Name: IX_games_ownerId; Type: INDEX; Schema: Global; Owner: postgres
--

CREATE INDEX "IX_games_ownerId" ON "Global".games USING btree ("ownerId");


--
-- TOC entry 4938 (class 1259 OID 75339)
-- Name: IX_users_email; Type: INDEX; Schema: Global; Owner: postgres
--

CREATE UNIQUE INDEX "IX_users_email" ON "Global".users USING btree (email);


--
-- TOC entry 4939 (class 1259 OID 75340)
-- Name: IX_users_name; Type: INDEX; Schema: Global; Owner: postgres
--

CREATE UNIQUE INDEX "IX_users_name" ON "Global".users USING btree (name);


--
-- TOC entry 4942 (class 1259 OID 75341)
-- Name: IX_accessToUnits_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_accessToUnits_fk_Nations" ON game_1."accessToUnits" USING btree ("fk_Nation");


--
-- TOC entry 4943 (class 1259 OID 75342)
-- Name: IX_accessToUnits_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_accessToUnits_fk_UnitTypes" ON game_1."accessToUnits" USING btree ("fk_UnitTypes");


--
-- TOC entry 4946 (class 1259 OID 75343)
-- Name: IX_accessestonations_fk_nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_accessestonations_fk_nations" ON game_1.accessestonations USING btree (fk_nations);


--
-- TOC entry 4947 (class 1259 OID 75344)
-- Name: IX_accessestonations_fk_users; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_accessestonations_fk_users" ON game_1.accessestonations USING btree (fk_users);


--
-- TOC entry 4950 (class 1259 OID 75345)
-- Name: IX_actions_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_actions_fk_Nations" ON game_1.actions USING btree ("fk_Nations");


--
-- TOC entry 4953 (class 1259 OID 75346)
-- Name: IX_armies_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_armies_fk_Nations" ON game_1.armies USING btree ("fk_Nations");


--
-- TOC entry 4954 (class 1259 OID 75347)
-- Name: IX_armies_fk_localisations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_armies_fk_localisations" ON game_1.armies USING btree (fk_localisations);


--
-- TOC entry 4961 (class 1259 OID 75348)
-- Name: IX_factions_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_factions_fk_Nations" ON game_1.factions USING btree ("fk_Nations");


--
-- TOC entry 4964 (class 1259 OID 75349)
-- Name: IX_localisations_fk_nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_localisations_fk_nations" ON game_1.localisations USING btree (fk_nations);


--
-- TOC entry 4969 (class 1259 OID 75350)
-- Name: IX_maintenanceCosts_fk_Resources; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_maintenanceCosts_fk_Resources" ON game_1."maintenanceCosts" USING btree ("fk_Resources");


--
-- TOC entry 4970 (class 1259 OID 75351)
-- Name: IX_maintenanceCosts_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_maintenanceCosts_fk_UnitTypes" ON game_1."maintenanceCosts" USING btree ("fk_UnitTypes");


--
-- TOC entry 4975 (class 1259 OID 75352)
-- Name: IX_mapAccess_fk_Maps; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_mapAccess_fk_Maps" ON game_1."mapAccess" USING btree ("fk_Maps");


--
-- TOC entry 4982 (class 1259 OID 75353)
-- Name: IX_offeredresources_fk_resource; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_offeredresources_fk_resource" ON game_1.offeredresources USING btree (fk_resource);


--
-- TOC entry 4983 (class 1259 OID 75354)
-- Name: IX_offeredresources_fk_tradeagreement; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_offeredresources_fk_tradeagreement" ON game_1.offeredresources USING btree (fk_tradeagreement);


--
-- TOC entry 4992 (class 1259 OID 75355)
-- Name: IX_populations_fk_cultures; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_populations_fk_cultures" ON game_1.populations USING btree (fk_cultures);


--
-- TOC entry 4993 (class 1259 OID 75356)
-- Name: IX_populations_fk_localisations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_populations_fk_localisations" ON game_1.populations USING btree (fk_localisations);


--
-- TOC entry 4994 (class 1259 OID 75357)
-- Name: IX_populations_fk_religions; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_populations_fk_religions" ON game_1.populations USING btree (fk_religions);


--
-- TOC entry 4995 (class 1259 OID 75358)
-- Name: IX_populations_fk_socialgroups; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_populations_fk_socialgroups" ON game_1.populations USING btree (fk_socialgroups);


--
-- TOC entry 5000 (class 1259 OID 75359)
-- Name: IX_productionCost_fk_Resources; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_productionCost_fk_Resources" ON game_1."productionCost" USING btree ("fk_Resources");


--
-- TOC entry 5001 (class 1259 OID 75360)
-- Name: IX_productionCost_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_productionCost_fk_UnitTypes" ON game_1."productionCost" USING btree ("fk_UnitTypes");


--
-- TOC entry 5006 (class 1259 OID 75361)
-- Name: IX_relatedEvents_fk_Events; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_relatedEvents_fk_Events" ON game_1."relatedEvents" USING btree ("fk_Events");


--
-- TOC entry 5007 (class 1259 OID 75362)
-- Name: IX_relatedEvents_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_relatedEvents_fk_Nations" ON game_1."relatedEvents" USING btree ("fk_Nations");


--
-- TOC entry 5016 (class 1259 OID 75363)
-- Name: IX_tradeagreements_fk_nationoffering; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_tradeagreements_fk_nationoffering" ON game_1.tradeagreements USING btree (fk_nationoffering);


--
-- TOC entry 5017 (class 1259 OID 75364)
-- Name: IX_tradeagreements_fk_nationreceiving; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_tradeagreements_fk_nationreceiving" ON game_1.tradeagreements USING btree (fk_nationreceiving);


--
-- TOC entry 5020 (class 1259 OID 75365)
-- Name: IX_troops_fk_Armies; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_troops_fk_Armies" ON game_1.troops USING btree ("fk_Armies");


--
-- TOC entry 5021 (class 1259 OID 75366)
-- Name: IX_troops_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_troops_fk_UnitTypes" ON game_1.troops USING btree ("fk_UnitTypes");


--
-- TOC entry 5024 (class 1259 OID 75367)
-- Name: IX_unitOrders_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_unitOrders_fk_Nations" ON game_1."unitOrders" USING btree ("fk_Nations");


--
-- TOC entry 5025 (class 1259 OID 75368)
-- Name: IX_unitOrders_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_unitOrders_fk_UnitTypes" ON game_1."unitOrders" USING btree ("fk_UnitTypes");


--
-- TOC entry 5032 (class 1259 OID 75369)
-- Name: IX_wantedresources_fk_resource; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_wantedresources_fk_resource" ON game_1.wantedresources USING btree (fk_resource);


--
-- TOC entry 5033 (class 1259 OID 75370)
-- Name: IX_wantedresources_fk_tradeagreement; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_wantedresources_fk_tradeagreement" ON game_1.wantedresources USING btree (fk_tradeagreement);


--
-- TOC entry 5087 (class 2620 OID 75371)
-- Name: nations trg_after_nations_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_nations_insert AFTER INSERT ON game_1.nations FOR EACH ROW EXECUTE FUNCTION game_1.add_nation_to_all_resources();


--
-- TOC entry 5089 (class 2620 OID 75372)
-- Name: populations trg_after_populations_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_populations_insert AFTER INSERT ON game_1.populations FOR EACH ROW EXECUTE FUNCTION game_1.add_population_relations();


--
-- TOC entry 5090 (class 2620 OID 75373)
-- Name: productionShares trg_after_productionshares_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_productionshares_insert AFTER INSERT ON game_1."productionShares" FOR EACH ROW EXECUTE FUNCTION game_1.add_production_shares_to_populations();


--
-- TOC entry 5091 (class 2620 OID 75374)
-- Name: resources trg_after_resources_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_resources_insert AFTER INSERT ON game_1.resources FOR EACH ROW EXECUTE FUNCTION game_1.add_resource_to_all_nations();


--
-- TOC entry 5092 (class 2620 OID 75375)
-- Name: usedResources trg_after_usedresources_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_usedresources_insert AFTER INSERT ON game_1."usedResources" FOR EACH ROW EXECUTE FUNCTION game_1.add_used_resources_to_populations();


--
-- TOC entry 5088 (class 2620 OID 75376)
-- Name: nations trg_create_default_armies; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_create_default_armies AFTER INSERT ON game_1.nations FOR EACH ROW EXECUTE FUNCTION game_1.create_default_armies();


--
-- TOC entry 5036 (class 2606 OID 75377)
-- Name: gameaccess FK_gameaccess_games_fk_Games; Type: FK CONSTRAINT; Schema: Global; Owner: postgres
--

ALTER TABLE ONLY "Global".gameaccess
    ADD CONSTRAINT "FK_gameaccess_games_fk_Games" FOREIGN KEY ("fk_Games") REFERENCES "Global".games(id) ON DELETE CASCADE;


--
-- TOC entry 5037 (class 2606 OID 75382)
-- Name: gameaccess FK_gameaccess_users_fk_Users; Type: FK CONSTRAINT; Schema: Global; Owner: postgres
--

ALTER TABLE ONLY "Global".gameaccess
    ADD CONSTRAINT "FK_gameaccess_users_fk_Users" FOREIGN KEY ("fk_Users") REFERENCES "Global".users(id) ON DELETE CASCADE;


--
-- TOC entry 5038 (class 2606 OID 75387)
-- Name: games FK_games_users_ownerId; Type: FK CONSTRAINT; Schema: Global; Owner: postgres
--

ALTER TABLE ONLY "Global".games
    ADD CONSTRAINT "FK_games_users_ownerId" FOREIGN KEY ("ownerId") REFERENCES "Global".users(id) ON DELETE CASCADE;


--
-- TOC entry 5039 (class 2606 OID 75392)
-- Name: refresh_tokens refresh_tokens_user_id_fkey; Type: FK CONSTRAINT; Schema: Global; Owner: postgres
--

ALTER TABLE ONLY "Global".refresh_tokens
    ADD CONSTRAINT refresh_tokens_user_id_fkey FOREIGN KEY (user_id) REFERENCES "Global".users(id) ON DELETE CASCADE;


--
-- TOC entry 5040 (class 2606 OID 75397)
-- Name: accessToUnits FK_accessToUnits_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."accessToUnits"
    ADD CONSTRAINT "FK_accessToUnits_fk_Nations" FOREIGN KEY ("fk_Nation") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5041 (class 2606 OID 75402)
-- Name: accessToUnits FK_accessToUnits_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."accessToUnits"
    ADD CONSTRAINT "FK_accessToUnits_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5042 (class 2606 OID 75407)
-- Name: accessestonations FK_accessestonations_fk_nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.accessestonations
    ADD CONSTRAINT "FK_accessestonations_fk_nations" FOREIGN KEY (fk_nations) REFERENCES game_1.nations(id) ON DELETE CASCADE;


--
-- TOC entry 5043 (class 2606 OID 75412)
-- Name: accessestonations FK_accessestonations_fk_users; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.accessestonations
    ADD CONSTRAINT "FK_accessestonations_fk_users" FOREIGN KEY (fk_users) REFERENCES game_1.players(id) ON DELETE CASCADE;


--
-- TOC entry 5044 (class 2606 OID 75417)
-- Name: actions FK_actions_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.actions
    ADD CONSTRAINT "FK_actions_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5045 (class 2606 OID 75422)
-- Name: armies FK_armies_localisations_fk_localisations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.armies
    ADD CONSTRAINT "FK_armies_localisations_fk_localisations" FOREIGN KEY (fk_localisations) REFERENCES game_1.localisations(id) ON DELETE CASCADE;


--
-- TOC entry 5046 (class 2606 OID 75427)
-- Name: armies FK_armies_nations_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.armies
    ADD CONSTRAINT "FK_armies_nations_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE CASCADE;


--
-- TOC entry 5047 (class 2606 OID 75432)
-- Name: factions FK_factions_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.factions
    ADD CONSTRAINT "FK_factions_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5049 (class 2606 OID 75437)
-- Name: localisationsResources FK_localisationsResources_localisations_fk_localisations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."localisationsResources"
    ADD CONSTRAINT "FK_localisationsResources_localisations_fk_localisations" FOREIGN KEY (fk_localisations) REFERENCES game_1.localisations(id) ON DELETE CASCADE;


--
-- TOC entry 5050 (class 2606 OID 75442)
-- Name: localisationsResources FK_localisationsResources_resources_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."localisationsResources"
    ADD CONSTRAINT "FK_localisationsResources_resources_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5048 (class 2606 OID 75447)
-- Name: localisations FK_localisations_nations_fk_nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.localisations
    ADD CONSTRAINT "FK_localisations_nations_fk_nations" FOREIGN KEY (fk_nations) REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5051 (class 2606 OID 75452)
-- Name: maintenanceCosts FK_maintenanceCosts_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."maintenanceCosts"
    ADD CONSTRAINT "FK_maintenanceCosts_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5052 (class 2606 OID 75457)
-- Name: maintenanceCosts FK_maintenanceCosts_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."maintenanceCosts"
    ADD CONSTRAINT "FK_maintenanceCosts_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5053 (class 2606 OID 75462)
-- Name: mapAccess FK_mapAccess_map_fk_Maps; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."mapAccess"
    ADD CONSTRAINT "FK_mapAccess_map_fk_Maps" FOREIGN KEY ("fk_Maps") REFERENCES game_1.map(id) ON DELETE CASCADE;


--
-- TOC entry 5054 (class 2606 OID 75467)
-- Name: mapAccess FK_mapAccess_map_fk_Nation; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."mapAccess"
    ADD CONSTRAINT "FK_mapAccess_map_fk_Nation" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE CASCADE;


--
-- TOC entry 5056 (class 2606 OID 75472)
-- Name: nations FK_nations_players_fk_cultures; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.nations
    ADD CONSTRAINT "FK_nations_players_fk_cultures" FOREIGN KEY (fk_cultures) REFERENCES game_1.cultures(id) ON DELETE CASCADE;


--
-- TOC entry 5057 (class 2606 OID 75477)
-- Name: nations FK_nations_players_fk_religions; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.nations
    ADD CONSTRAINT "FK_nations_players_fk_religions" FOREIGN KEY (fk_religions) REFERENCES game_1.religions(id) ON DELETE RESTRICT;


--
-- TOC entry 5058 (class 2606 OID 75482)
-- Name: offeredresources FK_offeredresources_resources_fk_resource; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.offeredresources
    ADD CONSTRAINT "FK_offeredresources_resources_fk_resource" FOREIGN KEY (fk_resource) REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5059 (class 2606 OID 75487)
-- Name: offeredresources FK_offeredresources_tradeagreements_fk_tradeagreement; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.offeredresources
    ADD CONSTRAINT "FK_offeredresources_tradeagreements_fk_tradeagreement" FOREIGN KEY (fk_tradeagreement) REFERENCES game_1.tradeagreements(id) ON DELETE CASCADE;


--
-- TOC entry 5060 (class 2606 OID 75492)
-- Name: ownedResources FK_ownedresources_nation_fk_nation; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."ownedResources"
    ADD CONSTRAINT "FK_ownedresources_nation_fk_nation" FOREIGN KEY (fk_nation) REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5061 (class 2606 OID 75497)
-- Name: ownedResources FK_ownedresources_resource_fk_resource; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."ownedResources"
    ADD CONSTRAINT "FK_ownedresources_resource_fk_resource" FOREIGN KEY (fk_resource) REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5062 (class 2606 OID 75502)
-- Name: players FK_players_users_fk_users; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.players
    ADD CONSTRAINT "FK_players_users_fk_users" FOREIGN KEY ("fk_User") REFERENCES "Global".users(id) ON DELETE CASCADE;


--
-- TOC entry 5065 (class 2606 OID 75507)
-- Name: populations FK_populations_cultures_fk_cultures; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "FK_populations_cultures_fk_cultures" FOREIGN KEY (fk_cultures) REFERENCES game_1.cultures(id) ON DELETE CASCADE;


--
-- TOC entry 5066 (class 2606 OID 75512)
-- Name: populations FK_populations_localisations_fk_localisations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "FK_populations_localisations_fk_localisations" FOREIGN KEY (fk_localisations) REFERENCES game_1.localisations(id) ON DELETE CASCADE;


--
-- TOC entry 5067 (class 2606 OID 75517)
-- Name: populations FK_populations_religions_fk_religions; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "FK_populations_religions_fk_religions" FOREIGN KEY (fk_religions) REFERENCES game_1.religions(id) ON DELETE RESTRICT;


--
-- TOC entry 5068 (class 2606 OID 75522)
-- Name: populations FK_populations_socialgroups_fk_socialgroups; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "FK_populations_socialgroups_fk_socialgroups" FOREIGN KEY (fk_socialgroups) REFERENCES game_1.socialgroups(id) ON DELETE CASCADE;


--
-- TOC entry 5071 (class 2606 OID 75527)
-- Name: productionCost FK_productionCost_resources_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionCost"
    ADD CONSTRAINT "FK_productionCost_resources_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5072 (class 2606 OID 75532)
-- Name: productionCost FK_productionCost_unitTypes_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionCost"
    ADD CONSTRAINT "FK_productionCost_unitTypes_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5073 (class 2606 OID 75537)
-- Name: productionShares FK_productionShares_populations_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionShares"
    ADD CONSTRAINT "FK_productionShares_populations_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5074 (class 2606 OID 75542)
-- Name: productionShares FK_productionShares_populations_fk_SocialGroups; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionShares"
    ADD CONSTRAINT "FK_productionShares_populations_fk_SocialGroups" FOREIGN KEY ("fk_SocialGroups") REFERENCES game_1.socialgroups(id) ON DELETE CASCADE;


--
-- TOC entry 5075 (class 2606 OID 75547)
-- Name: relatedEvents FK_relatedEvents_events_fk_Events; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."relatedEvents"
    ADD CONSTRAINT "FK_relatedEvents_events_fk_Events" FOREIGN KEY ("fk_Events") REFERENCES game_1.events(id) ON DELETE CASCADE;


--
-- TOC entry 5076 (class 2606 OID 75552)
-- Name: relatedEvents FK_relatedEvents_nations_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."relatedEvents"
    ADD CONSTRAINT "FK_relatedEvents_nations_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5077 (class 2606 OID 75557)
-- Name: tradeagreements FK_tradeagreements_nations_fk_nationoffering; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.tradeagreements
    ADD CONSTRAINT "FK_tradeagreements_nations_fk_nationoffering" FOREIGN KEY (fk_nationoffering) REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5078 (class 2606 OID 75562)
-- Name: tradeagreements FK_tradeagreements_nations_fk_nationreceiving; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.tradeagreements
    ADD CONSTRAINT "FK_tradeagreements_nations_fk_nationreceiving" FOREIGN KEY (fk_nationreceiving) REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5079 (class 2606 OID 75567)
-- Name: troops FK_troops_armies_fk_Armies; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.troops
    ADD CONSTRAINT "FK_troops_armies_fk_Armies" FOREIGN KEY ("fk_Armies") REFERENCES game_1.armies(id) ON DELETE CASCADE;


--
-- TOC entry 5080 (class 2606 OID 75572)
-- Name: troops FK_troops_unitTypes_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.troops
    ADD CONSTRAINT "FK_troops_unitTypes_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5081 (class 2606 OID 75577)
-- Name: unitOrders FK_unitOrders_nations_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."unitOrders"
    ADD CONSTRAINT "FK_unitOrders_nations_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5082 (class 2606 OID 75582)
-- Name: unitOrders FK_unitOrders_unitTypes_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."unitOrders"
    ADD CONSTRAINT "FK_unitOrders_unitTypes_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5083 (class 2606 OID 75587)
-- Name: usedResources FK_usedResources_populations_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."usedResources"
    ADD CONSTRAINT "FK_usedResources_populations_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5084 (class 2606 OID 75592)
-- Name: usedResources FK_usedResources_populations_fk_SocialGroups; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."usedResources"
    ADD CONSTRAINT "FK_usedResources_populations_fk_SocialGroups" FOREIGN KEY ("fk_SocialGroups") REFERENCES game_1.socialgroups(id) ON DELETE CASCADE;


--
-- TOC entry 5085 (class 2606 OID 75597)
-- Name: wantedresources FK_wantedresources_resources_fk_resource; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.wantedresources
    ADD CONSTRAINT "FK_wantedresources_resources_fk_resource" FOREIGN KEY (fk_resource) REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5086 (class 2606 OID 75602)
-- Name: wantedresources FK_wantedresources_tradeagreements_fk_tradeagreement; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.wantedresources
    ADD CONSTRAINT "FK_wantedresources_tradeagreements_fk_tradeagreement" FOREIGN KEY (fk_tradeagreement) REFERENCES game_1.tradeagreements(id) ON DELETE CASCADE;


--
-- TOC entry 5055 (class 2606 OID 75607)
-- Name: modifiers modifiers_event_id_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.modifiers
    ADD CONSTRAINT modifiers_event_id_fkey FOREIGN KEY (event_id) REFERENCES game_1.events(id) ON DELETE CASCADE;


--
-- TOC entry 5063 (class 2606 OID 75612)
-- Name: populationproductionshares populationproductionshares_fkpopulation_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationproductionshares
    ADD CONSTRAINT populationproductionshares_fkpopulation_fkey FOREIGN KEY (fk_population) REFERENCES game_1.populations(id) ON DELETE CASCADE;


--
-- TOC entry 5064 (class 2606 OID 75617)
-- Name: populationproductionshares populationproductionshares_fkresources_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationproductionshares
    ADD CONSTRAINT populationproductionshares_fkresources_fkey FOREIGN KEY (fk_resources) REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5069 (class 2606 OID 75622)
-- Name: populationusedresource populationusedresource_fkpopulation_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationusedresource
    ADD CONSTRAINT populationusedresource_fkpopulation_fkey FOREIGN KEY (fk_population) REFERENCES game_1.populations(id) ON DELETE CASCADE;


--
-- TOC entry 5070 (class 2606 OID 75627)
-- Name: populationusedresource populationusedresource_fkresources_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationusedresource
    ADD CONSTRAINT populationusedresource_fkresources_fkey FOREIGN KEY (fk_resources) REFERENCES game_1.resources(id) ON DELETE CASCADE;


-- Completed on 2025-12-05 21:40:20

--
-- PostgreSQL database dump complete
--


