SET FILE=%~n0
SET DIRECTORY=%~dp0
SET DIRECTORY=%DIRECTORY:~0,-1%
powershell set-executionpolicy unrestricted
powershell . .\RestartTeleopti.ps1 -ScriptPath "%DIRECTORY%\..\Tools\SupportTools\StartStopSystem\RestartTeleopti.ps1" >> RestartTeleopti.log
 