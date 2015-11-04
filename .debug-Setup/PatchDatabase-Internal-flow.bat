@ECHO OFF
SETLOCAL

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET CD=%ROOTDIR%\..
CALL :Branch "%CD%"
SET SqlInstance=localhost
SET CCC7DB=%Branch%_DemoSales_TeleoptiCCC7
SET AnalyticsDB=%Branch%_DemoSales_TeleoptiAnalytics
SET AggDB=%Branch%_DemoSales_TeleoptiCCCAgg

CALL "%~dp0PatchDatabase-Internal.bat" %SqlInstance% %CCC7DB% %AnalyticsDB% %AggDB%

:Branch
SET BRANCH=%~n1
SET BRANCH=%BRANCH%%~x1

ENDLOCAL
