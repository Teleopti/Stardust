@ECHO off

set Configuration=debug
set CustomConfig=c:\CustomConfig.txt

::Read config files in debug
for /f "tokens=1,2 delims=;" %%g in (ConfigFiles-%Configuration%.txt) do if exist %%g del %%g
for /f "tokens=1,2 delims=;" %%g in (ConfigFiles-%Configuration%.txt) do copy "%%h" "%%g"
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
