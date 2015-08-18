@ECHO off
@ECHO off

::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0
SET /A CROSSDB=0
SET /A ISCCC7=0
SET TRUNK=-T

COLOR A
cls

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

CD "%ROOTDIR%"
SET DATABASEPATH=%ROOTDIR%
SET DBMANAGER=%DATABASEPATH%\DBManager.exe
SET SECURITY=%DATABASEPATH%\Enrypted\Teleopti.Support.Security.exe

SET /P INSTANCE=Your SQL Server instance: 

CHOICE /C yn /M "Do you want to use WinAuth?"
IF %ERRORLEVEL% EQU 1 SET /a WinAuth=1
IF %ERRORLEVEL% EQU 2 SET /a WinAuth=0

::Check input
IF %WinAuth% equ 1 Call :WinAuth
IF %WinAuth% equ 0 Call :SQLAuth

SET /P DATABASEAPP=Application database name to patch:
SET /P DATABASEANAL=Analytics database name to patch:
SET /P DATABASEAGG=Agg database name to patch:
ECHO.
CHOICE /C yn /M "Will end users use SQL Login?"
IF %ERRORLEVEL% EQU 1 Call :AddSQLAppSecurity
IF %ERRORLEVEL% EQU 2 Call :AddWinAppSecurity

::Patch DB
::Upgrade DB to latest version, we now always include trunk
CD "%DBMANAGERPATH%"
"%DBMANAGER%" -S"%INSTANCE%" %Conn1% -D%DATABASEAPP% -OTeleoptiCCC7 -T %Conn3% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)
"%DBMANAGER%" -S"%INSTANCE%" %Conn1% -D%DATABASEANAL% -OTeleoptiAnalytics -T %Conn3% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)
"%DBMANAGER%" -S"%INSTANCE%" %Conn1% -D%DATABASEAGG% -OTeleoptiCCCAgg -T %Conn3% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)

CD "%ROOTDIR%"

ECHO Running: securityexe
"%SECURITY%" -DS"%INSTANCE%" -AP"%DATABASEAPP%" -AN"%DATABASEANAL%" -CD"%DATABASEAGG%" -CS"%TenantConnString%" %Conn2%
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=1
GOTO :error

ECHO.
ECHO upgrade successfull!
CD "%ROOTDIR%"
GOTO Finish

:AddSQLAppSecurity
SET /P SQLAppLogin=SQL Login: 
SET /P SQLAppPwd=SQL password: 
SET Conn3=-R -L"%SQLAppLogin%:%SQLAppPwd%"
SET TenantConnString=Data Source=%INSTANCE%;User Id=%SQLAppLogin%;Password=%SQLAppPwd%
goto :eof

:AddWinAppSecurity
SET /P WinAppLogin=Supply an existing Windows group in SQL Server: 
SET Conn3=-R -W"%WinAppLogin%"
SET TenantConnString=Data Source=%INSTANCE%;Trusted_Connection=True
goto :eof

::Set Auth string
:WinAuth
SET Conn1=-E
SET Conn2=-EE
goto :eof

:SQLAuth
SET /P SQLLogin=SQL Login: 
SET /P SQLPwd=SQL password: 
SET Conn1=-U%SQLLogin% -P%SQLPwd%
SET Conn2=-DU%SQLLogin% -DP%SQLPwd%
goto :eof

:Error
CD "%ROOTDIR%"
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO Could not connect Mart to Agg: EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%Branch%_%Customer%_TeleoptiCCCAgg'
IF %ERRORLEV% EQU 2 ECHO DB have a release trunk or the database is out of version sync
IF %ERRORLEV% EQU 5 ECHO Could not create views in Mart: EXEC %Branch%_%Customer%_TeleoptiAnalytics.mart.sys_crossDatabaseView_load
IF %ERRORLEV% EQU 6 ECHO Could not build DBManager.exe & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 7 ECHO Could not build Teleopti.Support.Tool & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 8 ECHO An error occured while changing FirstDayInWeek on Person
IF %ERRORLEV% EQU 9 ECHO An error occured while changing to Date Only in Forecasts
IF %ERRORLEV% EQU 10 ECHO An error occured while encrypting
IF %ERRORLEV% EQU 11 ECHO Could not restore databases
IF %ERRORLEV% EQU 12 ECHO Could not build Teleopti.Support.Security & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 13 ECHO Could not apply a product activation key on demoreg database
IF %ERRORLEV% EQU 14 ECHO Could not create empty Analytics DB
IF %ERRORLEV% EQU 15 ECHO Could not create empty Agg DB
IF %ERRORLEV% EQU 17 ECHO Failed to update msgBroker setings in Analytics
ECHO.
ECHO --------
PAUSE
GOTO :EOF


:Finish
PAUSE
