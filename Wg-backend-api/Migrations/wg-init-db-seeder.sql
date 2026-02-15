--
-- PostgreSQL database dump
--

\restrict ZxTD8dqHsw7TgfrfoxTgjkmt9vL1IjtC8cqhG88dx8WEBROKsb2zZqWBnWf45du

-- Dumped from database version 17.6
-- Dumped by pg_dump version 17.6

-- Started on 2026-02-15 17:04:07

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
-- TOC entry 5568 (class 0 OID 96082)
-- Dependencies: 220
-- Data for Name: gameaccess; Type: TABLE DATA; Schema: Global; Owner: postgres
--

COPY "Global".gameaccess (id, "fk_Users", "fk_Games", "accessType", "isArchived", "nationName") FROM stdin;
1	1	1	1	f	\N
2	2	1	0	f	\N
3	3	1	1	f	\N
4	4	1	1	f	\N
\.


--
-- TOC entry 5570 (class 0 OID 96086)
-- Dependencies: 222
-- Data for Name: games; Type: TABLE DATA; Schema: Global; Owner: postgres
--

COPY "Global".games (id, name, description, image, "ownerId", game_code) FROM stdin;
1	default_schema	Demo testowe\r\n	\N	2	XIWDFW
2	Tesfs	TEst	\N	5	JLI1OA
\.


--
-- TOC entry 5572 (class 0 OID 96093)
-- Dependencies: 224
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: Global; Owner: postgres
--

COPY "Global".refresh_tokens (id, user_id, token, expires_at, revoked_at, created_at) FROM stdin;
0a546c55-37d3-4698-8b15-3e77ae1d47c9	1	ZoHowE/spkY/fjWQznZA9rWuprh+5R/6RTuK37c1tkIMxqfMPeGDFkh3F3t4jyyDWMHMM2ntYxmBPlFicRkRVQ==	2026-02-22 13:08:57.823879+01	\N	2026-02-15 13:08:57.823844+01
7f20a253-bea2-4756-a7eb-4f79e599d7ca	2	AmClTQJYVm0o4bjvaXwb+Q+nXwse+06WABSm9oGinxXlKr07VmWkxgpcu/V/W3jAHOh4GBSYP+11OQwYSpYbqg==	2026-02-22 16:49:52.361271+01	\N	2026-02-15 16:49:52.361271+01
\.


--
-- TOC entry 5573 (class 0 OID 96100)
-- Dependencies: 225
-- Data for Name: users; Type: TABLE DATA; Schema: Global; Owner: postgres
--

COPY "Global".users (id, name, email, password, issso, isarchived, image) FROM stdin;
1	Test	test@test	$2a$11$s/J0zefb5amFzjdjllmuZ.AXuziyjVRcYTeEhxyemaxsJJyKnzxU2	f	f	\N
2	admin	admin@admin	$2a$11$t8DGLO5spPxXzpyRb5j0vuuk54ycsFEo9scO7xswpGCH9WvMcwive	f	f	\N
3	tomek	tomek@tomek	$2a$11$t8DGLO5spPxXzpyRb5j0vuuk54ycsFEo9scO7xswpGCH9WvMcwive	f	f	\N
4	jakub	jakub@jakub	$2a$11$t8DGLO5spPxXzpyRb5j0vuuk54ycsFEo9scO7xswpGCH9WvMcwive	f	f	\N
5	Teste	272731@student.pwr.edu.pl	$2a$11$4QPd9xbLTZr4uq8GZ2tH3.6ij.AWGiJApJZAkCQPmBHZdPWq1Wd8G	t	f	\N
\.


--
-- TOC entry 5575 (class 0 OID 96106)
-- Dependencies: 227
-- Data for Name: accessToUnits; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") FROM stdin;
1	1	1
2	1	2
3	1	3
4	2	1
5	2	3
6	2	4
7	3	1
8	3	5
9	4	1
10	4	2
11	5	1
12	5	3
\.


--
-- TOC entry 5577 (class 0 OID 96110)
-- Dependencies: 229
-- Data for Name: accessestonations; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.accessestonations (id, fk_nations, fk_users, dateacquired, isactive) FROM stdin;
1	1	1	2025-01-01 00:00:00+01	t
2	2	2	2025-01-02 00:00:00+01	t
\.


--
-- TOC entry 5579 (class 0 OID 96114)
-- Dependencies: 231
-- Data for Name: actions; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.actions (id, "fk_Nations", name, description, result, "isSettled") FROM stdin;
1	1	Ekspedycja	Wysłanie ekspedycji na niezbadane tereny	\N	f
2	2	Budowa Świątyni	Rozpoczęcie budowy wielkiej świątyni	\N	f
3	3	Szlak Handlowy	Otwarcie nowego szlaku handlowego	Zwiększenie przychodów o 10%	t
4	4	Reformy	Wprowadzenie reform społecznych	\N	f
5	5	Mobilizacja	Mobilizacja sił zbrojnych	Wzrost liczebności armii o 20%	t
\.


--
-- TOC entry 5581 (class 0 OID 96120)
-- Dependencies: 233
-- Data for Name: armies; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.armies (id, name, "fk_Nations", fk_localisations, is_naval) FROM stdin;
1	Armia Północy	1	1	f
2	Legiony Cesarskie	2	2	f
3	Flota Republiki	3	3	t
4	Drużyna Księcia	4	4	f
5	Jeźdźcy Pustyni	5	5	f
6	Baraki	1	\N	f
7	Baraki	2	\N	f
8	Baraki	3	\N	f
9	Baraki	4	\N	f
10	Baraki	5	\N	f
11	Doki	1	\N	t
12	Doki	2	\N	t
13	Doki	3	\N	t
14	Doki	4	\N	t
15	Doki	5	\N	t
\.


--
-- TOC entry 5638 (class 0 OID 96636)
-- Dependencies: 290
-- Data for Name: armySettings; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."armySettings" (id, "nameOfSettingsSet", "useMeleeAtack", "useRangeAtack", "useDefense", "useSpeed", "useMorale", "useMaintanace") FROM stdin;
1	Default	t	t	t	t	t	t
\.


--
-- TOC entry 5583 (class 0 OID 96126)
-- Dependencies: 235
-- Data for Name: cultures; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.cultures (id, name) FROM stdin;
1	Nordycka
2	Słowiańska
3	Germańska
4	Romańska
5	Grecka
\.


--
-- TOC entry 5585 (class 0 OID 96132)
-- Dependencies: 237
-- Data for Name: events; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.events (id, name, description, isactive, picture) FROM stdin;
1	Wielka Bitwa	Epiczna bitwa, która zmieniła losy świata	t	battle.jpg
2	Plaga	Śmiertelna zaraza dziesiątkująca populację	t	plague.jpg
3	Odkrycie	Odkrycie nowych terenów i technologii	t	discovery.jpg
4	Rewolta	Rewolta społeczeństwa przeciwko władcy	t	revolt.jpg
5	Sojusz	Zawarcie sojuszu między narodami	t	alliance.jpg
\.


--
-- TOC entry 5587 (class 0 OID 96138)
-- Dependencies: 239
-- Data for Name: factions; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.factions (id, name, "fk_Nations", power, agenda, contentment, color, description) FROM stdin;
1	Konserwatyści	1	70	Utrzymanie tradycji	60	#0000FF	\N
2	Reformatorzy	1	30	Wprowadzenie zmian	40	#00FF00	\N
3	Militaryści	2	60	Ekspansja militarna	50	#FF0000	\N
4	Handlarze	2	40	Rozwój handlu	70	#FFFF00	\N
5	Zjednoczeni	3	90	Jedność narodu	80	#800080	\N
\.


--
-- TOC entry 5589 (class 0 OID 96144)
-- Dependencies: 241
-- Data for Name: localisations; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.localisations (id, name, size, fortifications, fk_nations) FROM stdin;
1	Stolica Północy	5	4	1
2	Twierdza Cesarska	6	5	2
3	Port Republiki	4	3	3
4	Wschodni Gród	3	2	4
5	Oaza Południowa	4	3	5
6	Górska Osada	2	1	1
7	Cesarskie Tereny	3	2	2
8	Nadmorska Wioska	2	1	3
\.


--
-- TOC entry 5590 (class 0 OID 96149)
-- Dependencies: 242
-- Data for Name: localisationsResources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."localisationsResources" (id, fk_localisations, "fk_Resources", amount) FROM stdin;
1	1	1	1000
2	1	3	2000
3	2	2	1500
4	2	5	3000
5	3	1	800
6	3	8	500
7	4	3	2500
8	4	4	3500
9	5	7	600
10	5	6	400
\.


--
-- TOC entry 5593 (class 0 OID 96154)
-- Dependencies: 245
-- Data for Name: maintenanceCosts; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) FROM stdin;
1	1	1	0.5
2	1	4	1
3	2	1	0.3
4	2	3	0.5
5	3	1	1
6	3	4	1.5
7	4	1	2
8	4	3	1
9	5	1	3
10	5	3	2
\.


--
-- TOC entry 5595 (class 0 OID 96158)
-- Dependencies: 247
-- Data for Name: map; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.map (id, name, "mapLocation", "mapIconLocation") FROM stdin;
1	Europa Środkowa	maps/central_europe.jpg	maps/central_europe.jpg
2	Wyspy Brytyjskie	maps/british_isles.jpg	maps/central_europe.jpg
3	Półwysep Iberyjski	maps/iberia.jpg	maps/central_europe.jpg
4	Skandynawia	maps/scandinavia.jpg	maps/central_europe.jpg
5	Bałkany	maps/balkans.jpg	maps/central_europe.jpg
\.


--
-- TOC entry 5596 (class 0 OID 96163)
-- Dependencies: 248
-- Data for Name: mapAccess; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."mapAccess" ("fk_Nations", "fk_Maps") FROM stdin;
1	1
1	4
2	2
2	5
3	3
4	1
4	5
\.


--
-- TOC entry 5598 (class 0 OID 96167)
-- Dependencies: 250
-- Data for Name: modifiers; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.modifiers (id, event_id, modifier_type, effects) FROM stdin;
\.


--
-- TOC entry 5600 (class 0 OID 96173)
-- Dependencies: 252
-- Data for Name: nations; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.nations (id, name, fk_religions, fk_cultures, flag, color) FROM stdin;
1	Królestwo Północy	1	1	\N	red
2	Cesarstwo Centralne	2	3	\N	yellow
3	Republika Nadmorska	2	4	\N	red
4	Księstwo Wschodnie	1	2	\N	green
5	Kalifat Południowy	3	5	\N	blue
\.


--
-- TOC entry 5602 (class 0 OID 96179)
-- Dependencies: 254
-- Data for Name: offeredresources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.offeredresources (id, fk_resource, fk_tradeagreement, quantity) FROM stdin;
1	2	1	100
2	3	1	200
3	1	3	50
4	4	3	300
5	3	4	150
\.


--
-- TOC entry 5604 (class 0 OID 96183)
-- Dependencies: 256
-- Data for Name: ownedResources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."ownedResources" (id, fk_nation, fk_resource, amount) FROM stdin;
1	1	1	4201
2	1	2	4202
3	1	3	4203
4	1	4	4204
5	1	5	4205
6	1	6	4206
7	1	7	4207
8	1	8	4208
9	1	9	4209
10	2	1	2137
11	2	2	2173
12	2	3	1237
13	2	4	2137
14	2	5	1273
15	2	6	2137
16	2	7	1237
17	2	8	2173
18	2	9	1237
19	3	1	4201
20	3	2	4202
21	3	3	4203
22	3	4	4204
23	3	5	4205
24	3	6	4206
25	3	7	4207
26	3	8	4208
27	3	9	4209
28	4	1	2137
29	4	2	2173
30	4	3	1237
31	4	4	2137
32	4	5	1273
33	4	6	2137
34	4	7	1237
35	4	8	2173
36	4	9	1237
37	5	1	2137
38	5	2	2173
39	5	3	1237
40	5	4	2137
41	5	5	1273
42	5	6	2137
43	5	7	1237
44	5	8	2173
45	5	9	1237
\.


--
-- TOC entry 5606 (class 0 OID 96188)
-- Dependencies: 258
-- Data for Name: players; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.players (id, "fk_User", "playerType", name) FROM stdin;
1	1	1	Test
2	2	0	admin
3	3	1	tomek
4	4	1	jakub
\.


--
-- TOC entry 5608 (class 0 OID 96194)
-- Dependencies: 260
-- Data for Name: populationproductionshares; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.populationproductionshares (id, fk_population, fk_resources, coefficient) FROM stdin;
1	1	1	1.1
2	1	2	2.1
3	2	1	3.7
4	3	3	6.9
5	4	4	1.2
6	5	5	1.2
7	6	6	0.9
8	7	7	1.3
9	7	1	1
10	7	7	0.5
\.


--
-- TOC entry 5610 (class 0 OID 96198)
-- Dependencies: 262
-- Data for Name: populations; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) FROM stdin;
1	1	1	1	1	5.5	3
2	2	3	2	2	6	3
3	2	4	3	3	7.2	1
4	1	2	1	4	4.8	2
5	3	5	5	5	5.7	2
6	1	1	2	6	5.2	1
7	2	3	4	7	6.8	1
8	2	4	5	8	6.3	1
\.


--
-- TOC entry 5612 (class 0 OID 96202)
-- Dependencies: 264
-- Data for Name: populationusedresource; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.populationusedresource (id, fk_population, fk_resources, amount) FROM stdin;
1	1	1	0.1
2	1	2	1.1
3	1	3	2.7
4	3	3	0.9
5	4	4	1.2
6	5	5	1.4
7	6	6	0.7
8	7	7	1.3
9	7	4	30
\.


--
-- TOC entry 5614 (class 0 OID 96206)
-- Dependencies: 266
-- Data for Name: productionCost; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) FROM stdin;
1	1	1	10
2	1	2	5
3	2	1	8
4	2	3	10
5	3	1	20
6	3	2	15
7	4	1	30
8	4	3	25
9	5	1	50
10	5	2	30
\.


--
-- TOC entry 5616 (class 0 OID 96210)
-- Dependencies: 268
-- Data for Name: productionShares; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) FROM stdin;
1	1	3	2
2	1	4	3
3	2	1	1.5
4	2	6	2
5	3	2	1
6	3	5	1.5
9	5	1	2.5
10	5	8	2
11	4	1	1
12	4	7	0.5
\.


--
-- TOC entry 5618 (class 0 OID 96214)
-- Dependencies: 270
-- Data for Name: relatedEvents; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."relatedEvents" (id, "fk_Events", "fk_Nations") FROM stdin;
1	1	1
2	1	2
3	2	3
4	2	4
5	3	1
6	3	5
7	4	2
8	4	4
9	5	1
10	5	3
\.


--
-- TOC entry 5620 (class 0 OID 96218)
-- Dependencies: 272
-- Data for Name: religions; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.religions (id, name, icon) FROM stdin;
1	Pogaństwo	\N
2	Chrześcijaństwo	\N
3	Islam	\N
4	Judaizm	\N
5	Zoroastrianizm	\N
\.


--
-- TOC entry 5622 (class 0 OID 96224)
-- Dependencies: 274
-- Data for Name: resources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.resources (id, name, ismain, icon, "constProduction") FROM stdin;
1	Złoto	t	\N	f
2	Żelazo	t	\N	f
3	Drewno	t	\N	f
4	Żywność	t	\N	f
5	Kamień	t	\N	f
6	Tkaniny	f	\N	f
7	Przyprawy	f	\N	f
8	Wino	f	\N	f
9	Drewno	t	\N	f
11	Miód	t	\N	f
\.


--
-- TOC entry 5624 (class 0 OID 96230)
-- Dependencies: 276
-- Data for Name: socialgroups; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.socialgroups (id, name, basehappiness, volunteers, icon) FROM stdin;
1	Chłopi	5	10	\N
2	Mieszczanie	6	20	\N
3	Szlachta	7	30	\N
5	Kupcy	6.5	15	\N
4	Duchowieństwo	8	5	\N
\.


--
-- TOC entry 5626 (class 0 OID 96236)
-- Dependencies: 278
-- Data for Name: tradeagreements; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.tradeagreements (id, fk_nationoffering, fk_nationreceiving, status, duration, description) FROM stdin;
1	1	2	0	10	I am description
2	1	3	0	5	I am also description
3	2	4	3	8	But I m not description
4	3	5	1	12	What about me?
5	4	5	2	6	
\.


--
-- TOC entry 5628 (class 0 OID 96242)
-- Dependencies: 280
-- Data for Name: troops; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.troops (id, "fk_UnitTypes", "fk_Armies", quantity) FROM stdin;
1	1	1	500
2	2	1	300
3	3	2	400
4	1	2	600
5	5	3	20
6	1	4	300
7	3	4	150
10	3	5	4
15	3	10	120
18	3	10	120
\.


--
-- TOC entry 5630 (class 0 OID 96246)
-- Dependencies: 282
-- Data for Name: unitOrders; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) FROM stdin;
1	1	1	1
2	1	2	1
3	3	2	3
4	4	2	1
5	1	3	8
6	5	3	5
7	1	4	2
8	2	4	4
9	1	5	1
10	3	5	4
12	2	1	1
\.


--
-- TOC entry 5632 (class 0 OID 96250)
-- Dependencies: 284
-- Data for Name: unitTypes; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."unitTypes" (id, name, description, melee, range, defense, speed, morale, "volunteersNeeded", "isNaval") FROM stdin;
1	Piechota	Podstawowa jednostka piechoty	5	0	3	3	5	100	f
2	Łucznicy	Jednostka łuczników	1	6	2	3	4	80	f
3	Kawaleria	Szybka jednostka kawalerii	7	0	4	6	7	120	f
4	Oblężnicza	Machiny oblężnicze	1	8	1	2	3	150	f
5	Okręty wojenne	Okręty bojowe	6	4	5	4	6	200	t
\.


--
-- TOC entry 5634 (class 0 OID 96256)
-- Dependencies: 286
-- Data for Name: usedResources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) FROM stdin;
1	1	4	100
2	1	3	50
3	2	4	75
4	2	6	25
5	3	4	50
6	3	8	30
9	5	4	60
10	5	6	40
11	4	4	30
\.


--
-- TOC entry 5636 (class 0 OID 96260)
-- Dependencies: 288
-- Data for Name: wantedresources; Type: TABLE DATA; Schema: game_1; Owner: postgres
--

COPY game_1.wantedresources (id, fk_resource, fk_tradeagreement, amount) FROM stdin;
1	1	1	50
2	4	1	100
3	2	3	75
4	5	3	200
5	7	4	25
\.


--
-- TOC entry 5640 (class 0 OID 96657)
-- Dependencies: 292
-- Data for Name: accessToUnits; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."accessToUnits" (id, "fk_Nation", "fk_UnitTypes") FROM stdin;
\.


--
-- TOC entry 5642 (class 0 OID 96661)
-- Dependencies: 294
-- Data for Name: accessestonations; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.accessestonations (id, fk_nations, fk_users, dateacquired, isactive) FROM stdin;
\.


--
-- TOC entry 5644 (class 0 OID 96665)
-- Dependencies: 296
-- Data for Name: actions; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.actions (id, "fk_Nations", name, description, result, "isSettled") FROM stdin;
\.


--
-- TOC entry 5646 (class 0 OID 96671)
-- Dependencies: 298
-- Data for Name: armies; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.armies (id, name, "fk_Nations", fk_localisations, is_naval) FROM stdin;
\.


--
-- TOC entry 5648 (class 0 OID 96677)
-- Dependencies: 300
-- Data for Name: cultures; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.cultures (id, name) FROM stdin;
\.


--
-- TOC entry 5650 (class 0 OID 96683)
-- Dependencies: 302
-- Data for Name: events; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.events (id, name, description, isactive, picture) FROM stdin;
\.


--
-- TOC entry 5652 (class 0 OID 96689)
-- Dependencies: 304
-- Data for Name: factions; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.factions (id, name, "fk_Nations", power, agenda, contentment, color, description) FROM stdin;
\.


--
-- TOC entry 5654 (class 0 OID 96695)
-- Dependencies: 306
-- Data for Name: localisations; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.localisations (id, name, size, fortifications, fk_nations) FROM stdin;
\.


--
-- TOC entry 5655 (class 0 OID 96700)
-- Dependencies: 307
-- Data for Name: localisationsResources; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."localisationsResources" (id, fk_localisations, "fk_Resources", amount) FROM stdin;
\.


--
-- TOC entry 5658 (class 0 OID 96705)
-- Dependencies: 310
-- Data for Name: maintenanceCosts; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."maintenanceCosts" (id, "fk_UnitTypes", "fk_Resources", amount) FROM stdin;
\.


--
-- TOC entry 5660 (class 0 OID 96709)
-- Dependencies: 312
-- Data for Name: map; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.map (id, name, "mapLocation", "mapIconLocation") FROM stdin;
\.


--
-- TOC entry 5661 (class 0 OID 96714)
-- Dependencies: 313
-- Data for Name: mapAccess; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."mapAccess" ("fk_Nations", "fk_Maps") FROM stdin;
\.


--
-- TOC entry 5663 (class 0 OID 96718)
-- Dependencies: 315
-- Data for Name: modifiers; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.modifiers (id, event_id, modifier_type, effects) FROM stdin;
\.


--
-- TOC entry 5665 (class 0 OID 96724)
-- Dependencies: 317
-- Data for Name: nations; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.nations (id, name, fk_religions, fk_cultures, flag, color) FROM stdin;
\.


--
-- TOC entry 5667 (class 0 OID 96730)
-- Dependencies: 319
-- Data for Name: offeredresources; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.offeredresources (id, fk_resource, fk_tradeagreement, quantity) FROM stdin;
\.


--
-- TOC entry 5669 (class 0 OID 96734)
-- Dependencies: 321
-- Data for Name: ownedResources; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."ownedResources" (id, fk_nation, fk_resource, amount) FROM stdin;
\.


--
-- TOC entry 5671 (class 0 OID 96739)
-- Dependencies: 323
-- Data for Name: players; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.players (id, "fk_User", "playerType", name) FROM stdin;
\.


--
-- TOC entry 5673 (class 0 OID 96745)
-- Dependencies: 325
-- Data for Name: populationproductionshares; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.populationproductionshares (id, fk_population, fk_resources, coefficient) FROM stdin;
\.


--
-- TOC entry 5675 (class 0 OID 96749)
-- Dependencies: 327
-- Data for Name: populations; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.populations (id, fk_religions, fk_cultures, fk_socialgroups, fk_localisations, happiness, volunteers) FROM stdin;
\.


--
-- TOC entry 5677 (class 0 OID 96753)
-- Dependencies: 329
-- Data for Name: populationusedresource; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.populationusedresource (id, fk_population, fk_resources, amount) FROM stdin;
\.


--
-- TOC entry 5679 (class 0 OID 96757)
-- Dependencies: 331
-- Data for Name: productionCost; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."productionCost" (id, "fk_UnitTypes", "fk_Resources", amount) FROM stdin;
\.


--
-- TOC entry 5681 (class 0 OID 96761)
-- Dependencies: 333
-- Data for Name: productionShares; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."productionShares" (id, "fk_SocialGroups", "fk_Resources", coefficient) FROM stdin;
\.


--
-- TOC entry 5683 (class 0 OID 96765)
-- Dependencies: 335
-- Data for Name: relatedEvents; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."relatedEvents" (id, "fk_Events", "fk_Nations") FROM stdin;
\.


--
-- TOC entry 5685 (class 0 OID 96769)
-- Dependencies: 337
-- Data for Name: religions; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.religions (id, name, icon) FROM stdin;
\.


--
-- TOC entry 5687 (class 0 OID 96775)
-- Dependencies: 339
-- Data for Name: resources; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.resources (id, name, ismain, icon) FROM stdin;
\.


--
-- TOC entry 5689 (class 0 OID 96781)
-- Dependencies: 341
-- Data for Name: socialgroups; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.socialgroups (id, name, basehappiness, volunteers, icon) FROM stdin;
\.


--
-- TOC entry 5691 (class 0 OID 96787)
-- Dependencies: 343
-- Data for Name: tradeagreements; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.tradeagreements (id, fk_nationoffering, fk_nationreceiving, status, duration, description) FROM stdin;
\.


--
-- TOC entry 5693 (class 0 OID 96793)
-- Dependencies: 345
-- Data for Name: troops; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.troops (id, "fk_UnitTypes", "fk_Armies", quantity) FROM stdin;
\.


--
-- TOC entry 5695 (class 0 OID 96797)
-- Dependencies: 347
-- Data for Name: unitOrders; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."unitOrders" (id, "fk_UnitTypes", "fk_Nations", quantity) FROM stdin;
\.


--
-- TOC entry 5697 (class 0 OID 96801)
-- Dependencies: 349
-- Data for Name: unitTypes; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."unitTypes" (id, name, description, melee, range, defense, speed, morale, "volunteersNeeded", "isNaval") FROM stdin;
\.


--
-- TOC entry 5699 (class 0 OID 96807)
-- Dependencies: 351
-- Data for Name: usedResources; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2."usedResources" (id, "fk_SocialGroups", "fk_Resources", amount) FROM stdin;
\.


--
-- TOC entry 5701 (class 0 OID 96811)
-- Dependencies: 353
-- Data for Name: wantedresources; Type: TABLE DATA; Schema: game_2; Owner: postgres
--

COPY game_2.wantedresources (id, fk_resource, fk_tradeagreement, amount) FROM stdin;
\.


--
-- TOC entry 5712 (class 0 OID 0)
-- Dependencies: 221
-- Name: gameaccess_id_seq; Type: SEQUENCE SET; Schema: Global; Owner: postgres
--

SELECT pg_catalog.setval('"Global".gameaccess_id_seq', 4, true);


--
-- TOC entry 5713 (class 0 OID 0)
-- Dependencies: 223
-- Name: games_id_seq; Type: SEQUENCE SET; Schema: Global; Owner: postgres
--

SELECT pg_catalog.setval('"Global".games_id_seq', 2, true);


--
-- TOC entry 5714 (class 0 OID 0)
-- Dependencies: 226
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: Global; Owner: postgres
--

SELECT pg_catalog.setval('"Global".users_id_seq', 5, true);


--
-- TOC entry 5715 (class 0 OID 0)
-- Dependencies: 228
-- Name: accessToUnits_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."accessToUnits_id_seq"', 12, true);


--
-- TOC entry 5716 (class 0 OID 0)
-- Dependencies: 230
-- Name: accessestonations_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.accessestonations_id_seq', 5, true);


--
-- TOC entry 5717 (class 0 OID 0)
-- Dependencies: 232
-- Name: actions_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.actions_id_seq', 5, true);


--
-- TOC entry 5718 (class 0 OID 0)
-- Dependencies: 234
-- Name: armies_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.armies_id_seq', 15, true);


--
-- TOC entry 5719 (class 0 OID 0)
-- Dependencies: 291
-- Name: army_settings_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.army_settings_id_seq', 1, true);


--
-- TOC entry 5720 (class 0 OID 0)
-- Dependencies: 236
-- Name: cultures_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.cultures_id_seq', 5, true);


--
-- TOC entry 5721 (class 0 OID 0)
-- Dependencies: 238
-- Name: events_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.events_id_seq', 5, true);


--
-- TOC entry 5722 (class 0 OID 0)
-- Dependencies: 240
-- Name: factions_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.factions_id_seq', 5, true);


--
-- TOC entry 5723 (class 0 OID 0)
-- Dependencies: 243
-- Name: localisationsResources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."localisationsResources_id_seq"', 10, true);


--
-- TOC entry 5724 (class 0 OID 0)
-- Dependencies: 244
-- Name: localisations_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.localisations_id_seq', 8, true);


--
-- TOC entry 5725 (class 0 OID 0)
-- Dependencies: 246
-- Name: maintenanceCosts_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."maintenanceCosts_id_seq"', 10, true);


--
-- TOC entry 5726 (class 0 OID 0)
-- Dependencies: 249
-- Name: map_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.map_id_seq', 5, true);


--
-- TOC entry 5727 (class 0 OID 0)
-- Dependencies: 251
-- Name: modifiers_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.modifiers_id_seq', 1, false);


--
-- TOC entry 5728 (class 0 OID 0)
-- Dependencies: 253
-- Name: nations_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.nations_id_seq', 5, true);


--
-- TOC entry 5729 (class 0 OID 0)
-- Dependencies: 255
-- Name: offeredresources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.offeredresources_id_seq', 5, true);


--
-- TOC entry 5730 (class 0 OID 0)
-- Dependencies: 257
-- Name: ownedResources_Id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."ownedResources_Id_seq"', 45, true);


--
-- TOC entry 5731 (class 0 OID 0)
-- Dependencies: 259
-- Name: players_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.players_id_seq', 4, true);


--
-- TOC entry 5732 (class 0 OID 0)
-- Dependencies: 261
-- Name: populationproductionshares_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.populationproductionshares_id_seq', 10, true);


--
-- TOC entry 5733 (class 0 OID 0)
-- Dependencies: 263
-- Name: populations_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.populations_id_seq', 8, true);


--
-- TOC entry 5734 (class 0 OID 0)
-- Dependencies: 265
-- Name: populationusedresource_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.populationusedresource_id_seq', 9, true);


--
-- TOC entry 5735 (class 0 OID 0)
-- Dependencies: 267
-- Name: productionCost_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."productionCost_id_seq"', 10, true);


--
-- TOC entry 5736 (class 0 OID 0)
-- Dependencies: 269
-- Name: productionShares_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."productionShares_id_seq"', 12, true);


--
-- TOC entry 5737 (class 0 OID 0)
-- Dependencies: 271
-- Name: relatedEvents_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."relatedEvents_id_seq"', 10, true);


--
-- TOC entry 5738 (class 0 OID 0)
-- Dependencies: 273
-- Name: religions_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.religions_id_seq', 5, true);


--
-- TOC entry 5739 (class 0 OID 0)
-- Dependencies: 275
-- Name: resources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.resources_id_seq', 11, true);


--
-- TOC entry 5740 (class 0 OID 0)
-- Dependencies: 277
-- Name: socialgroups_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.socialgroups_id_seq', 5, true);


--
-- TOC entry 5741 (class 0 OID 0)
-- Dependencies: 279
-- Name: tradeagreements_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.tradeagreements_id_seq', 5, true);


--
-- TOC entry 5742 (class 0 OID 0)
-- Dependencies: 281
-- Name: troops_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.troops_id_seq', 8, true);


--
-- TOC entry 5743 (class 0 OID 0)
-- Dependencies: 283
-- Name: unitOrders_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."unitOrders_id_seq"', 12, true);


--
-- TOC entry 5744 (class 0 OID 0)
-- Dependencies: 285
-- Name: unitTypes_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."unitTypes_id_seq"', 5, true);


--
-- TOC entry 5745 (class 0 OID 0)
-- Dependencies: 287
-- Name: usedResources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1."usedResources_id_seq"', 11, true);


--
-- TOC entry 5746 (class 0 OID 0)
-- Dependencies: 289
-- Name: wantedresources_id_seq; Type: SEQUENCE SET; Schema: game_1; Owner: postgres
--

SELECT pg_catalog.setval('game_1.wantedresources_id_seq', 5, true);


--
-- TOC entry 5747 (class 0 OID 0)
-- Dependencies: 293
-- Name: accessToUnits_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."accessToUnits_id_seq"', 1, false);


--
-- TOC entry 5748 (class 0 OID 0)
-- Dependencies: 295
-- Name: accessestonations_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.accessestonations_id_seq', 1, false);


--
-- TOC entry 5749 (class 0 OID 0)
-- Dependencies: 297
-- Name: actions_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.actions_id_seq', 1, false);


--
-- TOC entry 5750 (class 0 OID 0)
-- Dependencies: 299
-- Name: armies_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.armies_id_seq', 1, false);


--
-- TOC entry 5751 (class 0 OID 0)
-- Dependencies: 301
-- Name: cultures_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.cultures_id_seq', 1, false);


--
-- TOC entry 5752 (class 0 OID 0)
-- Dependencies: 303
-- Name: events_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.events_id_seq', 1, false);


--
-- TOC entry 5753 (class 0 OID 0)
-- Dependencies: 305
-- Name: factions_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.factions_id_seq', 1, false);


--
-- TOC entry 5754 (class 0 OID 0)
-- Dependencies: 308
-- Name: localisationsResources_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."localisationsResources_id_seq"', 1, false);


--
-- TOC entry 5755 (class 0 OID 0)
-- Dependencies: 309
-- Name: localisations_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.localisations_id_seq', 1, false);


--
-- TOC entry 5756 (class 0 OID 0)
-- Dependencies: 311
-- Name: maintenanceCosts_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."maintenanceCosts_id_seq"', 1, false);


--
-- TOC entry 5757 (class 0 OID 0)
-- Dependencies: 314
-- Name: map_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.map_id_seq', 1, false);


--
-- TOC entry 5758 (class 0 OID 0)
-- Dependencies: 316
-- Name: modifiers_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.modifiers_id_seq', 1, false);


--
-- TOC entry 5759 (class 0 OID 0)
-- Dependencies: 318
-- Name: nations_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.nations_id_seq', 1, false);


--
-- TOC entry 5760 (class 0 OID 0)
-- Dependencies: 320
-- Name: offeredresources_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.offeredresources_id_seq', 1, false);


--
-- TOC entry 5761 (class 0 OID 0)
-- Dependencies: 322
-- Name: ownedResources_Id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."ownedResources_Id_seq"', 1, false);


--
-- TOC entry 5762 (class 0 OID 0)
-- Dependencies: 324
-- Name: players_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.players_id_seq', 1, false);


--
-- TOC entry 5763 (class 0 OID 0)
-- Dependencies: 326
-- Name: populationproductionshares_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.populationproductionshares_id_seq', 1, false);


--
-- TOC entry 5764 (class 0 OID 0)
-- Dependencies: 328
-- Name: populations_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.populations_id_seq', 1, false);


--
-- TOC entry 5765 (class 0 OID 0)
-- Dependencies: 330
-- Name: populationusedresource_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.populationusedresource_id_seq', 1, false);


--
-- TOC entry 5766 (class 0 OID 0)
-- Dependencies: 332
-- Name: productionCost_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."productionCost_id_seq"', 1, false);


--
-- TOC entry 5767 (class 0 OID 0)
-- Dependencies: 334
-- Name: productionShares_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."productionShares_id_seq"', 1, false);


--
-- TOC entry 5768 (class 0 OID 0)
-- Dependencies: 336
-- Name: relatedEvents_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."relatedEvents_id_seq"', 1, false);


--
-- TOC entry 5769 (class 0 OID 0)
-- Dependencies: 338
-- Name: religions_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.religions_id_seq', 1, false);


--
-- TOC entry 5770 (class 0 OID 0)
-- Dependencies: 340
-- Name: resources_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.resources_id_seq', 1, false);


--
-- TOC entry 5771 (class 0 OID 0)
-- Dependencies: 342
-- Name: socialgroups_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.socialgroups_id_seq', 1, false);


--
-- TOC entry 5772 (class 0 OID 0)
-- Dependencies: 344
-- Name: tradeagreements_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.tradeagreements_id_seq', 1, false);


--
-- TOC entry 5773 (class 0 OID 0)
-- Dependencies: 346
-- Name: troops_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.troops_id_seq', 1, false);


--
-- TOC entry 5774 (class 0 OID 0)
-- Dependencies: 348
-- Name: unitOrders_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."unitOrders_id_seq"', 1, false);


--
-- TOC entry 5775 (class 0 OID 0)
-- Dependencies: 350
-- Name: unitTypes_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."unitTypes_id_seq"', 1, false);


--
-- TOC entry 5776 (class 0 OID 0)
-- Dependencies: 352
-- Name: usedResources_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2."usedResources_id_seq"', 1, false);


--
-- TOC entry 5777 (class 0 OID 0)
-- Dependencies: 354
-- Name: wantedresources_id_seq; Type: SEQUENCE SET; Schema: game_2; Owner: postgres
--

SELECT pg_catalog.setval('game_2.wantedresources_id_seq', 1, false);


-- Completed on 2026-02-15 17:04:08

--
-- PostgreSQL database dump complete
--

\unrestrict ZxTD8dqHsw7TgfrfoxTgjkmt9vL1IjtC8cqhG88dx8WEBROKsb2zZqWBnWf45du

