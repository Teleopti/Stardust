@ECHO OFF
SETLOCAL

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET REPODIR=%ROOTDIR:\.debug-Setup=%
FOR %%f in (%REPODIR%) DO SET REPONAME=%%~nxf
SET CCC7DB=%REPONAME%_DemoSales_TeleoptiCCC7
SET AnalyticsDB=%REPONAME%_DemoSales_TeleoptiAnalytics

CALL FixMyConfig.bat %CCC7DB% %AnalyticsDB%

ENDLOCAL