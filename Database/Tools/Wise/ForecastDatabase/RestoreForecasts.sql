--:SETVAR BakDir "C:\Program Files\Teleopti\Forecasts\DatabaseInstaller\Forecasts"
--:SETVAR DataDir "C:\Program Files\Teleopti\Forecasts\DatabaseInstaller\Forecasts"

DECLARE @SQLString VARCHAR(5000)
DECLARE @DataDir nvarchar(260)
EXEC	master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\Setup',N'SQLDataRoot', @DataDir output, 'no_output'
SELECT	@DataDir = @DataDir + N'\Data'

USE master

IF NOT EXISTS (SELECT Name FROM sys.databases WHERE NAME = 'TeleoptiCCC_Forecasts')
BEGIN
	PRINT 'Restoring TeleoptiCCC_Forecasts'

	SELECT @SQLString = 'RESTORE DATABASE [TeleoptiCCC_Forecasts]
	FROM  DISK = N''$(BakDir)\TeleoptiCCC_Forecasts.BAK''
	WITH  FILE = 1,
	MOVE N''TeleoptiCCC7_Data'' TO N''' + @DataDir +'\TeleoptiCCC_Forecasts.mdf'',
	MOVE N''TeleoptiCCC7_Log'' TO N''' + @DataDir +'\TeleoptiCCC_Forecasts.LDF'',
	NOUNLOAD,
	REPLACE,
	STATS = 10'

	--SELECT (@SQLString)
	EXEC (@SQLString)
END
ELSE PRINT 'Database exist, upgrading ...'

