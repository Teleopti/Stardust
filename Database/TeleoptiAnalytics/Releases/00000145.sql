/* 
Trunk initiated: 
2009-08-27 
15:15
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2009-09-02
--Desc: We need to exclude the Teleopti "virtual" queue from Agg DB (usually -1)
----------------  
CREATE TABLE mart.sys_queue_exclude (queue_original_id int NOT NULL)
ALTER TABLE mart.sys_queue_exclude ADD CONSTRAINT
	PK_dim_queue_exclude PRIMARY KEY CLUSTERED 
	(
	queue_original_id
	)
GO
INSERT INTO mart.sys_queue_exclude (queue_original_id)
VALUES (-1)


----------------  
--Name: David Jonsson
--Date: 2009-09-07
--Desc: Create new view in order to access interval in Agg-DB
----------------
INSERT INTO mart.sys_crossdatabaseview (view_name,view_definition,target_id)
VALUES ('v_ccc_system_info','SELECT * FROM $$$target$$$.dbo.ccc_system_info',4)
GO
EXEC mart.sys_crossDatabaseView_load
GO

----------------  
--Name: Jonas Nordh
--Date: 2009-09-08
--Desc: Add two columns for scheduled resources incl shrinkage to stage.stg_schedule_forecast_skill and mart.fact_schedule_forecast_skill
----------------
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE stage.Tmp_stg_schedule_forecast_skill
	(
	date smalldatetime NOT NULL,
	interval_id smallint NOT NULL,
	skill_code uniqueidentifier NOT NULL,
	scenario_code uniqueidentifier NOT NULL,
	forecasted_resources_m decimal(28, 4) NULL,
	forecasted_resources decimal(28, 4) NULL,
	forecasted_resources_incl_shrinkage_m decimal(28, 4) NULL,
	forecasted_resources_incl_shrinkage decimal(28, 4) NULL,
	scheduled_resources_m decimal(28, 4) NULL,
	scheduled_resources decimal(28, 4) NULL,
	scheduled_resources_incl_shrinkage_m decimal(28, 4) NULL,
	scheduled_resources_incl_shrinkage decimal(28, 4) NULL,
	business_unit_code uniqueidentifier NOT NULL,
	business_unit_name nvarchar(50) NOT NULL,
	datasource_id smallint NOT NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL
	)  ON STAGE
GO
IF EXISTS(SELECT * FROM stage.stg_schedule_forecast_skill)
	 EXEC('INSERT INTO stage.Tmp_stg_schedule_forecast_skill (date, interval_id, skill_code, scenario_code, forecasted_resources_m, forecasted_resources, forecasted_resources_incl_shrinkage_m, forecasted_resources_incl_shrinkage, scheduled_resources_m, scheduled_resources, business_unit_code, business_unit_name, datasource_id, insert_date, update_date)
		SELECT date, interval_id, skill_code, scenario_code, forecasted_resources_m, forecasted_resources, forecasted_resources_incl_shrinkage_m, forecasted_resources_incl_shrinkage, scheduled_resources_m, scheduled_resources, business_unit_code, business_unit_name, datasource_id, insert_date, update_date FROM stage.stg_schedule_forecast_skill WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE stage.stg_schedule_forecast_skill
GO
EXECUTE sp_rename N'stage.Tmp_stg_schedule_forecast_skill', N'stg_schedule_forecast_skill', 'OBJECT' 
GO
ALTER TABLE stage.stg_schedule_forecast_skill ADD CONSTRAINT
	PK_stg_forecast_skill_1 PRIMARY KEY CLUSTERED 
	(
	date,
	interval_id,
	skill_code,
	scenario_code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON STAGE

GO
COMMIT
GO






BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT FK_fact_schedule_forecast_skill_dim_skill
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT FK_fact_schedule_forecast_skill_dim_scenario
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT FK_fact_schedule_forecast_skill_dim_interval
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT FK_fact_schedule_forecast_skill_dim_date
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT DF_fact_forecast_skill_startdate_id
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT DF_fact_forecast_skill_interval_id
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT DF_fact_forecast_skill_queue_id
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT DF_fact_forecast_skill_scenario_id
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT DF_fact_forecast_skill_datasource_id
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT DF_fact_forecast_skill_insert_date
GO
ALTER TABLE mart.fact_schedule_forecast_skill
	DROP CONSTRAINT DF_fact_forecast_skill_update_date
GO
CREATE TABLE mart.Tmp_fact_schedule_forecast_skill
	(
	date_id int NOT NULL,
	interval_id smallint NOT NULL,
	skill_id int NOT NULL,
	scenario_id smallint NOT NULL,
	forecasted_resources_m decimal(28, 4) NULL,
	forecasted_resources decimal(28, 4) NULL,
	forecasted_resources_incl_shrinkage_m decimal(28, 4) NULL,
	forecasted_resources_incl_shrinkage decimal(28, 4) NULL,
	scheduled_resources_m decimal(28, 4) NULL,
	scheduled_resources decimal(28, 4) NULL,
	scheduled_resources_incl_shrinkage_m decimal(28, 4) NULL,
	scheduled_resources_incl_shrinkage decimal(28, 4) NULL,
	intraday_deviation_m decimal(28, 4) NULL,
	relative_difference decimal(28, 4) NULL,
	business_unit_id int NULL,
	datasource_id smallint NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL
	)  ON MART
GO
ALTER TABLE mart.Tmp_fact_schedule_forecast_skill ADD CONSTRAINT
	DF_fact_forecast_skill_startdate_id DEFAULT ((-1)) FOR date_id
GO
ALTER TABLE mart.Tmp_fact_schedule_forecast_skill ADD CONSTRAINT
	DF_fact_forecast_skill_interval_id DEFAULT ((-1)) FOR interval_id
GO
ALTER TABLE mart.Tmp_fact_schedule_forecast_skill ADD CONSTRAINT
	DF_fact_forecast_skill_queue_id DEFAULT ((-1)) FOR skill_id
GO
ALTER TABLE mart.Tmp_fact_schedule_forecast_skill ADD CONSTRAINT
	DF_fact_forecast_skill_scenario_id DEFAULT ((-1)) FOR scenario_id
GO
ALTER TABLE mart.Tmp_fact_schedule_forecast_skill ADD CONSTRAINT
	DF_fact_forecast_skill_datasource_id DEFAULT ((-1)) FOR datasource_id
GO
ALTER TABLE mart.Tmp_fact_schedule_forecast_skill ADD CONSTRAINT
	DF_fact_forecast_skill_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_fact_schedule_forecast_skill ADD CONSTRAINT
	DF_fact_forecast_skill_update_date DEFAULT (getdate()) FOR update_date
GO
IF EXISTS(SELECT * FROM mart.fact_schedule_forecast_skill)
	 EXEC('INSERT INTO mart.Tmp_fact_schedule_forecast_skill (date_id, interval_id, skill_id, scenario_id, forecasted_resources_m, forecasted_resources, forecasted_resources_incl_shrinkage_m, forecasted_resources_incl_shrinkage, scheduled_resources_m, scheduled_resources, intraday_deviation_m, relative_difference, business_unit_id, datasource_id, insert_date, update_date)
		SELECT date_id, interval_id, skill_id, scenario_id, forecasted_resources_m, forecasted_resources, forecasted_resources_incl_shrinkage_m, forecasted_resources_incl_shrinkage, scheduled_resources_m, scheduled_resources, intraday_deviation_m, relative_difference, business_unit_id, datasource_id, insert_date, update_date FROM mart.fact_schedule_forecast_skill WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE mart.fact_schedule_forecast_skill
GO
EXECUTE sp_rename N'mart.Tmp_fact_schedule_forecast_skill', N'fact_schedule_forecast_skill', 'OBJECT' 
GO
ALTER TABLE mart.fact_schedule_forecast_skill ADD CONSTRAINT
	PK_fact_schedule_forecast_skill PRIMARY KEY CLUSTERED 
	(
	date_id,
	interval_id,
	skill_id,
	scenario_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.fact_schedule_forecast_skill ADD CONSTRAINT
	FK_fact_schedule_forecast_skill_dim_date FOREIGN KEY
	(
	date_id
	) REFERENCES mart.dim_date
	(
	date_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_schedule_forecast_skill ADD CONSTRAINT
	FK_fact_schedule_forecast_skill_dim_interval FOREIGN KEY
	(
	interval_id
	) REFERENCES mart.dim_interval
	(
	interval_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_schedule_forecast_skill ADD CONSTRAINT
	FK_fact_schedule_forecast_skill_dim_scenario FOREIGN KEY
	(
	scenario_id
	) REFERENCES mart.dim_scenario
	(
	scenario_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_schedule_forecast_skill ADD CONSTRAINT
	FK_fact_schedule_forecast_skill_dim_skill FOREIGN KEY
	(
	skill_id
	) REFERENCES mart.dim_skill
	(
	skill_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
GO 
GO 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (145,'7.0.145') 
