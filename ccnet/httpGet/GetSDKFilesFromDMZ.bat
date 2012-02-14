@ECHO off

::get rootdir
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

::get destination
SET destination=%1

::if emtpy default to rootdir
IF "%destination%"=="" SET destination=%ROOTDIR%

::Allow powershell
powershell Set-ExecutionPolicy RemoteSigned

::Declare/Set
SET sourceURL=http://onlinehelp.teleopti.com/release
SET userName=tfsintegration
SET password=m8kemew0rk

::Set file list
SET FileList=ContextHelpDE.exe;ContextHelpEN.exe;ContextHelpRU.exe;ContextHelpSV.exe

:ForEachFile
for /f "tokens=1* delims=;" %%a in ("%FileList%") do (
Call :HttpGet %%a %sourceURL% %destination% %userName% %password%
SET FileList=%%b
)
if not "%FileList%" == "" goto :ForEachFile
Echo Getting file from DMZ. done 

::================
::Done
::================
GOTO :eof

::================
::Sub routines
::================
:HttpGet
ECHO powershell -File "%ROOTDIR%\HttpGet.ps1" %1 %2 %3 %4 %5
powershell -File "%ROOTDIR%\HttpGet.ps1" %1 %2 %3 %4 %5
exit /b

:eof
