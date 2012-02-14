/* 
Trunk initiated: 
2009-06-29 
09:40
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: KJ
--Date: 2009-06-29
--Desc: Remove Agent Scorecard compact report from report list since its only used in MyTime, should not be visible in Reports module.
----------------  

IF EXISTS(SELECT 1 FROM mart.permission_report WHERE report_id=5)
	DELETE FROM mart.permission_report WHERE report_id=5
GO
IF EXISTS(SELECT 1 FROM mart.report WHERE report_id=5)
	DELETE FROM mart.report WHERE report_id=5
GO
----------------  
--Name: KJ
--Date: 2009-07-01
--Desc: New columns in 4 tables for new resource text handling. 2 tables dropped.
----------------  
drop table mart.language_translation
GO
CREATE TABLE mart.language_translation
           (
           Culture varchar(10) NOT NULL,
           language_id int NOT NULL,
           ResourceKey varchar(500) NOT NULL,
           term_language nvarchar(1000) NOT NULL, 
           term_english nvarchar(1000) NULL
           )  ON [PRIMARY]
GO
ALTER TABLE mart.language_translation ADD CONSTRAINT
           PK_language_translation PRIMARY KEY CLUSTERED 
           (
           Culture,
           ResourceKey
           ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE TABLE mart.Tmp_service_level_calculation
	(
	service_level_id int NOT NULL IDENTITY (1, 1),
	service_level_name nvarchar(100) NULL,
	resource_key nvarchar(500) NULL
	)  ON MART
GO
SET IDENTITY_INSERT mart.Tmp_service_level_calculation ON
GO
IF EXISTS(SELECT * FROM mart.service_level_calculation)
	 EXEC('INSERT INTO mart.Tmp_service_level_calculation (service_level_id, service_level_name, resource_key)
		SELECT service_level_id, service_level_name, CONVERT(nvarchar(500), term_id) FROM mart.service_level_calculation WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_service_level_calculation OFF
GO
DROP TABLE mart.service_level_calculation
GO
EXECUTE sp_rename N'mart.Tmp_service_level_calculation', N'service_level_calculation', 'OBJECT' 
GO
ALTER TABLE mart.service_level_calculation ADD CONSTRAINT
	PK_service_level_calculation PRIMARY KEY CLUSTERED 
	(
	service_level_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO

UPDATE mart.service_level_calculation
SET resource_key='ResAnsweredCallsWithinSLPerOfferedCalls'
WHERE service_level_id=1
GO
UPDATE mart.service_level_calculation
SET resource_key='ResAnsweredAndAbndCallsWithinSLPerOfferedCalls'
WHERE service_level_id=2
GO
UPDATE mart.service_level_calculation
SET resource_key='ResAnswCallsWithinSLPerAnswCalls'
WHERE service_level_id=3
GO

CREATE TABLE mart.Tmp_period_type
	(
	period_type_id int NOT NULL IDENTITY (1, 1),
	period_type_name nvarchar(100) NULL,
	resource_key nvarchar(500) NULL
	)  ON MART
GO
SET IDENTITY_INSERT mart.Tmp_period_type ON
GO
IF EXISTS(SELECT * FROM mart.period_type)
	 EXEC('INSERT INTO mart.Tmp_period_type (period_type_id, period_type_name, resource_key)
		SELECT period_type_id, period_type_name, CONVERT(nvarchar(500), term_id) FROM mart.period_type WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_period_type OFF
GO
DROP TABLE mart.period_type
GO
EXECUTE sp_rename N'mart.Tmp_period_type', N'period_type', 'OBJECT' 
GO
ALTER TABLE mart.period_type ADD CONSTRAINT
	PK_period_type PRIMARY KEY CLUSTERED 
	(
	period_type_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO

UPDATE mart.period_type
SET resource_key='ResInterval'
WHERE period_type_id=1
GO
UPDATE mart.period_type
SET resource_key='ResHalfHour'
WHERE period_type_id=2
GO
UPDATE mart.period_type
SET resource_key='ResHour'
WHERE period_type_id=3
GO
UPDATE mart.period_type
SET resource_key='ResDay'
WHERE period_type_id=4
GO
UPDATE mart.period_type
SET resource_key='ResWeek'
WHERE period_type_id=5
GO
UPDATE mart.period_type
SET resource_key='ResMonth'
WHERE period_type_id=6
GO
UPDATE mart.period_type
SET resource_key='ResWeekday'
WHERE period_type_id=7
GO

CREATE TABLE mart.Tmp_adherence_calculation
	(
	adherence_id int NOT NULL IDENTITY (1, 1),
	adherence_name nvarchar(100) NULL,
	resource_key nvarchar(500) NULL
	)  ON MART
GO
SET IDENTITY_INSERT mart.Tmp_adherence_calculation ON
GO
IF EXISTS(SELECT * FROM mart.adherence_calculation)
	 EXEC('INSERT INTO mart.Tmp_adherence_calculation (adherence_id, adherence_name, resource_key)
		SELECT adherence_id, adherence_name, CONVERT(nvarchar(500), term_id) FROM mart.adherence_calculation WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_adherence_calculation OFF
GO
DROP TABLE mart.adherence_calculation
GO
EXECUTE sp_rename N'mart.Tmp_adherence_calculation', N'adherence_calculation', 'OBJECT' 
GO
ALTER TABLE mart.adherence_calculation ADD CONSTRAINT
	PK_adherence_calculation PRIMARY KEY CLUSTERED 
	(
	adherence_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO

UPDATE mart.adherence_calculation
SET resource_key='ResReadyTimeVsScheduledReadyTime'
WHERE adherence_id=1
GO
UPDATE mart.adherence_calculation
SET resource_key='ResReadyTimeVsScheduledTime'
WHERE adherence_id=2
GO
UPDATE mart.adherence_calculation
SET resource_key='ResReadyTimeVsContractScheduleTime'
WHERE adherence_id=3
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[language_term]') AND type in (N'U'))
DROP TABLE [mart].[language_term]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[language]') AND type in (N'U'))
DROP TABLE [mart].[language]
GO

----------------  
--Name: David Jonsson
--Date: 2009-07-05
--Desc: Obosolete SPs
--		Remember to remove from them from Source Control! Else they will be built and delivered again
----------------  
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_dim_queue_create]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_dim_queue_create]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_dim_date_getId_ByDate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_dim_date_getId_ByDate]
GO

----------------  
--Name: David Jonsson
--Date: 2009-07-03
--Desc: Re-factor of dim_queue in Analytics
--		Remove Raptor Sync of Queues and ACD-logon
----------------  
SET NOCOUNT ON
GO
EXEC dbo.sp_rename
	@objname=N'[mart].[dim_acd_login].[acd_login_code]',
	@newname=N'acd_login_original_id',
	@objtype=N'COLUMN'
GO

EXEC dbo.sp_rename
	@objname=N'[mart].[dim_queue].[queue_code]',
	@newname=N'queue_original_id',
	@objtype=N'COLUMN'
GO

--Add new column: [queue_description]
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_queue_name
GO
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_datasource_id
GO
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_insert_date
GO
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_update_date
GO
ALTER TABLE mart.dim_queue
	DROP CONSTRAINT DF_dim_queue_datasource_update_date
GO
CREATE TABLE mart.Tmp_dim_queue
	(
	queue_id int NOT NULL IDENTITY (1, 1),
	queue_agg_id int NULL,
	queue_original_id nvarchar(50) NULL,
	queue_name nvarchar(100) NOT NULL,
	queue_description nvarchar(100) NULL,
	log_object_name nvarchar(100) NULL,
	datasource_id smallint NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL,
	datasource_update_date smalldatetime NULL
	)  ON MART
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_queue_name DEFAULT ('Not Defined') FOR queue_name
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_datasource_id DEFAULT ((-1)) FOR datasource_id
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_update_date DEFAULT (getdate()) FOR update_date
GO
ALTER TABLE mart.Tmp_dim_queue ADD CONSTRAINT
	DF_dim_queue_datasource_update_date DEFAULT ('1900-01-01') FOR datasource_update_date
GO
SET IDENTITY_INSERT mart.Tmp_dim_queue ON
GO
INSERT INTO mart.Tmp_dim_queue (queue_id, queue_agg_id, queue_original_id, queue_name, log_object_name, datasource_id, insert_date, update_date, datasource_update_date)
SELECT queue_id, queue_agg_id, queue_original_id, queue_name, log_object_name, datasource_id, insert_date, update_date, datasource_update_date FROM mart.dim_queue WITH (HOLDLOCK TABLOCKX)
GO
SET IDENTITY_INSERT mart.Tmp_dim_queue OFF
GO
ALTER TABLE mart.bridge_queue_workload
	DROP CONSTRAINT FK_bridge_queue_workload_dim_queue
GO
ALTER TABLE mart.fact_agent_queue
	DROP CONSTRAINT FK_fact_agent_queue_dim_queue
GO
ALTER TABLE mart.fact_queue
	DROP CONSTRAINT FK_fact_queue_dim_queue
GO
DROP TABLE mart.dim_queue
GO
EXECUTE sp_rename N'mart.Tmp_dim_queue', N'dim_queue', 'OBJECT' 
GO
ALTER TABLE mart.dim_queue ADD CONSTRAINT
	PK_dim_queue PRIMARY KEY CLUSTERED 
	(
	queue_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
CREATE NONCLUSTERED INDEX IX_dim_queue ON mart.dim_queue
	(
	queue_original_id,
	queue_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART
GO
ALTER TABLE mart.fact_queue ADD CONSTRAINT
	FK_fact_queue_dim_queue FOREIGN KEY
	(
	queue_id
	) REFERENCES mart.dim_queue
	(
	queue_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_agent_queue ADD CONSTRAINT
	FK_fact_agent_queue_dim_queue FOREIGN KEY
	(
	queue_id
	) REFERENCES mart.dim_queue
	(
	queue_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.bridge_queue_workload ADD CONSTRAINT
	FK_bridge_queue_workload_dim_queue FOREIGN KEY
	(
	queue_id
	) REFERENCES mart.dim_queue
	(
	queue_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO

 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (134,'7.0.134') 
