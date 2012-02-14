@ECHO off
::-----------
::Counter by Microsoft Support: http://www.it-etc.com/2010/04/14/use-perfmon-to-monitor-servers-and-find-bottlenecks/
::-----------

::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::init or re-init
SET INSTANCE=%COMPUTERNAME%
SET MyINSTANCE=
SET Testname=
SET MaxMinutes=
SET MaxDisk=1
SET Silent=0
SET PERFMONRUNAS=%userdomain%\%username% *
SET ManualWait=0
SET OutPutFolder=%ROOTDIR%\Data\
SET HelpersFolder=%ROOTDIR%\Helpers\

IF NOT EXIST "%OutPutFolder%" MKDIR "%OutPutFolder%"
IF NOT EXIST "%HelpersFolder%" MKDIR "%HelpersFolder%"

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

::----------------------
::Hard Disk
ECHO "\PhysicalDisk(_Total)\%% Idle Time" > "%HelpersFolder%BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk Sec/Read" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\PhysicalDisk(_Total)\Avg. Disk Sec/Write" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\PhysicalDisk(Teleopti*)\%% Idle Time" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\PhysicalDisk(Teleopti*)\Avg. Disk Sec/Read" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\PhysicalDisk(Teleopti*)\Avg. Disk Sec/Write" >> "%HelpersFolder%BaselineCounters.config"

::Memory
ECHO "\Memory(_Total)\Available MBytes" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Paging File(_Total)\%% Usage" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Memory(Teleopti*)\%% Committed Bytes in Use" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Memory(Teleopti*)\Available MBytes" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Memory(Teleopti*)\Free System Page Table Entries" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Memory(Teleopti*)\Pool Non-Paged Bytes" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Memory(Teleopti*)\Pool Paged Bytes" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Memory(Teleopti*)\Pages/sec"  >> "%HelpersFolder%BaselineCounters.config"

::Processor
ECHO "\Processor(_Total)\%% Processor Time" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Processor(Teleopti*)\%% Processor Time" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Processor(Teleopti*)\%% User Time " >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Processor(Teleopti*)\%% Interrupt Time" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\System(Teleopti*)\Processor Queue Length" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\System(Teleopti*)\Context Switches/sec" >> "%HelpersFolder%BaselineCounters.config"

::Network
ECHO "\Network Interface(_Total)\Bytes Total/Sec" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Network Interface(_Total)\Output Queue Length" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Network Interface(Teleopti*)\Bytes Total/Sec" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Network Interface(Teleopti*)\Output Queue Length" >> "%HelpersFolder%BaselineCounters.config"

::Process
ECHO "\Process(_Total)\Private Bytes" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Process(Teleopti*)\Private Bytes" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Process(Teleopti*)\%% Privileged Time" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Process(Teleopti*)\%% Processor Time" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Process(Teleopti*)\Handle Count" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Process(Teleopti*)\Thread Count" >> "%HelpersFolder%BaselineCounters.config"

::CLR counters (IIS)
ECHO "\.NET CLR Exception(w3wp#1)\# of Excep Thrown /sec (_Global_)" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\%% Time in GC" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\# Bytes in all Heaps" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\# Gen 0 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\# Gen 1 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp#1)\# Gen 2 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Exception(w3wp)\# of Excep Thrown /sec (_Global_)" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\%% Time in GC" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\# Bytes in all Heaps" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\# Gen 0 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\# Gen 1 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(w3wp)\# Gen 2 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Process(w3wp)\ID Process" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Process(w3wp#1)\ID Process" >> "%HelpersFolder%BaselineCounters.config"

::CLR counters (Teleopti)
ECHO "\.NET CLR Exception(Teleopti*)\# of Excep Thrown /sec (_Global_)" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(Teleopti*)\%% Time in GC" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(Teleopti*)\# Bytes in all Heaps" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(Teleopti*)\# Gen 0 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(Teleopti*)\# Gen 1 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\.NET CLR Memory(Teleopti*)\# Gen 2 Collections" >> "%HelpersFolder%BaselineCounters.config"
ECHO "\Process(Teleopti*)\ID Process" >> "%HelpersFolder%BaselineCounters.config"

::WCF counters
::ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls" >> "%HelpersFolder%BaselineCounters.config"
::ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls Duration" >> "%HelpersFolder%BaselineCounters.config"
::ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls Failed Per Second" >> "%HelpersFolder%BaselineCounters.config"
::ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls Per Second" >> "%HelpersFolder%BaselineCounters.config"
::ECHO "\ServiceModelService 3.0.0.0(teleopticccsdkservice@41sdk|teleopticccsdkservice.svc)\Calls Outstanding" >> "%HelpersFolder%BaselineCounters.config"

ECHO.
ECHO Note: missleading notation from Windows on the line ....
ECHO Please type the password of your current Windows login (%userdomain%\%username%)

GOTO Start

:Start
::get IIS pid and loaded dll:s
tasklist /M /FI "IMAGENAME eq w3wp.exe" > "%OutPutFolder%iisappPIDs.txt"

::create trace
IF %ManualWait%==0 logman create counter TeleoptiPerfmon -f bin -rf 0:%MaxMinutes%:00 -si 15 -v mmddhhmm -o "%OutPutFolder%TeleoptiPerfmon" -cf "%HelpersFolder%BaselineCounters.config" -max 20 -u %PERFMONRUNAS%
IF %ManualWait%==1 logman create counter TeleoptiPerfmon -f bin -m stop -si 15 -v mmddhhmm -o "%OutPutFolder%TeleoptiPerfmon" -cf "%HelpersFolder%BaselineCounters.config" -max 20 -u %PERFMONRUNAS%

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
"%HelpersFolder%Sleep.exe" %MaxMinutes%
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