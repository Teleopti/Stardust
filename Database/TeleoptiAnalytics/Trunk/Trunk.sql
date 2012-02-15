/*
20120207 AF: Purge old data
*/

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 4 AND [configuration_name] = 'YearsToKeepFactAgent')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(4,'YearsToKeepFactAgent',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 5 AND [configuration_name] = 'YearsToKeepFactAgentQueue')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(5,'YearsToKeepFactAgentQueue',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 6 AND [configuration_name] = 'YearsToKeepFactForecastWorkload')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(6,'YearsToKeepFactForecastWorkload',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 7 AND [configuration_name] = 'YearsToKeepFactQueue')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(7,'YearsToKeepFactQueue',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 8 AND [configuration_name] = 'YearsToKeepFactRequest')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(8,'YearsToKeepFactRequest',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 9 AND [configuration_name] = 'YearsToKeepFactSchedule')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(9,'YearsToKeepFactSchedule',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 10 AND [configuration_name] = 'YearsToKeepFactScheduleDayCount')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(10,'YearsToKeepFactScheduleDayCount',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 11 AND [configuration_name] = 'YearsToKeepFactScheduleDeviation')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(11,'YearsToKeepFactScheduleDeviation',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 12 AND [configuration_name] = 'YearsToKeepFactScheduleForecastSkill')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(12,'YearsToKeepFactScheduleForecastSkill',50)

IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 13 AND [configuration_name] = 'YearsToKeepFactSchedulePreferences')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(13,'YearsToKeepFactSchedulePreferences',50)


GO
ALTER TABLE mart.report_control
ADD Id uniqueidentifier null
GO
UPDATE mart.report_control SET Id = NEWID()
GO
ALTER TABLE mart.report_control
ALTER COLUMN Id uniqueidentifier not null
GO

-- we need to know these
UPDATE mart.report_control
SET id = 'A9718D69-77A9-4D1D-9D44-DBA7EA7E92F5'
WHERE control_id = 29

UPDATE mart.report_control
SET id = '8306A37C-2134-4717-BAF6-071112AB27B6'
WHERE control_id = 30

UPDATE mart.report_control
SET id = '80770D4D-11EF-42CB-9C91-9E6A27AF35E4'
WHERE control_id = 31

UPDATE mart.report_control
SET id = 'D7E6E133-0F28-46D1-B00E-873B49C7ACDF'
WHERE control_id = 32

UPDATE mart.report_control
SET id = 'F257C91F-CD6A-4EE0-918A-3C39BD8AAF04'
WHERE control_id = 35

UPDATE mart.report_control
SET id = '5A9C7B5C-C0C6-4C31-817F-FDAA0D093B85'
WHERE control_id = 37

UPDATE mart.report_control
SET id = 'B12E74F7-48EB-4FAF-8231-B5C422F80C9A'
WHERE control_id = 39

UPDATE mart.report_control
SET id = 'AF3DCA13-71A9-4598-96A7-7EFE703F3C9F'
WHERE control_id = 3

UPDATE mart.report_control
SET id = 'E12C7EB1-23B6-4730-8019-5013C9758663'
WHERE control_id = 4

UPDATE mart.report_control
SET id = '989F6F70-29F5-43FB-9291-AA02B6503C08'
WHERE control_id = 5

UPDATE mart.report_control
SET id = 'A7EF3BC8-E7B0-4C8F-B333-FB96068A21E9'
WHERE control_id = 18

UPDATE mart.report_control
SET id = '3D4F57F4-EC28-408B-BB96-E90DEABD16AD'
WHERE control_id = 34

UPDATE mart.report_control
SET id = '6BDA3854-7915-4689-910E-9CEEE52D014B'
WHERE control_id = 36

UPDATE mart.report_control
SET id = 'EFE140D0-904A-4326-BEC2-D45945F7EC6E'
WHERE control_id = 38


ALTER TABLE mart.report_control_collection
ADD Id uniqueidentifier null
GO
UPDATE mart.report_control_collection SET Id = NEWID()
GO
ALTER TABLE mart.report_control_collection
ALTER COLUMN Id uniqueidentifier not null
GO

CREATE TABLE #collections(id int not null, collId uniqueidentifier not null)
INSERT INTO #collections SELECT DISTINCT collection_id, NEWID() FROM mart.report_control_collection

GO
ALTER TABLE mart.report_control_collection
ADD CollectionId uniqueidentifier null
GO
UPDATE mart.report_control_collection SET CollectionId = collID
FROM mart.report_control_collection c INNER JOIN #collections
ON c.collection_id = #collections.id
GO 
ALTER TABLE mart.report_control_collection
ALTER COLUMN CollectionId uniqueidentifier not null

GO

ALTER TABLE mart.report_control_collection
ADD ControlId uniqueidentifier null
GO
UPDATE mart.report_control_collection SET ControlId = c.Id
FROM mart.report_control_collection cc INNER JOIN mart.report_control c
ON cc.control_id = c.control_id
GO 
ALTER TABLE mart.report_control_collection
ALTER COLUMN ControlId uniqueidentifier not null
GO

ALTER TABLE mart.report
ADD Id uniqueidentifier null
GO
UPDATE mart.report SET Id = NEWID()
GO
ALTER TABLE mart.report
ALTER COLUMN Id uniqueidentifier not null
GO
-- we need to know this
UPDATE mart.report SET Id = 'C5B88862-F7BE-431B-A63F-3DD5FF8ACE54'
WHERE report_id = 4

UPDATE mart.report SET Id = '8DE1AB0F-32C2-4619-A2B2-97385BE4C49C'
WHERE report_id = 27



ALTER TABLE mart.report
ADD ControlCollectionId uniqueidentifier null
GO
UPDATE mart.report SET ControlCollectionId = CollectionId
FROM mart.report INNER JOIN mart.report_control_collection c
ON mart.report.control_collection_id = c.collection_id
GO
ALTER TABLE mart.report
ALTER COLUMN ControlCollectionId uniqueidentifier not null
GO

ALTER TABLE mart.report_user_setting
ADD ReportId uniqueidentifier null
GO
UPDATE mart.report_user_setting SET ReportId = Id
FROM mart.report r INNER JOIN mart.report_user_setting s
ON r.report_id = s.report_id
GO
ALTER TABLE mart.report_user_setting
ALTER COLUMN ReportId uniqueidentifier not null
GO

ALTER TABLE mart.permission_report
ADD ReportId uniqueidentifier null
GO
UPDATE mart.permission_report SET ReportId = Id
FROM mart.report r INNER JOIN mart.permission_report s
ON r.report_id = s.report_id
GO
ALTER TABLE mart.permission_report
ALTER COLUMN ReportId uniqueidentifier not null
GO
ALTER TABLE mart.permission_report
	DROP CONSTRAINT PK_permission_report
GO
ALTER TABLE mart.permission_report ADD CONSTRAINT
	PK_permission_report PRIMARY KEY CLUSTERED 
	(
	person_code,
	team_id,
	my_own,
	business_unit_id,
	ReportId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.permission_report
	DROP CONSTRAINT FK_permission_report_data_report
GO
ALTER TABLE mart.report
	DROP CONSTRAINT PK_report
GO
ALTER TABLE mart.report ADD CONSTRAINT
	PK_report PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.permission_report ADD CONSTRAINT
	FK_permission_report_report FOREIGN KEY
	(
	ReportId
	) REFERENCES mart.report
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

-- remove the old and unused stuff
ALTER TABLE mart.permission_report DROP COLUMN report_id
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_report_report_group]') AND parent_object_id = OBJECT_ID(N'[mart].[report]'))
ALTER TABLE [mart].[report] DROP CONSTRAINT [FK_report_report_group]
GO

ALTER TABLE mart.report DROP COLUMN report_group_id

DROP TABLE mart.report_group
GO

ALTER TABLE mart.report DROP COLUMN text_id
GO
ALTER TABLE mart.report_control DROP COLUMN attribute_id
GO

ALTER TABLE mart.report_user_setting
	DROP CONSTRAINT PK_report_user_setting
GO
ALTER TABLE mart.report_user_setting ADD CONSTRAINT
	PK_report_user_setting PRIMARY KEY CLUSTERED 
	(
	person_code,
	param_name,
	saved_name_id,
	ReportId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.report_user_setting SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE mart.report_user_setting
	DROP COLUMN report_id
	

GO
	
	ALTER TABLE mart.report_control_collection
	DROP CONSTRAINT FK_report_control_collection_control
GO

ALTER TABLE mart.report_control
	DROP CONSTRAINT PK_report_control
GO
ALTER TABLE mart.report_control ADD CONSTRAINT
	PK_report_control PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.report_control_collection ADD CONSTRAINT
	FK_report_control_collection_report_control FOREIGN KEY
	(
	ControlId
	) REFERENCES mart.report_control
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
