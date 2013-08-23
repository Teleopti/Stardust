@echo off
SETLOCAL
set rootdir=%~dp0
SET rootdir=%rootdir:~0,-1%
set currentPester=2.0.3

::get Carbon
robocopy "%rootdir%\Carbon" "%SystemRoot%\System32\WindowsPowerShell\v1.0\Modules\Carbon" /MIR

::run installation tests
call "%rootdir%\run.bat" "%rootdir%\..\..\Teleopti.Support.Tool\WiseIISConfig\IISConfigCommands"

set outputFile=%rootdir%\Pester.%currentPester%\Test.xml
copy "%outputFile%" "%rootdir%\..\..\nunit.%~n0.PowerShell.xml"