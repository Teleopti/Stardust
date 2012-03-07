@ECHO off
SET ROOTDIR=%~dp0
PAUSE
CLS
SET /P customer=Which prefix to backup ^(VMdemoreg, Demoreg, TeamBlue, etc...^): 

ECHO NOTE!!!! This will overwrite the current baseline backup
SET BACKUPFOLDER=D:\SQL\SQLBackup

ECHO.
ECHO Close this Windows to abort, else
PAUSE
SQLCMD -SPontus -E -i"%ROOTDIR%\Backup.sql" -v customer=%customer% -v BACKUPFOLDER="%BACKUPFOLDER%"