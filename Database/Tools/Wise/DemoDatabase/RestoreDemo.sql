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
PRINT 'Restoring TeleoptiAnalytics_Demo'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiAnalytics_Demo')
ALTER DATABASE [TeleoptiAnalytics_Demo] SET SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiAnalytics_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiAnalytics_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiAnalytics_Primary'' TO N''' + @DataDir +'\TeleoptiAnalytics_demo_Primary.mdf'',
MOVE N''TeleoptiAnalytics_Log'' TO N''' + @DataDir +'\TeleoptiAnalytics_demo_Log.ldf'',
MOVE N''TeleoptiAnalytics_Stage'' TO N''' + @DataDir +'\TeleoptiAnalytics_demo_Stage.ndf'',
MOVE N''TeleoptiAnalytics_Mart'' TO N''' + @DataDir +'\TeleoptiAnalytics_demo_Mart.ndf'',
MOVE N''TeleoptiAnalytics_Msg'' TO N''' + @DataDir +'\TeleoptiAnalytics_demo_Msg.ndf'',
MOVE N''TeleoptiAnalytics_Rta'' TO N''' + @DataDir +'\TeleoptiAnalytics_demo_Rta.ndf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

-----
PRINT 'Restoring TeleoptiCCC7'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiCCC7_Demo')
ALTER DATABASE [TeleoptiCCC7_Demo] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiCCC7_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiCCC7_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiCCC7_Data'' TO N''' + @DataDir +'\TeleoptiCCC7_Demo.mdf'',
MOVE N''TeleoptiCCC7_Log'' TO N''' + @DataDir +'\TeleoptiCCC7_Demo.ldf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

---
PRINT 'Restoring TeleoptiCCC7Agg_Demo'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiCCC7Agg_Demo')
ALTER DATABASE [TeleoptiCCC7Agg_Demo] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiCCC7Agg_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiCCC7Agg_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiCCCAgg_Data'' TO N''' + @DataDir +'\TeleoptiCCC7Agg_Demo.mdf'',
MOVE N''TeleoptiCCCAgg_Log'' TO N''' + @DataDir +'\TeleoptiCCC7Agg_Demo.ldf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)