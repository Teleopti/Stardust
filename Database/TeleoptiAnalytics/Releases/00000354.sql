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

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (354,'7.1.354') 
