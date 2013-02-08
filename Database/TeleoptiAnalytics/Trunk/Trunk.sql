
--------------------------------
-- Robin: Empty the queue tables to make sure that duplicates that were created are gone.
--------------------------------
truncate table Queue.Messages
truncate table Queue.Queues
truncate table Queue.SubscriptionStorage
--Robin: Changing behavior of ETL locking

if not exists(select * from sys.columns where Name = N'lock_until' and Object_ID = Object_ID(N'mart.sys_etl_running_lock')) 
begin 
ALTER TABLE mart.sys_etl_running_lock ADD
	lock_until datetime NOT NULL CONSTRAINT DF_sys_etl_running_lock_lock_until DEFAULT dateadd(mi,1,getutcdate())
end
GO

-- Jonas: Standard reports. Change report control type for interval from and to.
IF EXISTS (select 1 from mart.report_control where control_id = 12 and control_name like 'cbo%')
	UPDATE mart.report_control SET control_name = REPLACE(control_name, 'cbo', 'time') WHERE control_id = 12
IF EXISTS (select 1 from mart.report_control where control_id = 13 and control_name like 'cbo%')
	UPDATE mart.report_control SET control_name = REPLACE(control_name, 'cbo', 'time') WHERE control_id = 13

----------------  
--Name: David and Erik
--Date: 2012-11-23  
--Desc: Bug #21451 Speed up intraday
----------------  	
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_person_dateFrom_dateTo')
DROP INDEX [IX_person_dateFrom_dateTo] ON [mart].[dim_person]
GO

CREATE NONCLUSTERED INDEX [IX_person_dateFrom_dateTo]
ON [mart].[dim_person] ([valid_from_date],[valid_to_date])
INCLUDE ([person_id],[skillset_id])
GO

----------------  
--Name: David
--Date: 2012-12-03
--Desc: Bug #21676 - Too many agents. Found some possible performance issues as well. adding a few indexes on dim x 2
----------------  	
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_acd_login]') AND name = N'IX_dim_acdLogin_datasourceId')
DROP INDEX [IX_dim_acdLogin_datasourceId] ON [mart].[dim_acd_login]
GO
CREATE NONCLUSTERED INDEX IX_dim_acdLogin_datasourceId
ON [mart].[dim_acd_login] ([datasource_id])
INCLUDE ([acd_login_id],[acd_login_original_id])
GO
---
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dimPerson_personCode_PersonBuInclude')
DROP INDEX [IX_dimPerson_personCode_PersonBuInclude] ON [mart].[dim_person]
GO
CREATE NONCLUSTERED INDEX IX_dimPerson_personCode_PersonBuInclude
ON [mart].[dim_person] ([person_code])
INCLUDE ([person_id],[business_unit_code])
---
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dimPerson_personCode_DateIncludes')
DROP INDEX [IX_dimPerson_personCode_DateIncludes] ON [mart].[dim_person]
GO
CREATE NONCLUSTERED INDEX IX_dimPerson_personCode_DateIncludes
ON [mart].[dim_person] ([person_code])
INCLUDE ([person_id],[valid_from_date_id],[valid_to_date_id],[business_unit_code])
GO

----------------  
--Name: Karin and David
--Date: 2012-12-12
--Desc: PBI21517
----------------  
UPDATE mart.report
SET help_key = REPLACE(help_key,'.html','')
WHERE RIGHT(help_key,5) = '.html'

UPDATE mart.report
SET help_key = REPLACE(help_key,'f01_','f01:')
WHERE LEFT(help_key,4) = 'f01_'

UPDATE mart.report
SET help_key = REPLACE(help_key,'_','+')
WHERE SUBSTRING(help_key,11,1) = '_'

UPDATE mart.report
SET help_key='f01:Report+AbsenceTimePerAbsence'
WHERE Id='D45A8874-57E1-4EB9-826D-E216A4CBC45B'

UPDATE mart.report
SET help_key='f01:Report+AbsenceTimePerAgent'
WHERE Id='C5B88862-F7BE-431B-A63F-3DD5FF8ACE54'

----------------  
--Name: David
--Date: 2013-02-07
--Desc: bug #22206
----------------  
--Check if previous table exists, in that case BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[permission_report]') AND type in (N'U'))
BEGIN
	CREATE TABLE [mart].[permission_report_A](
		[person_code] uniqueidentifier NOT NULL,
		[team_id] int NOT NULL,
		[my_own] bit NOT NULL,
		[business_unit_id] int NOT NULL,
		[datasource_id] smallint NOT NULL,
		[ReportId] uniqueidentifier NOT NULL,
		[datasource_update_date] smalldatetime NULL,
		[table_name] char(1) NOT NULL,
	CONSTRAINT [PK_permission_report_A] PRIMARY KEY CLUSTERED 
		(
			table_name ASC,
			[person_code] ASC,
			[team_id] ASC,
			[my_own] ASC,
			[business_unit_id] ASC,
			[ReportId] ASC
		),
	CONSTRAINT [CK_permission_report_A] CHECK
		(
		table_name = 'A'
		)
	)

	ALTER TABLE [mart].[permission_report_A] ADD  CONSTRAINT [DF_permission_report_A_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]

	CREATE TABLE [mart].[permission_report_B](
		[person_code] uniqueidentifier NOT NULL,
		[team_id] int NOT NULL,
		[my_own] bit NOT NULL,
		[business_unit_id] int NOT NULL,
		[datasource_id] smallint NOT NULL,
		[ReportId] uniqueidentifier NOT NULL,
		[datasource_update_date] smalldatetime NULL,
		[table_name] char(1) NOT NULL,
	CONSTRAINT [PK_permission_report_B] PRIMARY KEY CLUSTERED 
		(
			table_name ASC,
			[person_code] ASC,
			[team_id] ASC,
			[my_own] ASC,
			[business_unit_id] ASC,
			[ReportId] ASC
		),
	CONSTRAINT [CK_permission_report_B] CHECK
		(
		table_name = 'B'
		)
	)

	ALTER TABLE [mart].[permission_report_B] ADD  CONSTRAINT [DF_permission_report_B_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]

	CREATE TABLE [mart].[permission_report_active](
		[lock] [char](1) NOT NULL,
		[is_active] [char](1) NOT NULL
	)
	
	ALTER TABLE [mart].[permission_report_active] ADD CONSTRAINT PK_permission_report_active PRIMARY KEY CLUSTERED 
	(
		[lock] ASC
	)

	--only accept 'A' or 'B'
	ALTER TABLE [mart].[permission_report_active]  WITH CHECK ADD CONSTRAINT [CK_OnlyAorB] CHECK  (([is_active]='B' OR [is_active]='A'))
	--only accept one single row
	ALTER TABLE [mart].[permission_report_active]  WITH CHECK ADD CONSTRAINT [CK_OnlyOneRow] CHECK  (([lock]='x'))
	--default to 'x' for any new row
	ALTER TABLE [mart].[permission_report_active] ADD CONSTRAINT [DF_permission_report_active_lock] DEFAULT ('x') FOR [lock]
		
	INSERT INTO [mart].[permission_report_A]
		(person_code, team_id, my_own, business_unit_id, datasource_id, ReportId, datasource_update_date, table_name)
	SELECT [person_code]
		  ,[team_id]
		  ,[my_own]
		  ,[business_unit_id]
		  ,[datasource_id]
		  ,[ReportId]
		  ,[datasource_update_date]
		  ,'A'
	  FROM [mart].[permission_report]

	--set A as active
	INSERT INTO [mart].[permission_report_active] (is_active)
	SELECT 'A'

	DROP TABLE [mart].[permission_report]
END	
GO

--Drop obsolete SP
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_permission_data_check_test]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_permission_data_check_test]
GO
