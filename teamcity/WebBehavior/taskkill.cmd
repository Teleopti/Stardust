@ECHO OFF
SET ProcessName=%~1
ECHO taskkill.exe /IM %ProcessName% /F /FI "memusage gt 2" /t
taskkill.exe /IM %ProcessName% /F /FI "memusage gt 2" /t
EXIT %ERRORLEVEL%
