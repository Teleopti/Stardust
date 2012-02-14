/* 
Trunk initiated: 
2010-08-19 
13:54
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-08-30
--Desc: Groupings tables
----------------
CREATE TABLE stage.stg_group_page_person
           (
           group_page_code uniqueidentifier NOT NULL,	-- Contract mfl saknar code, f�r h�rdkoda i ETL kod.
           group_page_name nvarchar(100) NULL,			-- Contract mfl �r spr�kberoende och resolvas senare resursnyckeln group_page_name_key, h�r borde dock det engelskanamnet finnas pga kuben
           group_page_name_resource_key nvarchar(100) NULL,		-- Resursnyckel f�r att i asp.net resolva Contract mfl. User defined har NULL h�r.
           group_code uniqueidentifier NOT NULL,		-- Contract mfl saknar code, f�rh�rdkoda i ETL kod
           group_name nvarchar(1024) NOT NULL,			-- 
           group_is_custom bit NOT NULL,				-- S�tt till True = 1 om detta �r e n grupp anv�ndaren sj�lv skapat
           person_code uniqueidentifier NOT NULL,		
--           first_name nvarchar(30) NOT NULL,			-- beh�vs v�l inte h�r?
--           last_name nvarchar(30) NOT NULL,				-- beh�vs v�l inte h�r?
           datasource_id int NOT NULL,
           insert_date smalldatetime NULL,
           update_date smalldatetime NULL
           )  ON [STAGE]

ALTER TABLE stage.stg_group_page_person ADD CONSTRAINT
	PK_stg_group_page_person PRIMARY KEY CLUSTERED 
	(
	group_page_code,
	group_code
	) ON STAGE

CREATE TABLE mart.dim_group_page
           (
           group_page_id int identity(1,1),				-- PK i mart
           group_page_code uniqueidentifier NOT NULL,	-- Contract mfl saknar code, f�r h�rdkoda i ETL kod
           group_page_name_resource_key nvarchar(100) NULL,		-- Resursnyckel f�r att i asp.net resolva Contract mfl. User defined har NULL h�r.
           group_code uniqueidentifier NOT NULL,		-- Contract mfl saknar code, f�rh�rdkoda i ETL kod
           group_name nvarchar(1024) NOT NULL,
           datasource_id int NOT NULL,
           insert_date smalldatetime NULL,
           datasource_update_date smalldatetime NULL
           )  ON [MART]
           
ALTER TABLE mart.dim_group_page ADD CONSTRAINT
	PK_dim_group_page PRIMARY KEY CLUSTERED 
	(
	group_page_id
	) ON MART

CREATE TABLE mart.bridge_group_page_person			--Vilka snubbar ing�r i respektive groupings
           (
           group_page_id int NOT NULL,	-- alla group page x
           person_id int NOT NULL,		-- alla IDn f�r varje person_code
--           business_unit_id int NOT NULL, --kanske bra att ha i kuben
           datasource_id int NOT NULL,
           insert_date smalldatetime NULL --kommer att trunkeras vid varje ETL-laddning
           )  ON [MART]

ALTER TABLE mart.bridge_group_page_person ADD CONSTRAINT
	PK_bridge_group_page_person PRIMARY KEY CLUSTERED 
	(
	group_page_id,
	person_id
	) ON MART


 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (292,'7.1.292') 
