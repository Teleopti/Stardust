@ECHO off
color A
::------------------
:: note: ROOTDIR and TEAMFOUNDATION
:: comes from calling batch file
::------------------
IF "%ROOTDIR%"=="" SET LocalDIR=%~dp0
IF "%ROOTDIR%"=="" SET ROOTDIR=%LocalDIR%..

set msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set OutputDir=C:\temp\servicebus
Set ImagePath=
set /a ERRORLEV=0
set /A httpStatus=0

if not exist "%OutputDir%" mkdir "%OutputDir%"

::uninstall
CALL "%ROOTDIR%\ServiceBus\UnInstall.bat"

::Add payroll dummy .dll if not exist, else Post-build event will fail
IF EXIST "%ROOTDIR%\..\..\..\Teleopti.Ccc.Payroll\Teleopti.Ccc.Payroll\bin\Debug\Teleopti.Ccc.Payroll.dll" (
ECHO Payroll .dll exist) ELSE (ECHO dummy > "%ROOTDIR%\..\..\..\Teleopti.Ccc.Payroll\Teleopti.Ccc.Payroll\bin\Debug\Teleopti.Ccc.Payroll.dll")

ECHO %msbuild% "%ROOTDIR%\..\..\..\ServiceBus.sln" /t:Rebuild "Debug"
%msbuild% "%ROOTDIR%\..\..\..\ServiceBus.sln" /t:Build > "%OutputDir%\BusBuild.log"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=8 & GOTO :error

::Deploy
ROBOCOPY "%ROOTDIR%\..\..\..\Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.ServiceBus.Host\bin\Debug" "%OutputDir%" /E

::Install
ECHO Install ServiceBus Service
"C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe" "%OutputDir%\Teleopti.Ccc.Sdk.ServiceBus.Host.exe" /LogFile="" > "%OutputDir%\BusInstall.log"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=10 & GOTO :error

cls

ECHO Bus is installed!
PING 127.0.0.1 -n 3 > NUL

::Kick the local SDK url
Echo Browsing the SDK url
cscript "%ROOTDIR%\BrowseUrl.vbs" "http://localhost:1335/TeleoptiCCCSdkService.svc"
set /a httpStatus=%errorlevel%

if %httpStatus% neq 200 (
COLOR E
ECHO WARNING: could not send GET request to SDK
ECHO.
ECHO ---------
ECHO Run "NET START TeleoptiServiceBus" ones your SDK is running
ECHO ---------
PAUSE
)

If %httpStatus% equ 200 (
CHOICE /C yn /M "Would you like to start the Service Bus?"
IF ERRORLEVEL 2 NET START TeleoptiServiceBus
)

GOTO :Finish

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 5 ECHO Could not uninstall service & notepad "%OutputDir%\BusUninstall.log"
IF %ERRORLEV% EQU 8 ECHO Could not build projects & notepad "%OutputDir%\BusBuild.log"
IF %ERRORLEV% EQU 10 ECHO Could not install service & notepad "%OutputDir%\BusInstall.log"

ECHO.
ECHO --------
PAUSE
GOTO :EOF

:Finish
GOTO :EOF

:EOF
