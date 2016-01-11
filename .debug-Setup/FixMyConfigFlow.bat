@ECHO OFF
SETLOCAL

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

CALL :Branch %~dp0..
SET CCC7DB=%Branch%_DemoSales_TeleoptiCCC7
SET AnalyticsDB=%Branch%_DemoSales_TeleoptiAnalytics

CALL %ROOTDIR%\FixMyConfig.bat %CCC7DB% %AnalyticsDB%

:Branch
SET BRANCH=%~n1
SET BRANCH=%BRANCH%%~x1
GOTO :EOF

ENDLOCAL