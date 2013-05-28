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
SET MaxMinutes=1440
SET MaxDisk=
SET /A Silent=0
SET PERFMONRUNAS=%userdomain%\%username% *
SET ManualWait=0
SET READTRACE="%ProgramFiles%\Microsoft Corporation\RMLUtils\ReadTrace.exe"
SET AnalyseCmd=%ROOTDIR%\AnalyseExample.bat
SET OutPutFolder=%ROOTDIR%\Data\
SET TraceOutput=%ROOTDIR%\Helpers\TraceOutput.sql
SET sqlerror=0
SET /A traceid=0
SET tracefile=
SET winPerfmon=SQL2005BaselineCounters

::If now input parameters
IF "%1"=="" (GOTO Manual) ELSE (GOTO Silent)

::Fetch input
:Silent
SET /A Silent=1
SET MaxMinutes=%1
SET MaxDisc=%2
SET PERFMONRUNAS=%userdomain%\%username% %3
SET MyINSTANCE=%4
SET Conn=-E

ECHO %MaxMinutes%
ECHO %MaxDisc%
ECHO %PERFMONRUNAS%
ECHO %MyINSTANCE%

SET INSTANCE=%COMPUTERNAME%\%MyINSTANCE%
SET PERFCOUNTERINSTANCE=MSSQL$%MyINSTANCE%
GOTO Start

:Manual
CLS
ECHO Run this script locally on a SQL Server.
ECHO.
ECHO SQL traces will potentially use a lot of I/O resources in a buzy system.
ECHO If possible; run this batch file from disk device not used by Windows OS or SQL Server. Trace files will be placed here:
ECHO "%OutPutFolder%"
IF %Silent% EQU 0 PAUSE

cls

ECHO SQL Server instance name
SET /P MyINSTANCE=^(if you are running a default instance leave blank^):

IF "%MyINSTANCE%" == "" (
ECHO going for the default instance) ELSE (
SET INSTANCE=%INSTANCE%\%MyINSTANCE%
SET PERFCOUNTERINSTANCE=MSSQL$%MyINSTANCE%
)
ECHO.

CHOICE /C yn /M "Do you connect using WinAuth?"
IF ERRORLEVEL 1 SET /a WinAuth=1
IF ERRORLEVEL 2 SET /a WinAuth=0

IF %WinAuth% equ 1 Call :WinAuth
IF %WinAuth% equ 0 Call :SQLAuth

ECHO.
ECHO Max time for trace ^(minutes^)
SET /P MaxMinutes=^(to manually stop the trace, leave blank^):

::If manual stop, limit the trace to 24 hours
IF %MaxMinutes% EQU 1440 (
SET /A ManualWait=1
)

ECHO.
SET /P MaxDisc=Max disk usage for the SQL trace (Gb):

GOTO Start

:Start
call :cleanUpLogman
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
echo could not create Windows perfmon trace, will contiune with SQL trace only
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
echo.

::Get the output from the trace implementation(traceid + first tracefile)
FINDSTR /C:"%ROOTDIR%" /I "%TraceOutput%" > NUL
if %errorlevel% EQU 0 (
for /f "tokens=1,2 delims=," %%g in ('more "%TraceOutput%"') do call :GetOutput %%g %%h
) else (
echo unexpected error. Could not find trace path in output. will abort script. Try clean up any unwanted traces manually:
SQLCMD -S%INSTANCE% %Conn% -Q"SELECT * FROM :: fn_trace_getinfo(default)"
GOTO :quitError
)

::just for user to see the output
ping 127.0.0.1 -n 4 > NUL
cls
ECHO I will now start tracing. Set your users and/or applications in a start position.  
if %Silent% EQU 0 PAUSE

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
logman start %WinPerfmon%
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
ECHO Trace will now run for %MaxMinutes% minutes.
ECHO Sleeping for %MaxMinutes% minutes. Zzz ....
ECHO.
CSCRIPT "%ROOTDIR%\Helpers\Sleep.vbs" %MaxMinutes%
ECHO.
)

call :stopTrace %traceid%
call :cleanUpLogman

::if sql 2012
if %MajorVersion% equ 11 (
ECHO.
ECHO Trace files are generated on SQL 2012, I will try to "downgrade" them ...
ping 127.0.0.1 -n 2 > NUL
powershell set-executionpolicy unrestricted
powerShell -file "%ROOTDIR%\helpers\ConvertTraceTo2008.ps1" "%ROOTDIR%\Data"
)
ECHO.

::generate example call for Analyse
ECHO %READTRACE% -S%INSTANCE% %Conn% -I"%tracefile%.trc" > "%AnalyseCmd%"
goto :finished

::-----------functions----------------
:cleanUpLogman
logman stop %WinPerfmon% > NUL
ping 127.0.0.1 -n 1 > NUL
logman delete %WinPerfmon% > NUL
IF EXIST "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config" DEL "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config" /Q /F
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
ECHO "\Memory\Available MBytes" > "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Memory\Free System Page Table Entries" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Memory\Pages Input/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Memory\Pages/sec"  >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Access Methods\Full Scans/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Access Methods\Page Splits/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Access Methods\Workfiles Created/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Access Methods\Worktables Created/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Buffer cache hit ratio" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Checkpoint pages/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Free pages" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Lazy writes/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Page life expectancy" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Page reads/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Page writes/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Buffer Manager\Stolen pages" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:General Statistics\Logins/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:General Statistics\Logouts/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:General Statistics\User Connections" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Latches\Average Latch Wait Time (ms)" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Locks(_Total)\Average Wait Time (ms)" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Locks(_Total)\Lock Requests/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Locks(_Total)\Number of Deadlocks/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Memory Manager\Target Server Memory (KB)" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:Memory Manager\Total Server Memory (KB)" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:SQL Statistics\Batch Requests/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:SQL Statistics\SQL Compilations/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\%PERFCOUNTERINSTANCE%:SQL Statistics\SQL Re-Compilations/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Paging File(_Total)\%% Usage" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Paging File(_Total)\%% Usage Peak" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk Read Queue Length" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk sec/Read" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk sec/Transfer" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk sec/Write" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk Write Queue Length" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Process(sqlservr)\%% Privileged Time" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Process(sqlservr)\%% Processor Time" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Processor(_Total)\%% Privileged Time" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\Processor(_Total)\%% Processor Time" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\System\Context Switches/sec" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"
ECHO "\System\Processor Queue Length" >> "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config"

ECHO.
ECHO Note: missleading notation from Windows on the line ....
ECHO Please type the password of your current Windows login (%userdomain%\%username%)

IF %ManualWait%==0 logman create counter %WinPerfmon% -f bin -rf 0:%MaxMinutes%:00 -si 05 -v mmddhhmm -o "%ROOTDIR%\Data\%WinPerfmon%" -cf "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config" -max 20 -u %PERFMONRUNAS%
set /A logmanError=%errorlevel%
IF %ManualWait%==1 logman create counter %WinPerfmon% -f bin -m stop -si 05 -v mmddhhmm -o "%ROOTDIR%\Data\%WinPerfmon%" -cf "%ROOTDIR%\Helpers\SQL2005BaselineCounters.config" -max 20 -u %PERFMONRUNAS%
set /A logmanError=%errorlevel%

exit /b %logmanError%

::-----------Labels----------------
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