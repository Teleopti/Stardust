@ECHO off
::========
::PreReqs
::========
::Capture:
::1) sysAdmin in SQL instance 2) SQL Account needs write permission on the folder called \Data
::And .... Run installation with UAC disabled and/or "run as administrator"

::Analys:
::1) Report Viewer 2010 => http://www.microsoft.com/download/en/details.aspx?id=6442
::2) RML utilities => http://support.microsoft.com/kb/944837
::Potential installation bug => http://blogs.msdn.com/b/psssql/archive/2008/11/12/prb-rml-utilities-setup-may-always-prompt-for-report-viewer-2008-redistributable.aspx

::========
::Resources
::========
::http://blogs.msdn.com/b/psssql/archive/2008/11/12/cumulative-update-1-to-the-rml-utilities-for-microsoft-sql-server-released.aspx
::http://blogs.msdn.com/b/psssql/archive/tags/rml+utilities/
::http://www.sqlvillage.com/Articles/14.asp

::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::init or re-init
SET /A logmanError=0
SET INSTANCE=%COMPUTERNAME%
SET MyINSTANCE=
SET PERFCOUNTERINSTANCE=SQLServer
SET Testname=
SET MaxMinutes=
SET MaxDisk=
SET /A Silent=0
SET ManualWait=0
SET READTRACE="%ProgramFiles%\Microsoft Corporation\RMLUtils\ReadTrace.exe"
SET AnalyseCmd=%ROOTDIR%\AnalyseExample.bat
SET OutPutFolder=%ROOTDIR%\Data
SET TraceOutput=%ROOTDIR%\helpers\TraceOutput.sql
SET sqlerror=0
SET /A traceid=0
SET tracefile=
SET perfmonName=SQLServerBaselineCounters
SET perfmonConfig=%ROOTDIR%\helpers\SQLServerBaselineCounters.config
call :cleanUpLogman

::If now input parameters
IF "%1"=="" (GOTO Manual) ELSE (GOTO Silent)

::Fetch input
:Silent
SET /A Silent=1
SET /A MaxMinutes=%~1
SET /A MaxDisc=%~2
SET MyINSTANCE=%~3
SET /A ManualWait=%~4
SET Conn=-E

ECHO %MaxMinutes%
ECHO %MaxDisc%
ECHO %MyINSTANCE%

GOTO Start

:Manual
CLS
ECHO Run this script in elevated mode
ECHO Run this script locally on a SQL Server.
ECHO.
ECHO note: SQL traces will potentially use a lot of I/O resources in a buzy system.
ECHO e.g put batch file on a disk device not used by Windows OS and/or SQL Server.
ECHO Trace files will be placed here: "%OutPutFolder%"
IF %Silent% EQU 0 PAUSE
cls

ECHO SQL Server instance name
SET /P MyINSTANCE=^(if you are running a default instance leave blank^):
ECHO.

CHOICE /C yn /M "Do you connect using WinAuth?"
IF ERRORLEVEL 1 SET /a WinAuth=1
IF ERRORLEVEL 2 SET /a WinAuth=0
IF %WinAuth% equ 1 Call :WinAuth
IF %WinAuth% equ 0 Call :SQLAuth
ECHO.

ECHO Max time for trace ^(minutes^)
SET /P MaxMinutes=^(to manually stop the trace, leave blank^):
IF "%MaxMinutes%"=="" (
SET /A ManualWait=1
SET /A MaxMinutes=1441
) ELSE (
SET /A ManualWait=0
SET /A MaxMinutes=%MaxMinutes%
)

IF %MaxMinutes% GTR 1440 (
ECHO I will limit the trace to 24 hours ^(1440 min^)
SET /A MaxMinutes=1440
)
ECHO.

SET /P MaxDisc=Max disk usage for the SQL trace (Gb):
ECHO.

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
cls

::Try connect and check ALTER TRACE
ECHO Connect to SQL Server ...
SQLCMD -S%INSTANCE% %Conn% -Q"declare @alterTrace int;SELECT @alterTrace=COUNT(permission_name) FROM fn_my_permissions(NULL, NULL) WHERE permission_name='ALTER TRACE';if @alterTrace<>1 RAISERROR ('not member of ALTER TRACE', 16, 127)" -h-1
IF %ERRORLEVEL% NEQ 0 GOTO noConnection
ECHO Connect to SQL Server. Done
ECHO.

IF %Silent% equ 0 Call :DBfilter

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
SQLCMD -S%INSTANCE% %Conn% -dtempdb -i"%ROOTDIR%\tsql\TraceCaptureDef_ReportMin.sql" -o"%TraceOutput%" -b -v MaxMinutes="%MaxMinutes%" FolderName="%OutPutFolder%" MaxDisc="%MaxDisc%" DBIdString="%DBIdString%"
if %ERRORLEVEL% NEQ 0 (
call :sqlerror
GOTO :quitError
) else (
echo Create SQL Server trace. Done
)

::Get the output from the trace implementation(traceid + first tracefile)
FINDSTR /C:"%ROOTDIR%" /I "%TraceOutput%" > NUL
if %errorlevel% EQU 0 (
for /f "tokens=1,2 delims=," %%g in ('more "%TraceOutput%"') do call :GetOutput %%g %%h
) else (
echo unexpected error. Could not find trace path in output. will abort script. Try clean up any unwanted traces manually:
SQLCMD -S%INSTANCE% %Conn% -Q"SELECT * FROM :: fn_trace_getinfo(default)"
ping 127.0.0.1 -n 4 > NUL
GOTO :quitError
)

::just for user to see the output
if %Silent% EQU 0 (
ECHO Set your users and/or applications in a start position.
CHOICE /C yn /M "Start tracing?"
	IF ERRORLEVEL 2 (
	Call :stopTrace
	call :cleanUpLogman
	)
)

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

IF %ManualWait% EQU 1 (
ECHO The SQL and windows trace are now running, please do not close this window
ECHO To stop the trace press the AnyKey
PAUSE
) else (
CSCRIPT "%ROOTDIR%\helpers\Sleep.vbs" %MaxMinutes%
)

call :stopTrace %traceid%
call :cleanUpLogman

::generate example call for Analyse
ECHO %READTRACE% -S%INSTANCE% %Conn% -I"%tracefile%.trc" > "%AnalyseCmd%"
goto :finished

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

IF %ManualWait%==0 logman create counter %perfmonName% -f bin -rf 0:%MaxMinutes%:00 -si 05 -v mmddhhmm -o "%OutPutFolder%\%perfmonName%" -cf "%perfmonConfig%" -max 20
set /A logmanError=%errorlevel%
IF %ManualWait%==1 logman create counter %perfmonName% -f bin -m stop -si 05 -v mmddhhmm -o "%OutPutFolder%\%perfmonName%" -cf "%perfmonConfig%" -max 20
set /A logmanError=%errorlevel%
exit /b %logmanError%

::-----------Labels----------------
:DBfilter
::filter trace by database Id?
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

:eof