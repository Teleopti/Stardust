@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%
SET nhibDir=c:\nhib
SET nhibFile=FixMyConfig.nhib.xml
SET CCC7DB=%1
SET AnalyticsDB=%2

if "%CCC7DB%"=="" (
SET /P CCC7DB=CCC7DB: 
)

if "%AnalyticsDB%"=="" (
SET /P AnalyticsDB=AnalyticsDB: 
)
SET ConnectionStringAnalytics=Data Source^=.;Integrated Security^=True;initial Catalog^=%AnalyticsDB%;Current Language^=us_english
 
cd %ROOTDIR%

taskkill /IM WebDev.WebServer40.EXE /F
taskkill /IM WebDev.WebServer20.EXE /F
if not exist "%nhibDir%" mkdir "%nhibDir%"

set CustomConfig=%ROOTDIR%\CustomConfig.xml
call:debugConfigSetup "%CustomConfig%" "%nhibDir%" "%CCC7DB%" "%AnalyticsDB%" "%ConnectionStringAnalytics%"

::del current app.config+web.config
for /f "tokens=1,2 delims=," %%g in (ConfigFiles.txt) do if exist %%g del %%g

::copy app.config+web.config from BuildArtifacts
for /f "tokens=1,2 delims=," %%g in (ConfigFiles.txt) do copy "%%h" "%%g"

::update app.config+web.config according to your custom config
for /f "tokens=1,2 delims=," %%g in (ConfigFiles.txt) do call:Replace "%%g" "%CustomConfig%"

::update web.config with machineKey config
for /f "tokens=1,2 delims=," %%g in (ConfigFiles.txt) do SetMachineKeys "%%g"

cls
CHOICE /C yn /M "Fix my Infratest.ini?"
IF ERRORLEVEL 1 COPY Infratest.ini C:\Infratest.ini

::cleanup
XCOPY "%ROOTDIR%\%nhibFile%" "%nhibDir%" /Y
DEL "%ROOTDIR%\%nhibFile%"
DEL "%CustomConfig%"

ENDLOCAL
goto:eof

:Replace
SETLOCAL
set fileName=%~1
set CustomConfig=%~2
for /f "tokens=1,2 delims=," %%g in (%CustomConfig%) do cscript replace.vbs "%%g" "%%h" "%fileName%" > NUL
(
ENDLOCAL
)
goto:eof

:debugConfigSetup
ECHO $^(CCC7DB^),%~3>"%~1"
ECHO $^(AnalyticsDB^),%~4>>"%~1"
ECHO $^(SitePath^),%~2>>"%~1"
ECHO $^(ConnectionStringAnalytics^),%~5>>"%~1"
ECHO ^<compilation debug='false',^<compilation debug='true'>>"%~1"
goto:eof

