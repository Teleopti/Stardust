@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

For /f "tokens=2 delims=[]" %%G in ('ver') Do (set version=%%G)
For /f "tokens=2,3,4 delims=. " %%G in ('echo %version%') Do (set /A major=%%G & set minor=%%H & set build=%%I) 

::http://stackoverflow.com/questions/545666/how-to-translate-ms-windows-os-version-numbers-into-product-names-in-net
if %major% GEQ 6 (
	if %minor% EQU 1 call :configure_Win7_Win2008ServerR2
	if %minor% GEQ 2 call :configure_Win8_Win2012Server
)

GOTO :Finish

:configure_Win7_Win2008ServerR2
call "%ROOTDIR%\common\configure_Win7_Win2008ServerR2.bat"
exit /b

:configure_Win8_Win2012Server
powershell set-executionpolicy -scope CurrentUser unrestricted
powershell -file "%ROOTDIR%\common\configure_Win8_Win2012Server.ps1" 
exit /b

:Finish
ping 127.0.0.1 -n 3 > NUL