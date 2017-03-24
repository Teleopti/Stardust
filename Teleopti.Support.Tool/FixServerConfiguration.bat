@ECHO off

set WORKING_DIRECTORY=%~dp0
SET Kommandompigg=%WORKING_DIRECTORY%FixServerConfiguration.ps1
powershell.exe -ExecutionPolicy Bypass -File "%Kommandompigg%" >> "%WORKING_DIRECTORY%\FixServerConfiguration.log"
::PAUSE
