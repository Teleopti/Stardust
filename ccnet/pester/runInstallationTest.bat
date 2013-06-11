@echo off
SETLOCAL
set rootdir=%~dp0
SET rootdir=%rootdir:~0,-1%
set currentPester=2.0.3

call "%rootdir%\run.bat" "%rootdir%\..\..\Teleopti.Support.Tool\WiseIISConfig\IISConfigCommands"

set outputFile=%rootdir%\Pester.%currentPester%\Test.xml
copy "%outputFile%" "%rootdir%\..\..\%~n0.xml"

::For test by Mr.Ryding
notepad "%rootdir%\..\..\nunit.%~n0.PowerShell.xml"