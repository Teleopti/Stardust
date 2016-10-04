SET XACT_ABORT ON
GO
:on error exit
GO
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
	ALTER LOGIN [$(SQLLogin)] WITH PASSWORD=N'$(SQLPwd)',
	DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF, DEFAULT_LANGUAGE=[us_english]

--Re-move $(SQLLogin)
USE $(TELEOPTICCC)
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(SQLLogin)')
DROP USER [$(SQLLogin)]
GO

DELETE FROM Tenant.AdminUser WHERE Name = 'FirstAdmin' AND [Password] = '###2B2E73BBB3BEE5EC6C159C0FB4E5B9A2570CD8EE###'
IF NOT EXISTS (SELECT * FROM Tenant.AdminUser WHERE Name = 'FirstAdmin')
BEGIN
	INSERT INTO Tenant.AdminUser (Name, Email, Password, AccessToken)
	VALUES ('FirstAdmin', 'first@admin.is', '###70D74A6BBA33B5972EADAD9DD449D273E1F4961D###', 'andaDummyToken')
END

USE $(TELEOPTIANALYTICS)
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(SQLLogin)')
DROP USER [$(SQLLogin)]
GO

USE $(TELEOPTIAGG)
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$(SQLLogin)')
DROP USER [$(SQLLogin)]
GO


