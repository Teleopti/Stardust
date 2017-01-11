@ECHO off
SETLOCAL

::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

SET CODEDIR=%ROOTDIR%\..\..

::Setup toggle
::Fix configuration
CALL %CODEDIR%\.debug-setup\InfratestConfig.bat

::NUnit stuff
::Restore nuget packages
SET NugetServers="http://hestia/nuget";"https://nuget.org/api/v2"
CALL %CODEDIR%\.nuget\nuget.exe install %CODEDIR%\.nuget\packages.config -o %CODEDIR%\packages -source %NugetServers%

SET NUnit=%CODEDIR%\packages\NUnit.ConsoleRunner.3.4.1\tools\nunit3-console.exe %CODEDIR%\Teleopti.Ccc.Rta.PerformanceTest\bin\Debug\Teleopti.Ccc.Rta.PerformanceTest.dll

::Prepare database
echo Setup database
CALL %NUnit% --where="test==Teleopti.Ccc.Rta.PerformanceTest.PrepareDatabase"
if %ERRORLEVEL% NEQ 0 goto :PrepareError

::Setup RML
::init or re-init
SET /A logmanError=0
SET INSTANCE=%COMPUTERNAME%
SET MyINSTANCE=
SET PERFCOUNTERINSTANCE=SQLServer
SET MaxDisk=
SET /A Silent=0
SET READTRACE="%ProgramFiles%\Microsoft Corporation\RMLUtils\ReadTrace.exe"
SET AnalyseCmd=%ROOTDIR%\AnalyseExample.bat
SET OutPutFolder=%ROOTDIR%\IOMeasurementData
SET TraceOutput=%CODEDIR%\Teleopti.Support.Tool\SQLServerPerformance\RML\helpers\TraceOutput.sql
SET sqlerror=0
SET /A traceid=0
SET tracefile=
SET perfmonName=SQLServerBaselineCounters
SET perfmonConfig=%CODEDIR%\Teleopti.Support.Tool\SQLServerPerformance\RML\helpers\SQLServerBaselineCounters.config
call :cleanUpLogman

::If now input parameters
IF "%1"=="" (GOTO Manual) ELSE (GOTO Silent)

::Fetch input
:Silent
SET /A Silent=1
SET /A MaxDisc=%~2
SET MyINSTANCE=%~3
SET Conn=-E

ECHO %MaxDisc%
ECHO %MyINSTANCE%

GOTO Start

:Manual
::CLS
ECHO Run this script in elevated mode
ECHO Run this script locally on a SQL Server.
ECHO.
ECHO note: SQL traces will potentially use a lot of I/O resources in a buzy system.
ECHO e.g put batch file on a disk device not used by Windows OS and/or SQL Server.
ECHO Trace files will be placed here: "%OutPutFolder%"
::IF %Silent% EQU 0 PAUSE
::cls

::ECHO SQL Server instance name
::SET /P MyINSTANCE=^(if you are running a default instance leave blank^):
::ECHO.

::CHOICE /C yn /M "Do you connect using WinAuth?"
::IF ERRORLEVEL 1 SET /a WinAuth=1
::IF ERRORLEVEL 2 SET /a WinAuth=0
::IF %WinAuth% equ 1 Call :WinAuth
::IF %WinAuth% equ 0 Call :SQLAuth
::ECHO.
Call :WinAuth

::SET /P MaxDisc=Max disk usage for the SQL trace (Gb):
::ECHO.
SET MaxDisc=20

GOTO Start

:Start
IF "%MyINSTANCE%" == "" (
ECHO going for the default instance) ELSE (
SET INSTANCE=%INSTANCE%\%MyINSTANCE%
SET PERFCOUNTERINSTANCE=MSSQL$%MyINSTANCE%
)
ECHO.

set /A perfMaxDisc=%MaxDisc%*1024
IF NOT EXIST "%OutPutFolder%" MKDIR "%OutPutFolder%"
ECHO.
::cls

::Try connect and check ALTER TRACE
ECHO Connect to SQL Server ...
SQLCMD -S%INSTANCE% %Conn% -Q"declare @alterTrace int;SELECT @alterTrace=COUNT(permission_name) FROM fn_my_permissions(NULL, NULL) WHERE permission_name='ALTER TRACE';if @alterTrace<>1 RAISERROR ('not member of ALTER TRACE', 16, 127)" -h-1
IF %ERRORLEVEL% NEQ 0 GOTO noConnection
ECHO Connect to SQL Server. Done
ECHO.

::IF %Silent% equ 0 Call :DBfilter

::Get SQL Server version
sqlcmd -S%INSTANCE% %Conn% -Q"SELECT cast(SERVERPROPERTY('ProductVersion') as nvarchar(20))" -W -h-1 -o ProductVersion.txt 
set /P ProductVersion= <ProductVersion.txt
del ProductVersion.txt
set /A MajorVersion=%ProductVersion:~0,2%
ECHO ProductVersion is: %ProductVersion%, MajorVersion is: %MajorVersion%

::Try create a Windows perfmon trace
ECHO Create Windows Perfmon trace ...
call :createPerfmonTrace
if %logmanError% neq 0 (
echo could not create Windows perfmon trace, make sure to run in Elevated mode!
echo will contiune with SQL trace only
call :cleanUpLogman
)
ECHO Create Windows Perfmon trace. Done
echo.

::Try create a SQL server side trace
echo Create SQL Server trace ... 
SQLCMD -S%INSTANCE% %Conn% -dtempdb -i"%CODEDIR%\Teleopti.Support.Tool\SQLServerPerformance\RML\tsql\TraceCaptureDef_ReportMin.sql" -o"%TraceOutput%" -b -v MaxMinutes="1440" FolderName="%OutPutFolder%" MaxDisc="%MaxDisc%" DBIdString="%DBIdString%"
if %ERRORLEVEL% NEQ 0 (
call :sqlerror
GOTO :quitError
) else (
echo Create SQL Server trace. Done
)

::Get the output from the trace implementation(traceid + first tracefile)
for /f "tokens=1,2 delims=," %%g in ('more "%TraceOutput%"') do call :GetOutput %%g %%h

::Try start SQL Server trace
echo SQL Server trace start ... 
SQLCMD -S%INSTANCE% %Conn% -Q"exec sp_trace_setstatus %traceid%, 1"
if %ERRORLEVEL% NEQ 0 (
echo SQL Server trace start. Failed
call :sqlerror
GOTO :quitError
) else (
echo SQL Server trace started succesfully
)
echo.

echo Windows perfmon start ... 
logman start %perfmonName%
IF %ERRORLEVEL% NEQ 0 (
echo could not start Windows perfmon trace, will contiune with SQL trace only
call :cleanUpLogman
) else (
echo Windows perfmon started succesfully
)
echo.

ECHO Traces are running and start time is:
TIME /T
ECHO.

:: Run selected test within trace
CALL %NUnit% --where="test==Teleopti.Ccc.Rta.PerformanceTest.SendLargeBatchesTest"

:: Cleanup RML
call :stopTrace %traceid%
call :cleanUpLogman

::generate example call for Analyse
::ECHO %READTRACE% -S%INSTANCE% %Conn% -I"%tracefile%.trc" > "%AnalyseCmd%"
Call %READTRACE% -S%INSTANCE% %Conn% -I"%tracefile%.trc"
::goto :finished
goto :eof

::-----------functions----------------
:cleanUpLogman
logman stop %perfmonName% > NUL
ping 127.0.0.1 -n 1 > NUL
logman delete %perfmonName% > NUL
IF EXIST "%perfmonConfig%" DEL "%perfmonConfig%" /Q /F
exit /b

:sqlerror
ECHO.
ECHO failed to create SQL Server trace!
ping 127.0.0.1 -n 4 > NUL
MORE "%TraceOutput%"
exit /b

:stopTrace
SQLCMD -S%INSTANCE% %Conn% -Q"exec sp_trace_setstatus %traceid%, 0"
SQLCMD -S%INSTANCE% %Conn% -Q"exec sp_trace_setstatus %traceid%, 2"
exit /b

:getOutput
SET /A traceid=%1
SET tracefile=%~2
exit /b

:createPerfmonTrace
ECHO "\Memory\Available MBytes" > "%perfmonConfig%"
ECHO "\Memory\Free System Page Table Entries" >> "%perfmonConfig%"
ECHO "\Memory\Pages Input/sec" >> "%perfmonConfig%"
ECHO "\Memory\Pages/sec"  >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Access Methods\Full Scans/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Access Methods\Page Splits/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Access Methods\Workfiles Created/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Access Methods\Worktables Created/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Buffer cache hit ratio" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Checkpoint pages/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Free pages" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Lazy writes/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Page life expectancy" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Page reads/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Page writes/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Stolen pages" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:General Statistics\Logins/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:General Statistics\Logouts/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:General Statistics\User Connections" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Latches\Average Latch Wait Time (ms)" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Locks(_Total)\Average Wait Time (ms)" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Locks(_Total)\Lock Requests/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Locks(_Total)\Number of Deadlocks/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Memory Manager\Target Server Memory (KB)" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:Memory Manager\Total Server Memory (KB)" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:SQL Statistics\Batch Requests/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:SQL Statistics\SQL Compilations/sec" >> "%perfmonConfig%"
ECHO "\%PERFCOUNTERINSTANCE%:SQL Statistics\SQL Re-Compilations/sec" >> "%perfmonConfig%"
ECHO "\Paging File(_Total)\%% Usage" >> "%perfmonConfig%"
ECHO "\Paging File(_Total)\%% Usage Peak" >> "%perfmonConfig%"
ECHO "\PhysicalDisk(_Total)\Avg. Disk Read Queue Length" >> "%perfmonConfig%"
ECHO "\PhysicalDisk(_Total)\Avg. Disk sec/Read" >> "%perfmonConfig%"
ECHO "\PhysicalDisk(_Total)\Avg. Disk sec/Transfer" >> "%perfmonConfig%"
ECHO "\PhysicalDisk(_Total)\Avg. Disk sec/Write" >> "%perfmonConfig%"
ECHO "\PhysicalDisk(_Total)\Avg. Disk Write Queue Length" >> "%perfmonConfig%"
ECHO "\Process(sqlservr)\%% Privileged Time" >> "%perfmonConfig%"
ECHO "\Process(sqlservr)\%% Processor Time" >> "%perfmonConfig%"
ECHO "\Processor(_Total)\%% Privileged Time" >> "%perfmonConfig%"
ECHO "\Processor(_Total)\%% Processor Time" >> "%perfmonConfig%"
ECHO "\System\Context Switches/sec" >> "%perfmonConfig%"
ECHO "\System\Processor Queue Length" >> "%perfmonConfig%"

logman create counter %perfmonName% -f bin -m stop -si 05 -v mmddhhmm -o "%OutPutFolder%\%perfmonName%" -cf "%perfmonConfig%" -max 20
set /A logmanError=%errorlevel%
exit /b %logmanError%

::-----------Labels----------------
:DBfilter
filter trace by database Id?
CHOICE /C yn /M "Would you like to filter your trace by database_id?"
IF ERRORLEVEL 1 SET /a Filter=1
IF ERRORLEVEL 2 SET /a Filter=0
If %Filter% equ 1 (
sqlcmd -S%INSTANCE% %Conn% -Q"set nocount on;select database_id,name from sys.databases order by database_id" -W -w 60
ECHO.
ECHO Please provide a comma seprated string with integers ^(Example string: 5,8,34^), 
set /P DBIdString=representing database_id:
)
goto :eof

:PrepareError
echo Prepare failed
goto :eof

:WinAuth
SET Conn=-E
goto :eof

:SQLAuth
SET /P SQLLogin=SQL Login: 
SET /P SQLPwd=SQL password: 
SET Conn=-U%SQLLogin% -P%SQLPwd%
goto :eof

:noConnection
ECHO.
ECHO Sorry, can not connect to the %INSTANCE% or you dont have ALTER TRACE permssions
ping 127.0.0.1 -n 4 > NUL
GOTO eof

:quitError
echo something went wrong
IF %Silent% EQU 0 PAUSE
GOTO eof

:finished
ECHO Done
ECHO zip the Data-folder and do the anlyze at your local PC
IF %Silent% EQU 0 PAUSE
GOTO eof

ENDLOCAL
goto:eof

:eof
