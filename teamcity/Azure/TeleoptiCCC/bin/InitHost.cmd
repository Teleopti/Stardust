SET PROCESSNAME=InitHost
::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
REM   If Task1_Success.txt exists, then Application 1 is already installed.
IF EXIST "%DIRECTORY%\PendingReboot.txt" (
  ECHO Pending reboot flag exists.. exiting %PROCESSNAME%>> "%DIRECTORY%\StartupLog.txt" 2>&1
  GOTO Finish
)

IF EXIST "%DIRECTORY%\%PROCESSNAME%_Success.txt" (
  ECHO %PROCESSNAME% has already run. >> "%DIRECTORY%\StartupLog.txt" 2>&1
  GOTO Finish
)

REM   Run your real exe task
ECHO Running %PROCESSNAME%.ps1 >> "%DIRECTORY%\StartupLog.txt" 2>&1
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . %DIRECTORY%\%PROCESSNAME%.ps1 >> "%DIRECTORY%\StartupLog.txt" 2>&1

IF %ERRORLEVEL% EQU 0 (
  REM   The application installed without error. Create a file to indicate that the task
  REM   does not need to be run again.

  ECHO This line will create a file to indicate that Application 1 installed correctly. > "%DIRECTORY%\%PROCESSNAME%_Success.txt"

) ELSE (
  REM   An error occurred. Log the error and exit with the error code.

  DATE /T >> "%DIRECTORY%\StartupLog.txt" 2>&1
  TIME /T >> "%DIRECTORY%\StartupLog.txt" 2>&1
  ECHO  An error occurred running task 1. Errorlevel = %ERRORLEVEL%. >> "%DIRECTORY%\StartupLog.txt" 2>&1

)

exit /b 0

:Finish
exit /b 0
