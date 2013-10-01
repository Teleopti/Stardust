@ECHO off
SET ROOTDIR=%~dp0

SET customer=%~1
echo %customer%

SET DATAFOLDER=D:\SQL\SQLData
SET BACKUPFOLDER=D:\SQL\SQLBackup

if "%customer%"=="" (
SET /P customer=Which prefix to restore  ^(VMdemoreg, Demoreg, TeamBlue, etc...^): 
)

ECHO Version in %customer% is:
SQLCMD -S. -E -Q"set nocount on;select max(BuildNumber) from dbo.databaseVersion" -h -1 -d%customer%_TeleoptiCCC7
ECHO SQLCMD -S. -E -i"%ROOTDIR%Restore.sql" -v CUSTOMER="%customer%" -v DATAFOLDER="%DATAFOLDER%" -v BACKUPFOLDER="%BACKUPFOLDER%"
SQLCMD -S. -E -i"%ROOTDIR%Restore.sql" -v CUSTOMER="%customer%" -v DATAFOLDER="%DATAFOLDER%" -v BACKUPFOLDER="%BACKUPFOLDER%"

::pre-pare testdata
ECHO SQLCMD -S. -E -i"%ROOTDIR%PrepareTestdata.sql" -v ccc7="%customer%_TeleoptiCCC7" -v mart="%customer%_TeleoptiAnalytics" -v agg="%customer%_TeleoptiCCCAgg"
SQLCMD -S. -E -i"%ROOTDIR%PrepareTestdata.sql" -v ccc7="%customer%_TeleoptiCCC7" -v mart="%customer%_TeleoptiAnalytics" -v agg="%customer%_TeleoptiCCCAgg"
