SET PROCESSNAME=RestartService
SET PROCESSNAME2=InitializeServiceStarter
SET PROCESSNAME3=CopyLogfilesToBlob
SET PROCESSNAME4=SetRegistryAcls
::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%

IF EXIST "%DIRECTORY%\PendingReboot.txt" (
  del %DIRECTORY%\PendingReboot.txt /f
  Shutdown /r /t 0
  exit /b 0
)

::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . %DIRECTORY%\%PROCESSNAME%.ps1 >> "%DIRECTORY%\StartupLog.txt" 2>&1
::execute task scheduler script
powershell . %DIRECTORY%\%PROCESSNAME2%.ps1 >> "%DIRECTORY%\StartupLog.txt" 2>&1
::copy logfiles to blob for teleoptirnd cloudservice ONLY
powershell . %DIRECTORY%\%PROCESSNAME3%.ps1 >> "%DIRECTORY%\CopyLogfilesToBlob.txt" 2>&1
::Set registry read acls to avoid errors in eventlog and app insight
powershell . %DIRECTORY%\%PROCESSNAME4%.ps1 >> "%DIRECTORY%\SetRegistryAcls.txt" 2>&1

exit /b 0

