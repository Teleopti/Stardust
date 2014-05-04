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

--Only for Analytics so far.
--In RTA we use MERGE t-sql
DECLARE @max_compatibility_level tinyint
DECLARE @DBName sysname;
DECLARE @SQL nvarchar(1000);
SELECT @max_compatibility_level=max(compatibility_level) FROM sys.databases
SET @DBName = (SELECT db_name());
SELECT @SQL = 'ALTER DATABASE ' +@DBName +' SET COMPATIBILITY_LEVEL = ' + cast(@max_compatibility_level as varchar(10))

--Change demo databases to highest available compatibility_level. RTA needs this => MERGE statment is used (is under v8 licenses => SQL 2012)
IF (@max_compatibility_level) > (SELECT compatibility_level FROM sys.databases WHERE name = DB_NAME())
BEGIN
	EXEC sp_executesql @statement=@SQL
END


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
SELECT @csv=system_user

--delete all Windows domains as they stall IIS -> AD-lookup in TeleoptiPM
DELETE FROM TeleoptiCCC7_Demo.dbo.AuthenticationInfo

--insert current user and connect to @userid
INSERT INTO TeleoptiCCC7_Demo.dbo.AuthenticationInfo
SELECT
	Person=@userid,
	[Identity]=@csv

--Add currect user to IIS-users: update aspnet_users
UPDATE TeleoptiAnalytics_Demo.dbo.aspnet_Users
SET UserName=system_user,LoweredUserName=system_user
WHERE userid=@userid

--flush old RTA AcutalAgentState, else report can't handle seconds more than 24 hours
TRUNCATE TABLE TeleoptiAnalytics_Demo.rta.ActualAgentState  