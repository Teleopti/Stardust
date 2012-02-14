/* 
Trunk initiated: 
2010-07-07 
15:14
By: TOPTINET\niclash 
On TELEOPTI554 
*/ 
----------------
--Name: Robin K
--Date: 2010-07-07
--Desc: Added a new column to connect a scorecard to a team
----------------
CREATE TABLE [mart].[dim_team_new](
	[team_id] [int] IDENTITY(1,1) NOT NULL,
	[team_code] [uniqueidentifier] NULL,
	[team_name] [nvarchar](100) NULL,
	[scorecard_id] [int] NULL,
	[site_id] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK2_dim_team] PRIMARY KEY CLUSTERED 
(
	[team_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]

GO

SET IDENTITY_INSERT mart.dim_team_new ON
GO

IF EXISTS(SELECT * FROM mart.dim_team)
BEGIN
	EXEC('INSERT INTO mart.dim_team_new ([team_id],[team_code],[team_name],[scorecard_id],[site_id],[business_unit_id],[datasource_id],[insert_date],[update_date],[datasource_update_date])
		SELECT [team_id],[team_code],[team_name],-1,[site_id],[business_unit_id],[datasource_id],[insert_date],[update_date],[datasource_update_date] FROM mart.dim_team')
END
GO

SET IDENTITY_INSERT mart.dim_team_new OFF
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_dim_team_dim_site]') AND parent_object_id = OBJECT_ID(N'[mart].[dim_team]'))
ALTER TABLE [mart].[dim_team] DROP CONSTRAINT [FK_dim_team_dim_site]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_team_scorecard_id]'))
BEGIN
ALTER TABLE [mart].[dim_team] DROP CONSTRAINT DF_dim_team_scorecard_id
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_team_business_unit_id]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_team] DROP CONSTRAINT [DF_dim_team_business_unit_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_team_datasource_id]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_team] DROP CONSTRAINT [DF_dim_team_datasource_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_team_insert_date]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_team] DROP CONSTRAINT [DF_dim_team_insert_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_dim_team_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [mart].[dim_team] DROP CONSTRAINT [DF_dim_team_update_date]
END

GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_kpi_targets_team_dim_team]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_kpi_targets_team]'))
ALTER TABLE [mart].[fact_kpi_targets_team] DROP CONSTRAINT [FK_fact_kpi_targets_team_dim_team]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_acd_login_person_dim_team]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_acd_login_person]'))
ALTER TABLE [mart].[bridge_acd_login_person] DROP CONSTRAINT [FK_bridge_acd_login_person_dim_team]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_team]') AND type in (N'U'))
DROP TABLE [mart].[dim_team]
GO

EXEC sp_rename N'[mart].[dim_team_new]',N'dim_team','OBJECT'
GO

EXEC sp_rename N'[mart].[PK2_dim_team]',N'PK_dim_team','OBJECT'
GO

ALTER TABLE [mart].[fact_kpi_targets_team]  WITH CHECK ADD  CONSTRAINT [FK_fact_kpi_targets_team_dim_team] FOREIGN KEY([team_id])
REFERENCES [mart].[dim_team] ([team_id])
GO

ALTER TABLE [mart].[fact_kpi_targets_team] CHECK CONSTRAINT [FK_fact_kpi_targets_team_dim_team]
GO

ALTER TABLE [mart].[bridge_acd_login_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_acd_login_person_dim_team] FOREIGN KEY([team_id])
REFERENCES [mart].[dim_team] ([team_id])
GO

ALTER TABLE [mart].[bridge_acd_login_person] CHECK CONSTRAINT [FK_bridge_acd_login_person_dim_team]
GO

ALTER TABLE [mart].[dim_team]  WITH CHECK ADD  CONSTRAINT [FK_dim_team_dim_site] FOREIGN KEY([site_id])
REFERENCES [mart].[dim_site] ([site_id])
GO

ALTER TABLE [mart].[dim_team]  WITH CHECK ADD  CONSTRAINT [FK_dim_team_dim_scorecard] FOREIGN KEY([scorecard_id])
REFERENCES [mart].[dim_scorecard] ([scorecard_id])
GO

ALTER TABLE [mart].[dim_team] CHECK CONSTRAINT [FK_dim_team_dim_site]
GO

ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_scorecard_id]  DEFAULT ((-1)) FOR [scorecard_id]
GO

ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_site_id]  DEFAULT ((-1)) FOR [site_id]
GO

ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO

ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO

ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_person_valid_from_interval_id]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_person] DROP CONSTRAINT [DF_stg_person_valid_from_interval_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_person_valid_to_interval_id]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_person] DROP CONSTRAINT [DF_stg_person_valid_to_interval_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_person_datasource_id]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_person] DROP CONSTRAINT [DF_stg_person_datasource_id]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_person_insert_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_person] DROP CONSTRAINT [DF_stg_person_insert_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_person_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_person] DROP CONSTRAINT [DF_stg_person_update_date]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_stg_person_datasource_update_date]') AND type = 'D')
BEGIN
ALTER TABLE [stage].[stg_person] DROP CONSTRAINT [DF_stg_person_datasource_update_date]
END

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_person]') AND type in (N'U'))
DROP TABLE [stage].[stg_person]
GO

CREATE TABLE [stage].[stg_person](
	[person_code] [uniqueidentifier] NOT NULL,
	[valid_from_date] [smalldatetime] NOT NULL,
	[valid_to_date] [smalldatetime] NOT NULL,
	[valid_from_interval_id] [int] NOT NULL,
	[valid_to_interval_id] [int] NOT NULL,
	[valid_to_interval_start] [smalldatetime] NULL,
	[person_period_code] [uniqueidentifier] NULL,
	[person_first_name] [nvarchar](25) NOT NULL,
	[person_last_name] [nvarchar](25) NOT NULL,
	[team_code] [uniqueidentifier] NOT NULL,
	[team_name] [nvarchar](50) NOT NULL,
	[scorecard_code] [uniqueidentifier] NULL,
	[site_code] [uniqueidentifier] NOT NULL,
	[site_name] [nvarchar](50) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[email] [nvarchar](200) NULL,
	[note] [nchar](1024) NULL,
	[employment_number] [nvarchar](50) NULL,
	[employment_start_date] [smalldatetime] NOT NULL,
	[employment_end_date] [smalldatetime] NOT NULL,
	[time_zone_code] [nvarchar](50) NULL,
	[is_agent] [bit] NULL,
	[is_user] [bit] NULL,
	[contract_code] [uniqueidentifier] NULL,
	[contract_name] [nvarchar](50) NULL,
	[parttime_code] [uniqueidentifier] NULL,
	[parttime_percentage] [nvarchar](50) NULL,
	[employment_type] [nvarchar](50) NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_person] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[valid_from_date] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]

GO

ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_valid_from_interval_id]  DEFAULT ((0)) FOR [valid_from_interval_id]
GO

ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_valid_to_interval_id]  DEFAULT ((0)) FOR [valid_to_interval_id]
GO

ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO

ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO


----------------
--Name: David J
--Date: 2010-07-26
--Desc: Start of PBI 8649: temp check in for deleted stuff
----------------
--clean up
DELETE FROM mart.fact_contract
DROP TABLE mart.fact_contract
DROP TABLE stage.stg_contract

-- Scenario
TRUNCATE TABLE stage.stg_scenario
ALTER TABLE stage.stg_scenario ADD is_deleted bit NOT NULL

ALTER TABLE mart.dim_scenario ADD is_deleted bit NULL
GO
UPDATE mart.dim_scenario SET is_deleted = 0
ALTER TABLE mart.dim_scenario ALTER COLUMN is_deleted bit NOT NULL
ALTER TABLE mart.dim_scenario ADD CONSTRAINT DF_dim_scenario_is_deleted  DEFAULT ((0)) FOR [is_deleted]

-- Site, not needed. It's periodiserad via Agent.PersonPeriod
--------Commeted out-------------
--CREATE TABLE [stage].[stg_site](
	--[site_code] [uniqueidentifier] NOT NULL,
	--[site_name] [nvarchar](100) NULL,
	--[business_unit_code] [uniqueidentifier] NOT NULL,
	--[datasource_id] [smallint] NOT NULL,
	--[insert_date] [smalldatetime] NOT NULL,
	--[update_date] [smalldatetime] NOT NULL,
	--[datasource_update_date] [smalldatetime] NULL,
	--[is_deleted] [bit] NOT NULL
--) ON [STAGE]
GO
--ALTER TABLE stage.stg_site ADD CONSTRAINT
	--PK_stg_site PRIMARY KEY CLUSTERED 
	--(
	--site_code ASC
	--) ON STAGE

--ALTER TABLE mart.dim_site ADD is_deleted bit NULL
GO
--UPDATE mart.dim_site SET is_deleted = 0
--ALTER TABLE mart.dim_site ALTER COLUMN is_deleted bit NOT NULL
--ALTER TABLE mart.dim_site ADD CONSTRAINT DF_dim_site_is_deleted  DEFAULT ((0)) FOR [is_deleted]
--------Commeted out-------------

-- Contract
--Finns som stage tabell, men används inte.
--Finns inte som dim-table, dvs. det finns inte heller några urvals-sidor för contract i rapporterna
--Finns som fact table?! lägger till is_deleted där tills vidare. Tveksam!


-- removed
--------Commeted out-------------
--TRUNCATE TABLE stage.stg_contract
--ALTER TABLE stage.stg_contract ADD is_deleted bit NOT NULL

--ALTER TABLE mart.fact_contract ADD is_deleted bit NULL
GO
--UPDATE mart.fact_contract SET is_deleted = 0
--ALTER TABLE mart.fact_contract ALTER COLUMN is_deleted bit NOT NULL
--ALTER TABLE mart.fact_contract ADD CONSTRAINT DF_fact_contract_is_deleted  DEFAULT ((0)) FOR [is_deleted]
--------Commeted out-------------

-- Activity
TRUNCATE TABLE stage.stg_activity
ALTER TABLE stage.stg_activity ADD is_deleted bit NOT NULL

ALTER TABLE mart.dim_activity ADD is_deleted bit NULL
GO
UPDATE mart.dim_activity SET is_deleted = 0
ALTER TABLE mart.dim_activity ALTER COLUMN is_deleted bit NOT NULL
ALTER TABLE mart.dim_activity ADD CONSTRAINT DF_dim_activity_is_deleted  DEFAULT ((0)) FOR [is_deleted]

-- Absence
TRUNCATE TABLE stage.stg_absence
ALTER TABLE stage.stg_absence ADD is_deleted bit NOT NULL

ALTER TABLE mart.dim_absence ADD is_deleted bit NULL
GO
UPDATE mart.dim_absence SET is_deleted = 0
ALTER TABLE mart.dim_absence ALTER COLUMN is_deleted bit NOT NULL
ALTER TABLE mart.dim_absence ADD CONSTRAINT DF_dim_absence_is_deleted  DEFAULT ((0)) FOR [is_deleted]

--not needed, we think ...
--------Commeted out-------------
---- day_off
----Finns bara som DayOff Template i domänen. Kan vi verkligen jobba vidare med namn som PK i martet?!!!
--TRUNCATE TABLE stage.stg_day_off
--ALTER TABLE stage.stg_day_off ADD is_deleted bit NOT NULL

--ALTER TABLE mart.dim_day_off ADD is_deleted bit NULL

GO

--UPDATE mart.dim_day_off SET is_deleted = 0
--ALTER TABLE mart.dim_day_off ALTER COLUMN is_deleted bit NOT NULL
--ALTER TABLE mart.dim_day_off ADD CONSTRAINT DF_dim_day_off_is_deleted  DEFAULT ((0)) FOR [is_deleted]
--------Commeted out-------------

-- shift_category
TRUNCATE TABLE stage.stg_shift_category
ALTER TABLE stage.stg_shift_category ADD is_deleted bit NOT NULL

ALTER TABLE mart.dim_shift_category ADD is_deleted bit NULL
GO
UPDATE mart.dim_shift_category SET is_deleted = 0
ALTER TABLE mart.dim_shift_category ALTER COLUMN is_deleted bit NOT NULL
ALTER TABLE mart.dim_shift_category ADD  CONSTRAINT DF_dim_shift_category_is_deleted  DEFAULT ((0)) FOR [is_deleted]

--team, not needed. It's periodiserad via Agent.PersonPeriod
--------Commeted out-------------
--CREATE TABLE [stage].[stg_team](
	--[team_code] [uniqueidentifier] NOT NULL,
	--[team_name] [nvarchar](100) NULL,
	--[site_code] [uniqueidentifier] NOT NULL,
	--[scorecard_code] [uniqueidentifier] NOT NULL,
	--[business_unit_code] [uniqueidentifier] NOT NULL,
	--[datasource_id] [smallint] NOT NULL,
	--[insert_date] [smalldatetime] NOT NULL,
	--[update_date] [smalldatetime] NOT NULL,
	--[datasource_update_date] [smalldatetime] NULL,
	--[is_deleted] [bit] NOT NULL
--) ON [STAGE]

--ALTER TABLE stage.stg_team ADD CONSTRAINT
	--PK_stg_team PRIMARY KEY CLUSTERED 
	--(
	--team_code ASC
	--) ON STAGE


--ALTER TABLE mart.dim_team ADD is_deleted bit NULL
GO
--UPDATE mart.dim_team SET is_deleted = 0
--ALTER TABLE mart.dim_team ALTER COLUMN is_deleted bit NOT NULL
--ALTER TABLE mart.dim_team ADD CONSTRAINT DF_dim_team_is_deleted  DEFAULT ((0)) FOR [is_deleted]
--------Commeted out-------------

----------------
--Name: jonas n
--Date: 2010-08-18
--Desc: Add report Absence Time per Absence
----------------
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (22, 22, 1, N'~/Selection.aspx?ReportID=22', N'_self', N'Absence time per absence', N'ResReportAbsenceTimePerAbsence', 1, N'~/Reports/CCC/report_absence_time_per_absence.rdlc', 1000, N'mart.report_data_absence_time_per_agent', N'f01_Report_AbsenceTimeperAbsence.html', N'', N'', N'', N'')
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (290,'7.1.290') 
