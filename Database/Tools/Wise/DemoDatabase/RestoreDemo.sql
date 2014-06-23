--:SETVAR BakDir "C:\Program Files\Teleopti\DatabaseInstaller\DemoDatabase"
--:SETVAR DataDir "C:\Program Files\Teleopti\DatabaseInstaller\DemoDatabase"
--:SETVAR LogDir "C:\Temp"

/*DECLARE @DataDir VARCHAR(1000)
DECLARE @LogDir VARCHAR(1000)
*/
DECLARE @SQLString VARCHAR(5000)

DECLARE @DataDir nvarchar(260)
EXEC	master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\Setup',N'SQLDataRoot', @DataDir output, 'no_output'
SELECT	@DataDir = @DataDir + N'\Data'

USE master

/*
--some options to fetch location:
--Current (database doesn't not have to exists ...)
--DefultData according to regedit settings of SQL Server (usually NULL)
--master
SELECT @DataDir = SUBSTRING(physical_name, 1, (LEN(physical_name)-CHARINDEX('\',REVERSE(physical_name),0)))
FROM sys.database_files
WHERE name = 'master'

SELECT @LogDir = SUBSTRING(physical_name, 1, (LEN(physical_name)-CHARINDEX('\',REVERSE(physical_name),0)))
FROM sys.database_files
WHERE name = 'mastlog'
*/
---
PRINT 'Restoring TeleoptiWFMAnalytics_Demo'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiWFMAnalytics_Demo')
ALTER DATABASE [TeleoptiAnalytics_Demo] SET SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiWFMAnalytics_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiAnalytics_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiAnalytics_Primary'' TO N''' + @DataDir +'\TeleoptiWFMAnalytics_demo_Primary.mdf'',
MOVE N''TeleoptiAnalytics_Log'' TO N''' + @DataDir +'\TeleoptiWFMAnalytics_demo_Log.ldf'',
MOVE N''TeleoptiAnalytics_Stage'' TO N''' + @DataDir +'\TeleoptiWFMAnalytics_demo_Stage.ndf'',
MOVE N''TeleoptiAnalytics_Mart'' TO N''' + @DataDir +'\TeleoptiWFMAnalytics_demo_Mart.ndf'',
MOVE N''TeleoptiAnalytics_Msg'' TO N''' + @DataDir +'\TeleoptiWFMAnalytics_demo_Msg.ndf'',
MOVE N''TeleoptiAnalytics_Rta'' TO N''' + @DataDir +'\TeleoptiWFMAnalytics_demo_Rta.ndf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

-----
PRINT 'Restoring TeleoptiWFM'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiWFM_Demo')
ALTER DATABASE [TeleoptiCCC7_Demo] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiWFM_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiCCC7_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiCCC7_Data'' TO N''' + @DataDir +'\TeleoptiWFM_Demo.mdf'',
MOVE N''TeleoptiCCC7_Log'' TO N''' + @DataDir +'\TeleoptiWFM_Demo.ldf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

---
PRINT 'Restoring TeleoptiWFMAgg_Demo'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiWFMAgg_Demo')
ALTER DATABASE [TeleoptiCCC7Agg_Demo] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiWFMAgg_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiCCC7Agg_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiCCCAgg_Data'' TO N''' + @DataDir +'\TeleoptiWFMAgg_Demo.mdf'',
MOVE N''TeleoptiCCCAgg_Log'' TO N''' + @DataDir +'\TeleoptiWFMAgg_Demo.ldf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)
GO

--Re-move TeleoptiDemoUser. Will be re-added by DBManager
USE [TeleoptiWFMAnalytics_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]

USE [TeleoptiWFMAgg_Demo]
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'TeleoptiDemoUser')
DROP USER [TeleoptiDemoUser]

USE [TeleoptiWFM_Demo]
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