@ECHO off
SET CloudDNSName=%1
SET hostFile=%systemroot%\System32\drivers\etc\hosts

FINDSTR "%CloudDNSName%" "%hostFile%" > NUL
SET /A FoundString=%ERRORLEVEL%
IF %FoundString% NEQ 0 (
ECHO.>> "%hostFile%"
ECHO %CloudDNSName% 127.0.0.1>> "%hostFile%"
)