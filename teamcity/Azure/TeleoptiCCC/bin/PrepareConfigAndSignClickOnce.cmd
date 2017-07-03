::current dir
SET DIRECTORY=%~dp0
::remove trailer slash
SET DIRECTORY=%DIRECTORY:~0,-1%
::allow un-signed
powershell set-executionpolicy unrestricted
::execute
powershell . .\PrepareConfigAndSignClickOnce.ps1 >nul
set /A customError=%ERRORLEVEL%
::execute custom powershell script
if exist "%DIRECTORY%\CustomStartup.ps1" (
powershell -file "%DIRECTORY%\CustomStartup.ps1" >"%DIRECTORY%\CustomStartup.log"
)
exit %customError%