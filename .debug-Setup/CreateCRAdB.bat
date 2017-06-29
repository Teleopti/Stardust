@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0

call "%~dp0CheckMsbuildPath.bat"
IF %ERRORLEVEL% NEQ 0 GOTO :error

COLOR A
cls
SET DefaultDB=%1
IF "%DefaultDB%"=="" SET DefaultDB=TeleoptiAnalytics-root

::Default values
SET configuration=Debug
SET /A ERRORLEV=0

SET Customer=%DefaultDB%
SET TRUNK=-T -C -R -Lsa:dummyPwd
SET ROOTDIR=%ROOTDIR:~0,-1%
SET DataFolder=
SET SQLLogin=sa
SET SQLPwd=cadadi

::Get current Branch
CD "%ROOTDIR%\.."
SET HgFolder=%CD%
CALL :BRANCH "%CD%"
ECHO Current branch is: "%BRANCH%"
ECHO.

::Clean up last log files
CD "%ROOTDIR%"
IF EXIST DBManager*.log DEL DBManager*.log /Q

::Instance were the Baseline will  be restored
SET INSTANCE=%COMPUTERNAME%

::Build DbManager
ECHO Building "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" 
%MSBUILD% "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% EQU 0 (
SET DATABASEPATH="%ROOTDIR%\..\Database"
SET DBMANAGER="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\%configuration%\DBManager.exe"
SET DBMANAGERPATH="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\%configuration%"
) else (
SET /A ERRORLEV=6
GOTO :error
)

GOTO UserInput

:UserInput
cls
echo Leave blank to go with default instance ^(%INSTANCE%^)
SET /P INSTANCE=Give Named instance: %computername%\^[YourIntance^]: 

:Start

::Check if stat Databases exists, in that case leave as is
::else, create via DBManager
ECHO.
ECHO ------
ECHO Create new database ...


::create Analytics
ECHO %DBMANAGER% -S%INSTANCE% -D"TeleoptiAnalytics-root" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics% -F"%DATABASEPATH%" 
%DBMANAGER% -S%INSTANCE% -D"TeleoptiAnalytics-root" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics% -F"%DATABASEPATH%" 
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

ECHO Create databases. Done!
ECHO ------
ECHO.
goto:eof
:EOF