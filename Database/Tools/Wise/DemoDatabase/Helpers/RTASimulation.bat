@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0

::Start RTA Simulation
CALL "%ROOTDIR%\..\..\..\TeleoptiWFM\RTATools\Testapplication\Web\Teleopti.Ccc.Rta.TestApplication.exe"