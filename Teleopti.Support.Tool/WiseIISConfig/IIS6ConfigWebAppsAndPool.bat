@ECHO off
SET SSL=%~1
SET SDKCREDPROT=%~2
SET CustomIISUsr=%~3
SET CustomIISPwd=%~4
SET logfile=IIS6ConfigWebAppsAndPool.log
SET SSLPORT=443

ECHO Call was: IIS6ConfigWebAppsAndPool.bat %~1 %~2 %~3 %~4 > %logfile%
ECHO Sorry, Teleopti WFM does no longer support Win2003 and IIS6 >> %logfile%
ECHO Sorry, Teleopti WFM does no longer support Win2003 and IIS6
ping 127.0.0.1 -n 5 > NUL
