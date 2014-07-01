----------------  
--Name: David Jonsson
--Date: 2013-12-18
--Desc: Bug #26329 - Adding index for faster report permissions
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_to_be_deleted')
CREATE NONCLUSTERED INDEX IX_dim_person_to_be_deleted
ON [mart].[dim_person] ([to_be_deleted])
INCLUDE ([person_id],[team_id],[business_unit_code])
GO
 
----------------  
--Name: David Jonsson
--Date: 2013-12-16
--Desc: bug #26204 - make hard join on null guid instead of isnull
----------------
--make sure we use a zero guid instead of dbNull
update mart.dim_absence
set absence_code='00000000-0000-0000-0000-000000000000'
where absence_id=-1

update mart.dim_activity
set activity_code='00000000-0000-0000-0000-000000000000'
where activity_id=-1

update mart.dim_overtime
set overtime_code='00000000-0000-0000-0000-000000000000'
where overtime_id=-1

update mart.dim_shift_category
set shift_category_code='00000000-0000-0000-0000-000000000000'
where shift_category_id =-1

--add supporting indexes to the stg_schedule view
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_activity]') AND name = N'IX_activity_code')
CREATE NONCLUSTERED INDEX [IX_activity_code] ON [mart].[dim_activity]
(
	[activity_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_absence]') AND name = N'IX_absence_code')
CREATE NONCLUSTERED INDEX [IX_absence_code] ON [mart].[dim_absence]
(
	[absence_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_scenario]') AND name = N'IX_scenario_code')
CREATE NONCLUSTERED INDEX [IX_scenario_code] ON [mart].[dim_scenario]
(
	[scenario_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_shift_category]') AND name = N'IX_shift_category_code')
CREATE NONCLUSTERED INDEX [IX_shift_category_code] ON [mart].[dim_shift_category]
(
	[shift_category_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_shift_length]') AND name = N'IX_shift_length_m')
CREATE NONCLUSTERED INDEX [IX_shift_length_m] ON [mart].[dim_shift_length]
(
	[shift_length_m] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_overtime]') AND name = N'IX_dim_overtime_code')
CREATE NONCLUSTERED INDEX [IX_dim_overtime_code] ON [mart].[dim_overtime]
(
	[overtime_code] ASC
)

GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_scenario_id')
DROP INDEX [IX_fact_schedule_scenario_id] ON [mart].[fact_schedule]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_scenario_shift_category')
DROP INDEX [IX_fact_schedule_scenario_shift_category] ON [mart].[fact_schedule]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'idx_fact_schedule_schedule_date_id')
DROP INDEX [idx_fact_schedule_schedule_date_id] ON [mart].[fact_schedule]
GO

----------------  
--Name: David Jonsson
--Date: 2013-12-18
--Desc: Bug #26329 - Adding index for faster report permissions
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_to_be_deleted')
CREATE NONCLUSTERED INDEX IX_dim_person_to_be_deleted
ON [mart].[dim_person] ([to_be_deleted])
INCLUDE ([person_id],[team_id],[business_unit_code])
GO

--messed up cross view target found in customer database
update mart.sys_crossdatabaseview_target
set target_defaultName='TeleoptiCCCAgg'
where target_id=4
GO
-- bug 28436 
UPDATE mart.report
set target = '_blank' WHERE ID = 'D45A8874-57E1-4EB9-826D-E216A4CBC45B'
GO

----------------  
--Name: David Jonsson
--Date: 2014-07-01
--Desc: Bug #27933 - Give the end user a posibility to cherrypick tables for update_stat
----------------
SET NOCOUNT OFF
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_updatestat_tables]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[sys_updatestat_tables](
	table_schema sysname NOT NULL,
	table_name sysname NOT NULL,
	options NVARCHAR(200) NULL
	CONSTRAINT [PK_sys_updatestat_tables] PRIMARY KEY CLUSTERED 
	(
		table_schema ASC,
		table_name ASC
	)
)
END

GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'sqlserver_updatestat' AND jobstep_id=87)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(87,N'sqlserver_updatestat')
GO

