/* 
Trunk initiated: 
2011-01-31 
11:14
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2011-01-30
--Desc: Cut some time off Queue and Agent ETL-load,indexes added at live customer
--Note: added as IF EXISTS since this is already exist in 7.1.315 + customers < 7.1.315
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_acd_login]') AND name = N'IX_datasource')
	CREATE NONCLUSTERED INDEX [IX_datasource]
	ON [mart].[dim_acd_login] ([datasource_id])
	INCLUDE ([acd_login_id],[acd_login_agg_id])
	ON [MART]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_acd_login]') AND name = N'IX_acd_datasource')
	CREATE NONCLUSTERED INDEX [IX_acd_datasource]
	ON [mart].[dim_acd_login] ([acd_login_agg_id],[datasource_id])
	INCLUDE ([acd_login_id])
	ON [MART]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_queue]') AND name = N'IX_aggId_datasource')
	CREATE NONCLUSTERED INDEX [IX_aggId_datasource]
	ON [mart].[dim_queue] ([queue_agg_id],[datasource_id])
	INCLUDE ([queue_id])
	ON [MART]

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_queue]') AND name = N'IX_datasource')
	CREATE NONCLUSTERED INDEX [IX_datasource]
	ON [mart].[dim_queue] ([datasource_id])
	INCLUDE ([queue_id],[queue_agg_id])
	ON [MART]
GO

----------------  
--Name: David J + Mattias E
--Date: 2011-02-28
--Desc: Adding tables and report definitions for Overtime reporting
----------------  
-- Stage table for multiplicator definition set
CREATE TABLE [stage].[stg_overtime](
	[overtime_code] [uniqueidentifier] NULL,
	[overtime_name] [nvarchar](100) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL
) ON [stage]

ALTER TABLE [stage].[stg_overtime] ADD  CONSTRAINT [DF_stg_multiplicator_definition_set_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
ALTER TABLE [stage].[stg_overtime] ADD  CONSTRAINT [DF_stg_multiplicator_definition_set_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [stage].[stg_overtime] ADD  CONSTRAINT [DF_stg_multiplicator_definition_set_update_date]  DEFAULT (getdate()) FOR [update_date]

ALTER TABLE [stage].[stg_overtime] ADD  CONSTRAINT [DF_stg_multiplicator_definition_set_is_deleted]  DEFAULT ((0)) FOR [is_deleted]

-- Dimension for multiplicator definition set
CREATE TABLE [mart].[dim_overtime](
           [overtime_id] [int] IDENTITY(1,1) NOT NULL,
           [overtime_code] [uniqueidentifier] NULL,
           [overtime_name] [nvarchar](100) NOT NULL,
           [business_unit_id] [int] NOT NULL,
           [datasource_id] [smallint] NOT NULL,
           [insert_date] [smalldatetime] NOT NULL,
           [update_date] [smalldatetime] NOT NULL,
           [datasource_update_date] [smalldatetime] NULL,
           [is_deleted] [bit] NOT NULL
) ON [MART]

ALTER TABLE [mart].[dim_overtime] ADD  CONSTRAINT [DF_dim_multiplicator_definition_set_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
ALTER TABLE [mart].[dim_overtime] ADD  CONSTRAINT [DF_dim_multiplicator_definition_set_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[dim_overtime] ADD  CONSTRAINT [DF_dim_multiplicator_definition_set_update_date]  DEFAULT (getdate()) FOR [update_date]
ALTER TABLE [mart].[dim_overtime] ADD  CONSTRAINT [DF_dim_multiplicator_definition_set_is_deleted]  DEFAULT ((0)) FOR [is_deleted]

-- Update fact_schedule with overtime id and minutes
ALTER TABLE mart.fact_schedule ADD
	overtime_id INT DEFAULT -1 NOT NULL

-- Update stg_schedule with multiplicator definition set and overtime minutes
ALTER TABLE stage.stg_schedule ADD
	overtime_code UNIQUEIDENTIFIER NULL

-------------
--Add new control
-------------
INSERT INTO mart.report_control 
SELECT 33,'twolistOvertime','mart.report_control_twolist_overtime_get',NULL

------------
--Add report
------------
insert into mart.report
select 23, 23,1,'~/Selection.aspx?ReportID=23','_blank','Schedule overtime','ResReportScheduledOvertimePerAgent',1,'~/Reports/CCC/report_scheduled_overtime_per_agent.rdlc',1000,'mart.report_data_scheduled_overtime_per_agent','f01_Report_ScheduleOvertime.html','','','',''

-------------
--Add selection params
-------------
INSERT INTO [mart].[report_control_collection]
SELECT '226','23','1','14','0','ResScenarioColon',NULL,'@scenario_id',NULL,NULL,NULL,NULL UNION ALL
SELECT '227','23','2','1','12:00','ResDateFromColon',NULL,'@date_from',NULL,NULL,NULL,NULL UNION ALL
SELECT '228','23','3','2','12:00','ResDateToColon',NULL,'@date_to','227',NULL,NULL,NULL UNION ALL
SELECT '229','23','4','12','0','ResIntervalFromColon','1','@interval_from',NULL,NULL,NULL,NULL UNION ALL
SELECT '230','23','5','13','-99','ResIntervalToColon','2','@interval_to',NULL,NULL,NULL,NULL UNION ALL
SELECT '231','23','9','3','-2','ResSiteNameColon',NULL,'@site_id','227','228','235',NULL UNION ALL
SELECT '232','23','10','4','-2','ResTeamNameColon',NULL,'@team_id','227','228','231',NULL UNION ALL
SELECT '233','23','11','5','-2','ResAgentsColon',NULL,'@agent_code','227','228','231','232' UNION ALL
SELECT '234','23','13','22','-1','ResTimeZoneColon',NULL,'@time_zone_id',NULL,NULL,NULL,NULL UNION ALL
SELECT '235','23','6','29','-2','ResGroupPageColon',NULL,'@group_page_code',NULL,NULL,NULL,NULL UNION ALL
SELECT '236','23','7','30','-2','ResGroupPageGroupColon',NULL,'@group_page_group_id','235',NULL,NULL,NULL UNION ALL
SELECT '237','23','8','31','-2','ResAgentColon',NULL,'@group_page_agent_code','227','228','235','236' UNION ALL
SELECT '238','23','12','33','-1','ResOvertimeColon',NULL,'@overtime_set',NULL,NULL,NULL,NULL 
----------------  
--Name: DavidJ+NiclasH
--Date: 2011-02-23
--Desc: to speed up MyReport
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_person_scenario_schdate_interval')
	CREATE NONCLUSTERED INDEX [IX_person_scenario_schdate_interval]
	ON [mart].[fact_schedule] 
	(
		  [person_id] ASC,
		  [scenario_id] ASC,
		  [schedule_date_id] ASC,
		  [interval_id] ASC
	)
	INCLUDE ( [scheduled_ready_time_m])
GO

------------------
--Name: DavidJ
--Date: 2011-02-28
--Desc: Alter user settings to store more data (added to root as well, NO MERGE)
------------------
IF EXISTS(
	SELECT * FROM sys.columns sc
	INNER JOIN sys.objects so
	ON sc.object_id = so.object_id
	INNER JOIN sys.types AS t
	ON sc.user_type_id=t.user_type_id
	WHERE so.name='report_user_setting'
	AND so.schema_id = schema_id('mart')
	AND t.name = 'nvarchar'
	AND sc.name = 'control_setting'
)
ALTER TABLE mart.report_user_setting
ALTER COLUMN control_setting varchar(max)
 
GO 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (316,'7.1.316') 
