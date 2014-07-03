SET FILE=%~n0
SET DIRECTORY=%~dp0
SET DIRECTORY=%DIRECTORY:~0,-1%
powershell set-executionpolicy unrestricted
powershell ". %DIRECTORY%\..\Tools\SupportTools\StartStopSystem\RestartTeleopti.ps1; main" >"%DIRECTORY%\RestartTeleopti.log"