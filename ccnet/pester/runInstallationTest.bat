@echo off
SETLOCAL
set rootdir=%~dp0
SET rootdir=%rootdir:~0,-1%

call "%rootdir%\runAllTests.bat" "%rootdir%\..\..\Teleopti.Support.Tool\WiseIISConfig\IISConfigCommands"