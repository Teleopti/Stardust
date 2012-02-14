@ECHO off

SET MyServerInstance=ARES\CCC

::Get path to this batchfile
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
::Move one level up in the folder structure
SET ROOTDIR=%ROOTDIR%\..

ECHO Starting with Team Blue....
SET TeleoptiAnalytics=demoteamblue_TeleoptiAnalytics
SET TeleoptiCCC7=demoteamblue_TeleoptiCCC7
SET TeleoptiCCCAgg=demoteamblue_TeleoptiCCCAgg

::Deploy databases
ECHO Upgrading databases...
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiAnalytics% -E -OTeleoptiAnalytics
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCC7% -E -OTeleoptiCCC7
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCCAgg% -E -OTeleoptiCCCAgg

::Encrypt passwords
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -EE
::DateOnly in Forecasts
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -FM -EE
::FirstDayOffWeek in Persons
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -PU -EE

::Deploy all cross database views
SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -Q"mart.sys_crossdatabaseview_load"

ECHO Starting with Team Red....
SET TeleoptiAnalytics=demoteamred_TeleoptiAnalytics
SET TeleoptiCCC7=demoteamred_TeleoptiCCC7
SET TeleoptiCCCAgg=demoteamred_TeleoptiCCCAgg

::Deploy databases
ECHO Upgrading databases...
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiAnalytics% -E -OTeleoptiAnalytics
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCC7% -E -OTeleoptiCCC7
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCCAgg% -E -OTeleoptiCCCAgg

::Encrypt passwords
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -EE
::DateOnly in Forecasts
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -FM -EE
::FirstDayOffWeek in Persons
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -PU -EE

::Deploy all cross database views
SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -Q"mart.sys_crossdatabaseview_load"

ECHO Starting with Team Yellow....
SET TeleoptiAnalytics=demoteamyellow_TeleoptiAnalytics
SET TeleoptiCCC7=demoteamyellow_TeleoptiCCC7
SET TeleoptiCCCAgg=demoteamyellow_TeleoptiCCCAgg

::Deploy databases
ECHO Upgrading databases...
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiAnalytics% -E -OTeleoptiAnalytics
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCC7% -E -OTeleoptiCCC7
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCCAgg% -E -OTeleoptiCCCAgg

::Encrypt passwords
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -EE
::DateOnly in Forecasts
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -FM -EE
::FirstDayOffWeek in Persons
"%ROOTDIR%\Tools\Encryption\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -PU -EE

::Deploy all cross database views
SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -Q"mart.sys_crossdatabaseview_load"

ECHO Done upgrading!
PAUSE
