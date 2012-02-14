@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::init or re-init
SET INSTANCE=%COMPUTERNAME%
SET MyINSTANCE=
SET PERFCOUNTERINSTANCE=SQLServer
SET Testname=
SET MaxMinutes=
SET MaxDisk=1
SET Silent=0
SET PERFMONRUNAS=%userdomain%\%username% *
SET ManualWait=0
SET OutPutFolder=%ROOTDIR%\Data\

ECHO.
ECHO Max time for trace (minutes)
SET /P MaxMinutes=(to manually stop the trace, leave blank):

::If manual stop, limit the trace to 24 hours
IF "%MaxMinutes%" == "" (
SET ManualWait=1
SET MaxMinutes=1440
)

::clean up
echo remove old traces ...
logman stop TeleoptiPerfmon > NUL
ping 127.0.0.1 -n 3 > NUL
logman delete TeleoptiPerfmon > NUL

ECHO "\Memory\Available MBytes" > "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\Memory\Free System Page Table Entries" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\Memory\Pages Input/sec" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\Memory\Pages/sec"  >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\Paging File(_Total)\%% Usage" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\Paging File(_Total)\%% Usage Peak" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk sec/Read" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk sec/Write" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\Processor(_Total)\%% Privileged Time" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\Processor(_Total)\%% Processor Time" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\System\Context Switches/sec" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\System\Processor Queue Length" >> "%ROOTDIR%\Helpers\BaselineCounters.config"

ECHO "\.NET CLR Exception(w3wp#1)\# of Excep Thrown /sec (_Global_)" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\%% Time in GC" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\# Bytes in all Heaps" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\# Gen 0 Collections" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\# Gen 1 Collections" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\# Gen 2 Collections" >> "%ROOTDIR%\Helpers\BaselineCounters.config"

ECHO "\.NET CLR Exception(w3wp)\# of Excep Thrown /sec (_Global_)" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\%% Time in GC" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\# Bytes in all Heaps" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\# Gen 0 Collections" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\# Gen 1 Collections" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\# Gen 2 Collections" >> "%ROOTDIR%\Helpers\BaselineCounters.config"

ECHO "\Process(w3wp)\ID Process" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\Process(w3wp#1)\ID Process" >> "%ROOTDIR%\Helpers\BaselineCounters.config"

ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls Duration" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls Failed Per Second" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls Per Second" >> "%ROOTDIR%\Helpers\BaselineCounters.config"
ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls Outstanding" >> "%ROOTDIR%\Helpers\BaselineCounters.config"

cscript "%systemroot%\system32\iisapp.vbs" > "%ROOTDIR%\Data\iisappPIDs.txt"

ECHO.
ECHO Note: each trace are allowed to grow to this size
SET /P MaxDisc=Max disk usage per trace (Gb):
set /A perfMaxDisc=%MaxDisc%*1024

ECHO.
ECHO Note: missleading notation from Windows on the line ....
ECHO Please type the password of your current Windows login (%userdomain%\%username%)

GOTO Start

:Start
::create trace
IF %ManualWait%==0 logman create counter TeleoptiPerfmon -f bin -rf 0:%MaxMinutes%:00 -si 05 -v mmddhhmm -o "%ROOTDIR%\Data\TeleoptiPerfmon" -cf "%ROOTDIR%\Helpers\BaselineCounters.config" -max 20 -u %PERFMONRUNAS%
IF %ManualWait%==1 logman create counter TeleoptiPerfmon -f bin -m stop -si 05 -v mmddhhmm -o "%ROOTDIR%\Data\TeleoptiPerfmon" -cf "%ROOTDIR%\Helpers\BaselineCounters.config" -max 20 -u %PERFMONRUNAS%

::Start Windows perfmon Trace
logman start TeleoptiPerfmon
IF %ERRORLEVEL% NEQ 0 GOTO logmanerror
ECHO.

IF %ManualWait% == 1 GOTO ManualWait

::wait
:Sleep
ECHO Trace will now run for %MaxMinutes% minutes.
ECHO.
ECHO -----------------
ECHO The Windows perfmon trace are now running, please do not close this window!
ECHO Sleeping for %MaxMinutes% minutes. Zzz ....
ECHO.
"%ROOTDIR%\Helpers\Sleep.exe" %MaxMinutes%
ECHO -----------------
ECHO.
GOTO stopTrace

:ManualWait
ECHO The Windows trace are now running, please do not close this window!
ECHO To stop the trace press the "AnyKey"
PAUSE
GOTO stopTrace

:stopTrace
logman stop TeleoptiPerfmon
ping 127.0.0.1 -n 3 > NUL
logman delete TeleoptiPerfmon
GOTO finished

:logmanerror
ECHO failed to create perfmon trace. abort
logman stop TeleoptiPerfmon
GOTO eof

:finished
ECHO.
ECHO Done
ECHO zip the Data-folder and do the anlyze at your local PC
PAUSE
GOTO eof

:eof