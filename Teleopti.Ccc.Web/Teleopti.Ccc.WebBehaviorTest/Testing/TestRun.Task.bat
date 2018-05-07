@echo off

set work=C:\Code\teleoptiwfm.git\
set command=%work%Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\Testing\TestRun.Task.TheTask.bat
set task=schtasks /create /tn WebBehaviorTest /tr "%command%" /sc once /st 12:00 /sd 2050-01-01 /ru SYSTEM

echo %task%

%task%

pause
