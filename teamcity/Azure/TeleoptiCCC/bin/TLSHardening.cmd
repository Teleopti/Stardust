SET PROCESSNAME=TLSHardeningGo
::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
ECHO Running %PROCESSNAME%.ps1 >> "%DIRECTORY%\TLSHardeningGo.txt" 2>&1
powershell . %DIRECTORY%\%PROCESSNAME%.ps1 >> "%DIRECTORY%\%PROCESSNAME%.txt" 2>&1

IF %ERRORLEVEL% EQU 0 (
  REM   The application installed without error. Create a file to indicate that the task
  REM   does not need to be run again.

  ECHO This line will create a file to indicate that %PROCESSNAME%.ps1 script run correctly. > "%DIRECTORY%\%PROCESSNAME%_Success.txt"

) ELSE (
  REM   An error occurred. Log the error and exit with the error code.

  DATE /T >> "%DIRECTORY%\%PROCESSNAME%.txt" 2>&1
  TIME /T >> "%DIRECTORY%\%PROCESSNAME%.txt" 2>&1
  ECHO  An error occurred running %PROCESSNAME%.ps1. Errorlevel = %ERRORLEVEL%. >> "%DIRECTORY%\%PROCESSNAME%.txt" 2>&1

)
exit %ERRORLEVEL%