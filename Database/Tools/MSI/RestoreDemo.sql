--:SETVAR BakDir "C:\Program Files\Teleopti\DatabaseInstaller\DemoDatabase"
--:SETVAR DataDir "C:\Program Files\Teleopti\DatabaseInstaller\DemoDatabase"
--:SETVAR LogDir "C:\Temp"

/*DECLARE @DataDir VARCHAR(1000)
DECLARE @LogDir VARCHAR(1000)
*/
DECLARE @SQLString VARCHAR(5000)

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
PRINT 'Restoring TeleoptiMessaging_Demo'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiMessaging_Demo')
ALTER DATABASE [TeleoptiMessaging_Demo] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiMessaging_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiMessaging_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiMessaging_Data'' TO N''$(DataDir)\TeleoptiMessaging_Demo.mdf'',
MOVE N''TeleoptiMessaging_Log'' TO N''$(DataDir)\TeleoptiMessaging_Demo.LDF'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

---
PRINT 'Restoring TeleoptiAnalytics_Stage_Demo'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiAnalytics_Stage_Demo')
ALTER DATABASE [TeleoptiAnalytics_Stage_Demo] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiAnalytics_Stage_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiAnalytics_Stage_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiAnalytics_Stage_Data'' TO N''$(DataDir)\TeleoptiAnalytics_Stage_Demo.mdf'',
MOVE N''TeleoptiAnalytics_Stage_Log'' TO N''$(DataDir)\TeleoptiAnalytics_Stage_Demo.LDF'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

---
PRINT 'Restoring TeleoptiAnalytics_Demo'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiAnalytics_Demo')
ALTER DATABASE [TeleoptiAnalytics_Demo] SET SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [TeleoptiAnalytics_Demo]
FROM  DISK = N''$(BakDir)\TeleoptiAnalytics_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiAnalytics_Data'' TO N''$(DataDir)\TeleoptiAnalytics_Demo.mdf'',
MOVE N''TeleoptiAnalytics_Log'' TO N''$(DataDir)\TeleoptiAnalytics_Demo.LDF'',
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
MOVE N''TeleoptiCCC7_Data'' TO N''$(DataDir)\TeleoptiCCC7_Demo.mdf'',
MOVE N''TeleoptiCCC7_Log'' TO N''$(DataDir)\TeleoptiCCC7_Demo.LDF'',
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
MOVE N''TeleoptiCCCAgg_Data'' TO N''$(DataDir)\TeleoptiCCC7Agg_Demo.mdf'',
MOVE N''TeleoptiCCCAgg_Log'' TO N''$(DataDir)\TeleoptiCCC7Agg_Demo.LDF'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)