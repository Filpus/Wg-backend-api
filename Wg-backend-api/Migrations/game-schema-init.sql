--
-- PostgreSQL database dump
--

\restrict nLEDlEc7vb7uaJ1fdL3jIyaEkxrcOruNbcuEQMHU5Ko8Hvf5ksvMLg3yTsar032

-- Dumped from database version 17.6
-- Dumped by pg_dump version 17.6

-- Started on 2025-12-04 14:23:03

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
-- TOC entry 7 (class 2615 OID 71499)
-- Name: game_1; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA game_1;


ALTER SCHEMA game_1 OWNER TO postgres;

--
-- TOC entry 289 (class 1255 OID 71500)
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
-- TOC entry 290 (class 1255 OID 71501)
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
-- TOC entry 291 (class 1255 OID 71502)
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
-- TOC entry 292 (class 1255 OID 71503)
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
-- TOC entry 293 (class 1255 OID 71504)
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


--
-- Name: create_default_armies(); Type: FUNCTION; Schema: game_1; Owner: -
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

ALTER FUNCTION game_1.add_used_resources_to_populations() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 226 (class 1259 OID 71529)
-- Name: accessToUnits; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."accessToUnits" (
    id integer NOT NULL,
    "fk_Nation" integer NOT NULL,
    "fk_UnitTypes" integer NOT NULL
);


ALTER TABLE game_1."accessToUnits" OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 71532)
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
-- TOC entry 228 (class 1259 OID 71533)
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
-- TOC entry 229 (class 1259 OID 71536)
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
-- TOC entry 230 (class 1259 OID 71537)
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
-- TOC entry 231 (class 1259 OID 71542)
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
-- TOC entry 232 (class 1259 OID 71543)
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
-- TOC entry 233 (class 1259 OID 71548)
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
-- TOC entry 234 (class 1259 OID 71549)
-- Name: cultures; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.cultures (
    id integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE game_1.cultures OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 71554)
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
-- TOC entry 236 (class 1259 OID 71555)
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
-- TOC entry 237 (class 1259 OID 71560)
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
-- TOC entry 238 (class 1259 OID 71561)
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
-- TOC entry 239 (class 1259 OID 71566)
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
-- TOC entry 240 (class 1259 OID 71567)
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
-- TOC entry 241 (class 1259 OID 71572)
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
-- TOC entry 242 (class 1259 OID 71575)
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
-- TOC entry 243 (class 1259 OID 71576)
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
-- TOC entry 244 (class 1259 OID 71577)
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
-- TOC entry 245 (class 1259 OID 71580)
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
-- TOC entry 246 (class 1259 OID 71581)
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
-- TOC entry 247 (class 1259 OID 71586)
-- Name: mapAccess; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."mapAccess" (
    "fk_Nations" integer NOT NULL,
    "fk_Maps" integer NOT NULL
);


ALTER TABLE game_1."mapAccess" OWNER TO postgres;

--
-- TOC entry 248 (class 1259 OID 71589)
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
-- TOC entry 249 (class 1259 OID 71590)
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
-- TOC entry 250 (class 1259 OID 71595)
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
-- TOC entry 5217 (class 0 OID 0)
-- Dependencies: 250
-- Name: modifiers_id_seq; Type: SEQUENCE OWNED BY; Schema: game_1; Owner: postgres
--

ALTER SEQUENCE game_1.modifiers_id_seq OWNED BY game_1.modifiers.id;


--
-- TOC entry 251 (class 1259 OID 71596)
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
-- TOC entry 252 (class 1259 OID 71601)
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
-- TOC entry 253 (class 1259 OID 71602)
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
-- TOC entry 254 (class 1259 OID 71605)
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
-- TOC entry 255 (class 1259 OID 71606)
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
-- TOC entry 256 (class 1259 OID 71610)
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
-- TOC entry 5218 (class 0 OID 0)
-- Dependencies: 256
-- Name: ownedResources_Id_seq; Type: SEQUENCE OWNED BY; Schema: game_1; Owner: postgres
--

ALTER SEQUENCE game_1."ownedResources_Id_seq" OWNED BY game_1."ownedResources".id;


--
-- TOC entry 257 (class 1259 OID 71611)
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
-- TOC entry 258 (class 1259 OID 71616)
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
-- TOC entry 259 (class 1259 OID 71617)
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
-- TOC entry 260 (class 1259 OID 71620)
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
-- TOC entry 261 (class 1259 OID 71621)
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
-- TOC entry 262 (class 1259 OID 71624)
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
-- TOC entry 263 (class 1259 OID 71625)
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
-- TOC entry 264 (class 1259 OID 71628)
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
-- TOC entry 265 (class 1259 OID 71629)
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
-- TOC entry 266 (class 1259 OID 71632)
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
-- TOC entry 267 (class 1259 OID 71633)
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
-- TOC entry 268 (class 1259 OID 71636)
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
-- TOC entry 269 (class 1259 OID 71637)
-- Name: relatedEvents; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."relatedEvents" (
    id integer NOT NULL,
    "fk_Events" integer NOT NULL,
    "fk_Nations" integer NOT NULL
);


ALTER TABLE game_1."relatedEvents" OWNER TO postgres;

--
-- TOC entry 270 (class 1259 OID 71640)
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
-- TOC entry 271 (class 1259 OID 71641)
-- Name: religions; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1.religions (
    id integer NOT NULL,
    name text NOT NULL,
    icon text
);


ALTER TABLE game_1.religions OWNER TO postgres;

--
-- TOC entry 272 (class 1259 OID 71646)
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
-- TOC entry 273 (class 1259 OID 71647)
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
-- TOC entry 274 (class 1259 OID 71652)
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
-- TOC entry 275 (class 1259 OID 71653)
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
-- TOC entry 276 (class 1259 OID 71658)
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
-- TOC entry 277 (class 1259 OID 71659)
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
-- TOC entry 278 (class 1259 OID 71664)
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
-- TOC entry 279 (class 1259 OID 71665)
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
-- TOC entry 280 (class 1259 OID 71668)
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
-- TOC entry 281 (class 1259 OID 71669)
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
-- TOC entry 282 (class 1259 OID 71672)
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
-- TOC entry 283 (class 1259 OID 71673)
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
-- TOC entry 284 (class 1259 OID 71678)
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
-- TOC entry 285 (class 1259 OID 71679)
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
-- TOC entry 286 (class 1259 OID 71682)
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
-- TOC entry 287 (class 1259 OID 71683)
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
-- TOC entry 288 (class 1259 OID 71686)
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
-- TOC entry 4918 (class 2604 OID 71687)
-- Name: modifiers id; Type: DEFAULT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.modifiers ALTER COLUMN id SET DEFAULT nextval('game_1.modifiers_id_seq'::regclass);


--
-- TOC entry 4919 (class 2604 OID 71688)
-- Name: ownedResources id; Type: DEFAULT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."ownedResources" ALTER COLUMN id SET DEFAULT nextval('game_1."ownedResources_Id_seq"'::regclass);


--
-- TOC entry 4924 (class 2606 OID 71698)
-- Name: accessToUnits PK_accessToUnits; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."accessToUnits"
    ADD CONSTRAINT "PK_accessToUnits" PRIMARY KEY (id);


--
-- TOC entry 4928 (class 2606 OID 71700)
-- Name: accessestonations PK_accessestonations; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.accessestonations
    ADD CONSTRAINT "PK_accessestonations" PRIMARY KEY (id);


--
-- TOC entry 4931 (class 2606 OID 71702)
-- Name: actions PK_actions; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.actions
    ADD CONSTRAINT "PK_actions" PRIMARY KEY (id);


--
-- TOC entry 4935 (class 2606 OID 71704)
-- Name: armies PK_armies; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.armies
    ADD CONSTRAINT "PK_armies" PRIMARY KEY (id);


--
-- TOC entry 4937 (class 2606 OID 71706)
-- Name: cultures PK_cultures; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.cultures
    ADD CONSTRAINT "PK_cultures" PRIMARY KEY (id);


--
-- TOC entry 4939 (class 2606 OID 71708)
-- Name: events PK_events; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.events
    ADD CONSTRAINT "PK_events" PRIMARY KEY (id);


--
-- TOC entry 4942 (class 2606 OID 71710)
-- Name: factions PK_factions; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.factions
    ADD CONSTRAINT "PK_factions" PRIMARY KEY (id);


--
-- TOC entry 4945 (class 2606 OID 71712)
-- Name: localisations PK_localisations; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.localisations
    ADD CONSTRAINT "PK_localisations" PRIMARY KEY (id);


--
-- TOC entry 4947 (class 2606 OID 71714)
-- Name: localisationsResources PK_localisationsResources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."localisationsResources"
    ADD CONSTRAINT "PK_localisationsResources" PRIMARY KEY (id);


--
-- TOC entry 4951 (class 2606 OID 71716)
-- Name: maintenanceCosts PK_maintenanceCosts; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."maintenanceCosts"
    ADD CONSTRAINT "PK_maintenanceCosts" PRIMARY KEY (id);


--
-- TOC entry 4953 (class 2606 OID 71718)
-- Name: map PK_map; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.map
    ADD CONSTRAINT "PK_map" PRIMARY KEY (id);


--
-- TOC entry 4956 (class 2606 OID 71720)
-- Name: mapAccess PK_mapAccess; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."mapAccess"
    ADD CONSTRAINT "PK_mapAccess" PRIMARY KEY ("fk_Nations", "fk_Maps");


--
-- TOC entry 4960 (class 2606 OID 71722)
-- Name: nations PK_nations; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.nations
    ADD CONSTRAINT "PK_nations" PRIMARY KEY (id);


--
-- TOC entry 4964 (class 2606 OID 71724)
-- Name: offeredresources PK_offeredresources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.offeredresources
    ADD CONSTRAINT "PK_offeredresources" PRIMARY KEY (id);


--
-- TOC entry 4968 (class 2606 OID 71726)
-- Name: players PK_players; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.players
    ADD CONSTRAINT "PK_players" PRIMARY KEY (id);


--
-- TOC entry 4976 (class 2606 OID 71728)
-- Name: populations PK_populations; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "PK_populations" PRIMARY KEY (id);


--
-- TOC entry 4982 (class 2606 OID 71730)
-- Name: productionCost PK_productionCost; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionCost"
    ADD CONSTRAINT "PK_productionCost" PRIMARY KEY (id);


--
-- TOC entry 4984 (class 2606 OID 71732)
-- Name: productionShares PK_productionShares; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionShares"
    ADD CONSTRAINT "PK_productionShares" PRIMARY KEY (id);


--
-- TOC entry 4988 (class 2606 OID 71734)
-- Name: relatedEvents PK_relatedEvents; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."relatedEvents"
    ADD CONSTRAINT "PK_relatedEvents" PRIMARY KEY (id);


--
-- TOC entry 4990 (class 2606 OID 71736)
-- Name: religions PK_religions; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.religions
    ADD CONSTRAINT "PK_religions" PRIMARY KEY (id);


--
-- TOC entry 4992 (class 2606 OID 71738)
-- Name: resources PK_resources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.resources
    ADD CONSTRAINT "PK_resources" PRIMARY KEY (id);


--
-- TOC entry 4994 (class 2606 OID 71740)
-- Name: socialgroups PK_socialgroups; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.socialgroups
    ADD CONSTRAINT "PK_socialgroups" PRIMARY KEY (id);


--
-- TOC entry 4998 (class 2606 OID 71742)
-- Name: tradeagreements PK_tradeagreements; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.tradeagreements
    ADD CONSTRAINT "PK_tradeagreements" PRIMARY KEY (id);


--
-- TOC entry 5002 (class 2606 OID 71744)
-- Name: troops PK_troops; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.troops
    ADD CONSTRAINT "PK_troops" PRIMARY KEY (id);


--
-- TOC entry 5006 (class 2606 OID 71746)
-- Name: unitOrders PK_unitOrders; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."unitOrders"
    ADD CONSTRAINT "PK_unitOrders" PRIMARY KEY (id);


--
-- TOC entry 5008 (class 2606 OID 71748)
-- Name: unitTypes PK_unitTypes; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."unitTypes"
    ADD CONSTRAINT "PK_unitTypes" PRIMARY KEY (id);


--
-- TOC entry 5010 (class 2606 OID 71750)
-- Name: usedResources PK_usedResources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."usedResources"
    ADD CONSTRAINT "PK_usedResources" PRIMARY KEY (id);


--
-- TOC entry 5014 (class 2606 OID 71752)
-- Name: wantedresources PK_wantedresources; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.wantedresources
    ADD CONSTRAINT "PK_wantedresources" PRIMARY KEY (id);


--
-- TOC entry 4958 (class 2606 OID 71754)
-- Name: modifiers modifiers_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.modifiers
    ADD CONSTRAINT modifiers_pkey PRIMARY KEY (id);


--
-- TOC entry 4966 (class 2606 OID 71756)
-- Name: ownedResources ownedResources_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."ownedResources"
    ADD CONSTRAINT "ownedResources_pkey" PRIMARY KEY (id);


--
-- TOC entry 4970 (class 2606 OID 71758)
-- Name: populationproductionshares populationproductionshares_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationproductionshares
    ADD CONSTRAINT populationproductionshares_pkey PRIMARY KEY (id);


--
-- TOC entry 4978 (class 2606 OID 71760)
-- Name: populationusedresource populationusedresource_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationusedresource
    ADD CONSTRAINT populationusedresource_pkey PRIMARY KEY (id);


--
-- TOC entry 4921 (class 1259 OID 71766)
-- Name: IX_accessToUnits_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_accessToUnits_fk_Nations" ON game_1."accessToUnits" USING btree ("fk_Nation");


--
-- TOC entry 4922 (class 1259 OID 71767)
-- Name: IX_accessToUnits_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_accessToUnits_fk_UnitTypes" ON game_1."accessToUnits" USING btree ("fk_UnitTypes");


--
-- TOC entry 4925 (class 1259 OID 71768)
-- Name: IX_accessestonations_fk_nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_accessestonations_fk_nations" ON game_1.accessestonations USING btree (fk_nations);


--
-- TOC entry 4926 (class 1259 OID 71769)
-- Name: IX_accessestonations_fk_users; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_accessestonations_fk_users" ON game_1.accessestonations USING btree (fk_users);


--
-- TOC entry 4929 (class 1259 OID 71770)
-- Name: IX_actions_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_actions_fk_Nations" ON game_1.actions USING btree ("fk_Nations");


--
-- TOC entry 4932 (class 1259 OID 71771)
-- Name: IX_armies_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_armies_fk_Nations" ON game_1.armies USING btree ("fk_Nations");


--
-- TOC entry 4933 (class 1259 OID 71772)
-- Name: IX_armies_fk_localisations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_armies_fk_localisations" ON game_1.armies USING btree (fk_localisations);


--
-- TOC entry 4940 (class 1259 OID 71773)
-- Name: IX_factions_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_factions_fk_Nations" ON game_1.factions USING btree ("fk_Nations");


--
-- TOC entry 4943 (class 1259 OID 71774)
-- Name: IX_localisations_fk_nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_localisations_fk_nations" ON game_1.localisations USING btree (fk_nations);


--
-- TOC entry 4948 (class 1259 OID 71775)
-- Name: IX_maintenanceCosts_fk_Resources; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_maintenanceCosts_fk_Resources" ON game_1."maintenanceCosts" USING btree ("fk_Resources");


--
-- TOC entry 4949 (class 1259 OID 71776)
-- Name: IX_maintenanceCosts_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_maintenanceCosts_fk_UnitTypes" ON game_1."maintenanceCosts" USING btree ("fk_UnitTypes");


--
-- TOC entry 4954 (class 1259 OID 71777)
-- Name: IX_mapAccess_fk_Maps; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_mapAccess_fk_Maps" ON game_1."mapAccess" USING btree ("fk_Maps");


--
-- TOC entry 4961 (class 1259 OID 71778)
-- Name: IX_offeredresources_fk_resource; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_offeredresources_fk_resource" ON game_1.offeredresources USING btree (fk_resource);


--
-- TOC entry 4962 (class 1259 OID 71779)
-- Name: IX_offeredresources_fk_tradeagreement; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_offeredresources_fk_tradeagreement" ON game_1.offeredresources USING btree (fk_tradeagreement);


--
-- TOC entry 4971 (class 1259 OID 71780)
-- Name: IX_populations_fk_cultures; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_populations_fk_cultures" ON game_1.populations USING btree (fk_cultures);


--
-- TOC entry 4972 (class 1259 OID 71781)
-- Name: IX_populations_fk_localisations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_populations_fk_localisations" ON game_1.populations USING btree (fk_localisations);


--
-- TOC entry 4973 (class 1259 OID 71782)
-- Name: IX_populations_fk_religions; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_populations_fk_religions" ON game_1.populations USING btree (fk_religions);


--
-- TOC entry 4974 (class 1259 OID 71783)
-- Name: IX_populations_fk_socialgroups; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_populations_fk_socialgroups" ON game_1.populations USING btree (fk_socialgroups);


--
-- TOC entry 4979 (class 1259 OID 71784)
-- Name: IX_productionCost_fk_Resources; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_productionCost_fk_Resources" ON game_1."productionCost" USING btree ("fk_Resources");


--
-- TOC entry 4980 (class 1259 OID 71785)
-- Name: IX_productionCost_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_productionCost_fk_UnitTypes" ON game_1."productionCost" USING btree ("fk_UnitTypes");


--
-- TOC entry 4985 (class 1259 OID 71786)
-- Name: IX_relatedEvents_fk_Events; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_relatedEvents_fk_Events" ON game_1."relatedEvents" USING btree ("fk_Events");


--
-- TOC entry 4986 (class 1259 OID 71787)
-- Name: IX_relatedEvents_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_relatedEvents_fk_Nations" ON game_1."relatedEvents" USING btree ("fk_Nations");


--
-- TOC entry 4995 (class 1259 OID 71788)
-- Name: IX_tradeagreements_fk_nationoffering; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_tradeagreements_fk_nationoffering" ON game_1.tradeagreements USING btree (fk_nationoffering);


--
-- TOC entry 4996 (class 1259 OID 71789)
-- Name: IX_tradeagreements_fk_nationreceiving; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_tradeagreements_fk_nationreceiving" ON game_1.tradeagreements USING btree (fk_nationreceiving);


--
-- TOC entry 4999 (class 1259 OID 71790)
-- Name: IX_troops_fk_Armies; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_troops_fk_Armies" ON game_1.troops USING btree ("fk_Armies");


--
-- TOC entry 5000 (class 1259 OID 71791)
-- Name: IX_troops_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_troops_fk_UnitTypes" ON game_1.troops USING btree ("fk_UnitTypes");


--
-- TOC entry 5003 (class 1259 OID 71792)
-- Name: IX_unitOrders_fk_Nations; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_unitOrders_fk_Nations" ON game_1."unitOrders" USING btree ("fk_Nations");


--
-- TOC entry 5004 (class 1259 OID 71793)
-- Name: IX_unitOrders_fk_UnitTypes; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_unitOrders_fk_UnitTypes" ON game_1."unitOrders" USING btree ("fk_UnitTypes");


--
-- TOC entry 5011 (class 1259 OID 71794)
-- Name: IX_wantedresources_fk_resource; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_wantedresources_fk_resource" ON game_1.wantedresources USING btree (fk_resource);


--
-- TOC entry 5012 (class 1259 OID 71795)
-- Name: IX_wantedresources_fk_tradeagreement; Type: INDEX; Schema: game_1; Owner: postgres
--

CREATE INDEX "IX_wantedresources_fk_tradeagreement" ON game_1.wantedresources USING btree (fk_tradeagreement);


--
-- TOC entry 5062 (class 2620 OID 71796)
-- Name: nations trg_after_nations_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_nations_insert AFTER INSERT ON game_1.nations FOR EACH ROW EXECUTE FUNCTION game_1.add_nation_to_all_resources();


--
-- TOC entry 5063 (class 2620 OID 71797)
-- Name: populations trg_after_populations_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_populations_insert AFTER INSERT ON game_1.populations FOR EACH ROW EXECUTE FUNCTION game_1.add_population_relations();


--
-- TOC entry 5064 (class 2620 OID 71798)
-- Name: productionShares trg_after_productionshares_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_productionshares_insert AFTER INSERT ON game_1."productionShares" FOR EACH ROW EXECUTE FUNCTION game_1.add_production_shares_to_populations();


--
-- TOC entry 5065 (class 2620 OID 71799)
-- Name: resources trg_after_resources_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_resources_insert AFTER INSERT ON game_1.resources FOR EACH ROW EXECUTE FUNCTION game_1.add_resource_to_all_nations();


--
-- TOC entry 5066 (class 2620 OID 71800)
-- Name: usedResources trg_after_usedresources_insert; Type: TRIGGER; Schema: game_1; Owner: postgres
--

CREATE TRIGGER trg_after_usedresources_insert AFTER INSERT ON game_1."usedResources" FOR EACH ROW EXECUTE FUNCTION game_1.add_used_resources_to_populations();


--
-- Name: nations trg_create_default_armies; Type: TRIGGER; Schema: game_1; Owner: -
--

CREATE TRIGGER trg_create_default_armies AFTER INSERT ON game_1.nations FOR EACH ROW EXECUTE FUNCTION game_1.create_default_armies();


--
-- Name: accessToUnits FK_accessToUnits_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: -
-- TOC entry 5015 (class 2606 OID 71821)
-- Name: accessToUnits FK_accessToUnits_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."accessToUnits"
    ADD CONSTRAINT "FK_accessToUnits_fk_Nations" FOREIGN KEY ("fk_Nation") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5016 (class 2606 OID 71826)
-- Name: accessToUnits FK_accessToUnits_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."accessToUnits"
    ADD CONSTRAINT "FK_accessToUnits_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5017 (class 2606 OID 71831)
-- Name: accessestonations FK_accessestonations_fk_nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.accessestonations
    ADD CONSTRAINT "FK_accessestonations_fk_nations" FOREIGN KEY (fk_nations) REFERENCES game_1.nations(id) ON DELETE CASCADE;


--
-- TOC entry 5018 (class 2606 OID 71836)
-- Name: accessestonations FK_accessestonations_fk_users; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.accessestonations
    ADD CONSTRAINT "FK_accessestonations_fk_users" FOREIGN KEY (fk_users) REFERENCES game_1.players(id) ON DELETE CASCADE;


--
-- TOC entry 5019 (class 2606 OID 71841)
-- Name: actions FK_actions_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.actions
    ADD CONSTRAINT "FK_actions_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5020 (class 2606 OID 71846)
-- Name: armies FK_armies_localisations_fk_localisations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.armies
    ADD CONSTRAINT "FK_armies_localisations_fk_localisations" FOREIGN KEY (fk_localisations) REFERENCES game_1.localisations(id) ON DELETE CASCADE;


--
-- TOC entry 5021 (class 2606 OID 71851)
-- Name: armies FK_armies_nations_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.armies
    ADD CONSTRAINT "FK_armies_nations_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE CASCADE;


--
-- TOC entry 5022 (class 2606 OID 71856)
-- Name: factions FK_factions_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.factions
    ADD CONSTRAINT "FK_factions_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5024 (class 2606 OID 71861)
-- Name: localisationsResources FK_localisationsResources_localisations_fk_localisations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."localisationsResources"
    ADD CONSTRAINT "FK_localisationsResources_localisations_fk_localisations" FOREIGN KEY (fk_localisations) REFERENCES game_1.localisations(id) ON DELETE CASCADE;


--
-- TOC entry 5025 (class 2606 OID 71866)
-- Name: localisationsResources FK_localisationsResources_resources_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."localisationsResources"
    ADD CONSTRAINT "FK_localisationsResources_resources_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5023 (class 2606 OID 71871)
-- Name: localisations FK_localisations_nations_fk_nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.localisations
    ADD CONSTRAINT "FK_localisations_nations_fk_nations" FOREIGN KEY (fk_nations) REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5026 (class 2606 OID 71876)
-- Name: maintenanceCosts FK_maintenanceCosts_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."maintenanceCosts"
    ADD CONSTRAINT "FK_maintenanceCosts_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5027 (class 2606 OID 71881)
-- Name: maintenanceCosts FK_maintenanceCosts_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."maintenanceCosts"
    ADD CONSTRAINT "FK_maintenanceCosts_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5028 (class 2606 OID 71886)
-- Name: mapAccess FK_mapAccess_map_fk_Maps; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."mapAccess"
    ADD CONSTRAINT "FK_mapAccess_map_fk_Maps" FOREIGN KEY ("fk_Maps") REFERENCES game_1.map(id) ON DELETE CASCADE;


--
-- TOC entry 5029 (class 2606 OID 71891)
-- Name: mapAccess FK_mapAccess_map_fk_Nation; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."mapAccess"
    ADD CONSTRAINT "FK_mapAccess_map_fk_Nation" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE CASCADE;


--
-- TOC entry 5031 (class 2606 OID 71896)
-- Name: nations FK_nations_players_fk_cultures; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.nations
    ADD CONSTRAINT "FK_nations_players_fk_cultures" FOREIGN KEY (fk_cultures) REFERENCES game_1.cultures(id) ON DELETE CASCADE;


--
-- TOC entry 5032 (class 2606 OID 71901)
-- Name: nations FK_nations_players_fk_religions; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.nations
    ADD CONSTRAINT "FK_nations_players_fk_religions" FOREIGN KEY (fk_religions) REFERENCES game_1.religions(id) ON DELETE RESTRICT;


--
-- TOC entry 5033 (class 2606 OID 71906)
-- Name: offeredresources FK_offeredresources_resources_fk_resource; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.offeredresources
    ADD CONSTRAINT "FK_offeredresources_resources_fk_resource" FOREIGN KEY (fk_resource) REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5034 (class 2606 OID 71911)
-- Name: offeredresources FK_offeredresources_tradeagreements_fk_tradeagreement; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.offeredresources
    ADD CONSTRAINT "FK_offeredresources_tradeagreements_fk_tradeagreement" FOREIGN KEY (fk_tradeagreement) REFERENCES game_1.tradeagreements(id) ON DELETE CASCADE;


--
-- TOC entry 5035 (class 2606 OID 71916)
-- Name: ownedResources FK_ownedresources_nation_fk_nation; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."ownedResources"
    ADD CONSTRAINT "FK_ownedresources_nation_fk_nation" FOREIGN KEY (fk_nation) REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5036 (class 2606 OID 71921)
-- Name: ownedResources FK_ownedresources_resource_fk_resource; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."ownedResources"
    ADD CONSTRAINT "FK_ownedresources_resource_fk_resource" FOREIGN KEY (fk_resource) REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5037 (class 2606 OID 71926)
-- Name: players FK_players_users_fk_users; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.players
    ADD CONSTRAINT "FK_players_users_fk_users" FOREIGN KEY ("fk_User") REFERENCES "Global".users(id) ON DELETE CASCADE;


--
-- TOC entry 5040 (class 2606 OID 71931)
-- Name: populations FK_populations_cultures_fk_cultures; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "FK_populations_cultures_fk_cultures" FOREIGN KEY (fk_cultures) REFERENCES game_1.cultures(id) ON DELETE CASCADE;


--
-- TOC entry 5041 (class 2606 OID 71936)
-- Name: populations FK_populations_localisations_fk_localisations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "FK_populations_localisations_fk_localisations" FOREIGN KEY (fk_localisations) REFERENCES game_1.localisations(id) ON DELETE CASCADE;


--
-- TOC entry 5042 (class 2606 OID 71941)
-- Name: populations FK_populations_religions_fk_religions; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "FK_populations_religions_fk_religions" FOREIGN KEY (fk_religions) REFERENCES game_1.religions(id) ON DELETE RESTRICT;


--
-- TOC entry 5043 (class 2606 OID 71946)
-- Name: populations FK_populations_socialgroups_fk_socialgroups; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populations
    ADD CONSTRAINT "FK_populations_socialgroups_fk_socialgroups" FOREIGN KEY (fk_socialgroups) REFERENCES game_1.socialgroups(id) ON DELETE CASCADE;


--
-- TOC entry 5046 (class 2606 OID 71951)
-- Name: productionCost FK_productionCost_resources_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionCost"
    ADD CONSTRAINT "FK_productionCost_resources_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5047 (class 2606 OID 71956)
-- Name: productionCost FK_productionCost_unitTypes_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionCost"
    ADD CONSTRAINT "FK_productionCost_unitTypes_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5048 (class 2606 OID 71961)
-- Name: productionShares FK_productionShares_populations_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionShares"
    ADD CONSTRAINT "FK_productionShares_populations_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5049 (class 2606 OID 71966)
-- Name: productionShares FK_productionShares_populations_fk_SocialGroups; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."productionShares"
    ADD CONSTRAINT "FK_productionShares_populations_fk_SocialGroups" FOREIGN KEY ("fk_SocialGroups") REFERENCES game_1.socialgroups(id) ON DELETE CASCADE;


--
-- TOC entry 5050 (class 2606 OID 71971)
-- Name: relatedEvents FK_relatedEvents_events_fk_Events; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."relatedEvents"
    ADD CONSTRAINT "FK_relatedEvents_events_fk_Events" FOREIGN KEY ("fk_Events") REFERENCES game_1.events(id) ON DELETE CASCADE;


--
-- TOC entry 5051 (class 2606 OID 71976)
-- Name: relatedEvents FK_relatedEvents_nations_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."relatedEvents"
    ADD CONSTRAINT "FK_relatedEvents_nations_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5052 (class 2606 OID 71981)
-- Name: tradeagreements FK_tradeagreements_nations_fk_nationoffering; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.tradeagreements
    ADD CONSTRAINT "FK_tradeagreements_nations_fk_nationoffering" FOREIGN KEY (fk_nationoffering) REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5053 (class 2606 OID 71986)
-- Name: tradeagreements FK_tradeagreements_nations_fk_nationreceiving; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.tradeagreements
    ADD CONSTRAINT "FK_tradeagreements_nations_fk_nationreceiving" FOREIGN KEY (fk_nationreceiving) REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5054 (class 2606 OID 71991)
-- Name: troops FK_troops_armies_fk_Armies; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.troops
    ADD CONSTRAINT "FK_troops_armies_fk_Armies" FOREIGN KEY ("fk_Armies") REFERENCES game_1.armies(id) ON DELETE CASCADE;


--
-- TOC entry 5055 (class 2606 OID 71996)
-- Name: troops FK_troops_unitTypes_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.troops
    ADD CONSTRAINT "FK_troops_unitTypes_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5056 (class 2606 OID 72001)
-- Name: unitOrders FK_unitOrders_nations_fk_Nations; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."unitOrders"
    ADD CONSTRAINT "FK_unitOrders_nations_fk_Nations" FOREIGN KEY ("fk_Nations") REFERENCES game_1.nations(id) ON DELETE RESTRICT;


--
-- TOC entry 5057 (class 2606 OID 72006)
-- Name: unitOrders FK_unitOrders_unitTypes_fk_UnitTypes; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."unitOrders"
    ADD CONSTRAINT "FK_unitOrders_unitTypes_fk_UnitTypes" FOREIGN KEY ("fk_UnitTypes") REFERENCES game_1."unitTypes"(id) ON DELETE CASCADE;


--
-- TOC entry 5058 (class 2606 OID 72011)
-- Name: usedResources FK_usedResources_populations_fk_Resources; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."usedResources"
    ADD CONSTRAINT "FK_usedResources_populations_fk_Resources" FOREIGN KEY ("fk_Resources") REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5059 (class 2606 OID 72016)
-- Name: usedResources FK_usedResources_populations_fk_SocialGroups; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."usedResources"
    ADD CONSTRAINT "FK_usedResources_populations_fk_SocialGroups" FOREIGN KEY ("fk_SocialGroups") REFERENCES game_1.socialgroups(id) ON DELETE CASCADE;


--
-- TOC entry 5060 (class 2606 OID 72021)
-- Name: wantedresources FK_wantedresources_resources_fk_resource; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.wantedresources
    ADD CONSTRAINT "FK_wantedresources_resources_fk_resource" FOREIGN KEY (fk_resource) REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5061 (class 2606 OID 72026)
-- Name: wantedresources FK_wantedresources_tradeagreements_fk_tradeagreement; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.wantedresources
    ADD CONSTRAINT "FK_wantedresources_tradeagreements_fk_tradeagreement" FOREIGN KEY (fk_tradeagreement) REFERENCES game_1.tradeagreements(id) ON DELETE CASCADE;


--
-- TOC entry 5030 (class 2606 OID 72031)
-- Name: modifiers modifiers_event_id_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.modifiers
    ADD CONSTRAINT modifiers_event_id_fkey FOREIGN KEY (event_id) REFERENCES game_1.events(id) ON DELETE CASCADE;


--
-- TOC entry 5038 (class 2606 OID 72036)
-- Name: populationproductionshares populationproductionshares_fkpopulation_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationproductionshares
    ADD CONSTRAINT populationproductionshares_fkpopulation_fkey FOREIGN KEY (fk_population) REFERENCES game_1.populations(id) ON DELETE CASCADE;


--
-- TOC entry 5039 (class 2606 OID 72041)
-- Name: populationproductionshares populationproductionshares_fkresources_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationproductionshares
    ADD CONSTRAINT populationproductionshares_fkresources_fkey FOREIGN KEY (fk_resources) REFERENCES game_1.resources(id) ON DELETE CASCADE;


--
-- TOC entry 5044 (class 2606 OID 72046)
-- Name: populationusedresource populationusedresource_fkpopulation_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationusedresource
    ADD CONSTRAINT populationusedresource_fkpopulation_fkey FOREIGN KEY (fk_population) REFERENCES game_1.populations(id) ON DELETE CASCADE;


--
-- TOC entry 5045 (class 2606 OID 72051)
-- Name: populationusedresource populationusedresource_fkresources_fkey; Type: FK CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1.populationusedresource
    ADD CONSTRAINT populationusedresource_fkresources_fkey FOREIGN KEY (fk_resources) REFERENCES game_1.resources(id) ON DELETE CASCADE;


-- Completed on 2025-12-04 14:23:03

--
-- PostgreSQL database dump complete
--

\unrestrict nLEDlEc7vb7uaJ1fdL3jIyaEkxrcOruNbcuEQMHU5Ko8Hvf5ksvMLg3yTsar032

