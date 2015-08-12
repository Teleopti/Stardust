@ECHO off

set Configuration=debug
set CustomConfig=c:\CustomConfig.txt

if "%Configuration%"=="debug" call:debugConfigSetup

::del current config
for /f "tokens=1,2 delims=;" %%g in (ConfigFiles-%Configuration%.txt) do if exist %%g del %%g

::copy new config from BuildArtifacts
for /f "tokens=1,2 delims=;" %%g in (ConfigFiles-%Configuration%.txt) do copy "%%h" "%%g"

::update according to your custom config
for /f "tokens=1,2 delims=;" %%g in (ConfigFiles-%Configuration%.txt) do call:Replace "%%g" "%CustomConfig%"

goto:eof

:Replace
SETLOCAL
set fileName=%~1
set CustomConfig=%~2
for /f "tokens=1,2 delims=;" %%g in (%CustomConfig%) do cscript replace.vbs "%%g" "%%h" "%fileName%"
(
ENDLOCAL
)
goto:eof

:debugConfigSetup
taskkill /IM WebDev.WebServer40.EXE /F
taskkill /IM WebDev.WebServer20.EXE /F
if not exist "%CustomConfig%" (
ECHO $^(CCC7DB^);main_Demoreg_TeleoptiCCC7>"%CustomConfig%"
ECHO $^(AnalyticsDB^);main_Demoreg_TeleoptiAnalytics>>"%CustomConfig%"
)
goto:eof

