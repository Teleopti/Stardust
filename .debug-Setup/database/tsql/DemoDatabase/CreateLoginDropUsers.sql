/*
:SETVAR TELEOPTICCC Training_TeleoptiCCC7
:SETVAR TELEOPTIANALYTICS Training_TeleoptiAnalytics
:SETVAR TELEOPTIAGG Training_TeleoptiCCCAgg
:SETVAR SQLLogin TeleoptiDemoUser
:SETVAR SQLPwd TeleoptiDemoPwd2
*/
--Create SQL User and Pwd
USE [master]
IF NOT EXISTS (SELECT * FROM sys.syslogins WHERE name = '$(SQLLogin)')
BEGIN
	CREATE LOGIN [$(SQLLogin)] WITH PASSWORD=N'$(SQLPwd)',
	DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF, DEFAULT_LANGUAGE=[us_english]
END
ELSE
	ALTER LOGIN [$(SQLLogin)] WITH PASSWORD=N'$(SQLPwd)', DEFAULT_LANGUAGE=[us_english]

--Re-move $(SQLLogin)
USE $(TELEOPTICCC)
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(SQLLogin)')
DROP USER [$(SQLLogin)]
GO

USE $(TELEOPTIANALYTICS)
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(SQLLogin)')
DROP USER [$(SQLLogin)]
GO

USE $(TELEOPTIAGG)
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(SQLLogin)')
DROP USER [$(SQLLogin)]
GO


