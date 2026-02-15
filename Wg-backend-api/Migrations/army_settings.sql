--
-- PostgreSQL database dump
--

\restrict 3B5fK0bY8oGjOZyA93WtZzYOTNLGeXQNcAeZv7m3Wsqkwto0JytgOQ5VMZLdThr

-- Dumped from database version 17.6
-- Dumped by pg_dump version 18.0

-- Started on 2026-02-15 19:26:45

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

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 289 (class 1259 OID 96636)
-- Name: armySettings; Type: TABLE; Schema: game_1; Owner: postgres
--

CREATE TABLE game_1."armySettings" (
    id integer NOT NULL,
    "nameOfSettingsSet" text,
    "useMeleeAtack" boolean DEFAULT true NOT NULL,
    "useRangeAtack" boolean DEFAULT true NOT NULL,
    "useDefense" boolean DEFAULT true NOT NULL,
    "useSpeed" boolean DEFAULT true NOT NULL,
    "useMorale" boolean DEFAULT true NOT NULL,
    "useMaintanace" boolean DEFAULT true NOT NULL
);


ALTER TABLE game_1."armySettings" OWNER TO postgres;

--
-- TOC entry 290 (class 1259 OID 96643)
-- Name: army_settings_id_seq; Type: SEQUENCE; Schema: game_1; Owner: postgres
--

ALTER TABLE game_1."armySettings" ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME game_1.army_settings_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 10000
    CACHE 1
);


--
-- TOC entry 5044 (class 0 OID 96636)
-- Dependencies: 289
-- Data for Name: armySettings; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."armySettings" (id, "nameOfSettingsSet", "useMeleeAtack", "useRangeAtack", "useDefense", "useSpeed", "useMorale", "useMaintanace") FROM stdin;
1	Default	t	t	t	t	t	t
\.


--
-- TOC entry 5051 (class 0 OID 0)
-- Dependencies: 290
-- Name: army_settings_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.army_settings_id_seq', 1, true);


--
-- TOC entry 4898 (class 2606 OID 96642)
-- Name: armySettings army_settings_pkey; Type: CONSTRAINT; Schema: game_1; Owner: postgres
--

ALTER TABLE ONLY game_1."armySettings"
    ADD CONSTRAINT army_settings_pkey PRIMARY KEY (id);


-- Completed on 2026-02-15 19:26:45

--
-- PostgreSQL database dump complete
--

\unrestrict 3B5fK0bY8oGjOZyA93WtZzYOTNLGeXQNcAeZv7m3Wsqkwto0JytgOQ5VMZLdThr

