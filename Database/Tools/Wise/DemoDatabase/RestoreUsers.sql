--Create SQL User and Pwd
USE [master]
IF NOT EXISTS (SELECT * FROM sys.syslogins WHERE name = 'TeleoptiDemoUser')
BEGIN
	CREATE LOGIN [TeleoptiDemoUser] WITH PASSWORD=N'TeleoptiDemoPwd2',
	DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF, DEFAULT_LANGUAGE=[us_english]
END
ELSE
	ALTER LOGIN [TeleoptiDemoUser] WITH PASSWORD=N'TeleoptiDemoPwd2', DEFAULT_LANGUAGE=[us_english]

--Re-move TeleoptiDemoUser
USE [TeleoptiAnalytics_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]

USE [TeleoptiCCC7Agg_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]

USE [TeleoptiCCC7_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]

--Adding current user to Standard demo-user
DECLARE @WinUser varchar(50)
DECLARE @WinDomain varchar(50)
DECLARE @delim varchar(1)
DECLARE @commaindex int
DECLARE @csv varchar(100)
DECLARE @userid uniqueidentifier

SET @userid = '10957ad5-5489-48e0-959a-9b5e015b2b5c'
SET @delim = '\'
SELECT @csv=system_user

SELECT @commaindex = CHARINDEX(@delim, @csv)
	
SELECT @WinDomain = LEFT(@csv, @commaindex-1)

SELECT @WinUser = RIGHT(@csv, LEN(@csv) - @commaindex)

--delete all Windows domains as they stall IIS -> AD-lookup in TeleoptiPM
DELETE FROM TeleoptiCCC7_Demo.dbo.WindowsAuthenticationInfo

--insert current user and connect to @userid
INSERT INTO TeleoptiCCC7_Demo.dbo.WindowsAuthenticationInfo
SELECT
	Person=@userid,
	WindowsLogOnName=@WinUser,
	DomainName=@WinDomain

--Add currect user to IIS-users: update aspnet_users
UPDATE TeleoptiAnalytics_Demo.dbo.aspnet_Users
SET UserName=system_user,LoweredUserName=system_user
WHERE userid=@userid
