@ECHO OFF
SETLOCAL

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

SET BASELINE_NAME=%1
IF "%BASELINE_NAME%"=="" SET BASELINE_NAME=DemoSales

SET CD=%ROOTDIR%\..
CALL :Branch "%CD%"

SET SqlInstance=.
SET CCC7DB=%Branch%_%BASELINE_NAME%_TeleoptiCCC7
SET AnalyticsDB=%Branch%_%BASELINE_NAME%_TeleoptiAnalytics
SET AggDB=%Branch%_%BASELINE_NAME%_TeleoptiCCCAgg

CALL "%~dp0PatchDatabase-Internal.bat" %SqlInstance% %CCC7DB% %AnalyticsDB% %AggDB%

:Branch
SET BRANCH=%~n1
SET BRANCH=%BRANCH%%~x1

ENDLOCAL
