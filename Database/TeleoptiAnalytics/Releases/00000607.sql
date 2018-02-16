--AF 2018-02-18: Pseudonymize personal data for new installs according to Teleopti Data Retention Policy
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_maintenance_configuration] WHERE configuration_id = 18)
	INSERT INTO [mart].[etl_maintenance_configuration] VALUES(18,'MonthsToKeepPersonalData',120) --Set to 3 for new and 120 for existing in migration scripts
