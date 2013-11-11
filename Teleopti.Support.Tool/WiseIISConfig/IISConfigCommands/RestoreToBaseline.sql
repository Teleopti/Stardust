CREATE PROCEDURE [dbo].[RestoreToBaseline]
	@computer nvarchar(500) 
AS
BEGIN
--RestoreToBaseline 'DUMMY'
DECLARE @ana nvarchar(500) = @computer + '_TeleoptiAnalytics'
DECLARE @agg nvarchar(500) = @computer + '_TeleoptiCCCAgg'
DECLARE @app nvarchar(500) = @computer + '_TeleoptiCCC7'

DECLARE @dataFolderRelease nvarchar(4000) = 'D:\SQLData\CCC\MSSQL10_50.CCC\MSSQL\DATA\' + @computer
DECLARE @backupFolder nvarchar(4000) = 'D:\SQLData\CCC\MSSQL10_50.CCC\MSSQL\Backup\QA Baselines\'

IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = @ana)
exec ('ALTER DATABASE ' + @ana + ' SET  SINGLE_USER WITH ROLLBACK IMMEDIATE')

PRINT 'Restoring ' + @ana

exec ( 'RESTORE DATABASE ['+ @computer + '_TeleoptiAnalytics]
FROM  DISK = N''' + @backupFolder + 'DemoregBaseline_TeleoptiAnalytics.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiAnalytics_Primary'' TO N''' + @dataFolderRelease + '_TeleoptiAnalytics_Primary.mdf''
,
MOVE N''TeleoptiAnalytics_Log'' TO N''' + @dataFolderRelease + '_TeleoptiAnalytics_Log.ldf'',
MOVE N''TeleoptiAnalytics_Stage'' TO N''' + @dataFolderRelease + '_TeleoptiAnalytics_Stage.ndf'',
MOVE N''TeleoptiAnalytics_Mart'' TO N''' + @dataFolderRelease + '_TeleoptiAnalytics_Mart.ndf'',
MOVE N''TeleoptiAnalytics_Msg'' TO N''' + @dataFolderRelease + '_TeleoptiAnalytics_Msg.ndf'',
MOVE N''TeleoptiAnalytics_Rta'' TO N''' + @dataFolderRelease + '_TeleoptiAnalytics_Rta.ndf'',
NOUNLOAD,
REPLACE,
STATS = 10')

IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = @ana)
	exec ('ALTER DATABASE ' + @ana + ' SET  MULTI_USER')
	exec ('ALTER DATABASE ' +  @ana + ' SET RECOVERY SIMPLE WITH NO_WAIT')

-----
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = @agg)
	exec ('ALTER DATABASE ' + @agg + ' SET  SINGLE_USER WITH ROLLBACK IMMEDIATE')

PRINT 'Restoring '  + @agg

exec ( 'RESTORE DATABASE [' + @agg + ']
FROM  DISK = N''' + @backupFolder + 'DemoregBaseline_TeleoptiCCCAgg.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiCCCAgg_Data'' TO N''' + @dataFolderRelease + '_TeleoptiCCCAgg.mdf'',
MOVE N''TeleoptiCCCAgg_Log'' TO N''' + @dataFolderRelease + '_TeleoptiCCCAgg.ldf'',
NOUNLOAD,
REPLACE,
STATS = 10')

IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = @agg)
	exec ('ALTER DATABASE ' + @agg + ' SET  MULTI_USER')
	exec ('ALTER DATABASE '+ @agg + ' SET RECOVERY SIMPLE WITH NO_WAIT')


IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = @app)
	EXEC ('ALTER DATABASE ' + @app + ' SET  SINGLE_USER WITH ROLLBACK IMMEDIATE')

PRINT 'Restoring ' + @app

exec( 'RESTORE DATABASE [' + @app + ']
FROM DISK = N''' + @backupFolder + 'DemoregBaseline_TeleoptiCCC7.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiCCC7_Data'' TO N''' + @dataFolderRelease + '_TeleoptiCCC7.mdf'',
MOVE N''TeleoptiCCC7_Log'' TO N''' + @dataFolderRelease + '_TeleoptiCCC7.ldf'',
NOUNLOAD,
REPLACE,
STATS = 10')

exec('ALTER DATABASE ' + @app + ' SET  MULTI_USER')
exec('ALTER DATABASE ' + @app +  ' SET RECOVERY SIMPLE WITH NO_WAIT')
END

