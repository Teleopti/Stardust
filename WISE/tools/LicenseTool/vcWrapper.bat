@echo off
::check CPU Type
if "%PROCESSOR_ARCHITECTURE%"=="x86" set ProgRoot=%ProgramFiles%
if not "%ProgramFiles(x86)%" == "" set ProgRoot=%ProgramFiles(x86)%

::change from \\UNC to local
c:

::change dir
echo cd "%ProgRoot%\Microsoft Visual Studio 9.0\VC"
cd "%ProgRoot%\Microsoft Visual Studio 9.0\VC"

::check file exist
if not exist vcvarsall.bat (
echo vcvarsall.bat does not exists!! Can't create keypair on this host.
pause
exit
)

::Set Visual Studio Stuff
call vcvarsall.bat

::Delete Keypair
sn.exe -d %1

::Create Keypair
sn.exe -i "%2" %1