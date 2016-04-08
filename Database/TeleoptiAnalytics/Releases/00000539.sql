--views
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_permission_report]'))
DROP VIEW [mart].[v_permission_report]
GO

--config to be used for maintenance
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 17 AND [configuration_name] = 'DaysToKeepUnchangedPermissionReport')
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(17,'DaysToKeepUnchangedPermissionReport',90)

