@ECHO off
:: =============================================
:: Author:	DJ
:: Create date: 2012-04-20
:: Description:	Used for setting "Modify" permission on a folder for a specific user
:: This file depends on SetPermission.bat where all input is handled and global variables set
:: =============================================

::Statics
SET /A WinXP=501
SET /A Win2003=502
SET IIS8=8
SET IIS7=7
SET IIS6=6
SET IIS5=5
SET IIS7PoolUser40=IIS APPPOOL\Teleopti WFM
SET IIS7PoolUser40Web=IIS APPPOOL\Teleopti Web
SET IIS7PoolUser40Broker=IIS APPPOOL\Teleopti Broker
SET IIS7PoolUser40RTA=IIS APPPOOL\Teleopti RTA
SET IIS7PoolUser40SDK=IIS APPPOOL\Teleopti SDK
SET IIS6PoolUser=NT AUTHORITY\Network Service
SET IIS5PoolUser=IUSR_%COMPUTERNAME%
SET TargetFolder=%~5


::create the TargetFolder if missing
IF NOT EXIST "%TargetFolder%" (
ECHO target folder "%TargetFolder%" is missing... re-creating!
MKDIR "%TargetFolder%"
SET /A localError=%errorlevel%
)

::IIS Log permissions
IF %IISVersion% EQU %IIS8% (
Call:SetPermissions "%IIS7PoolUser40%" %localError% localError
Call:SetPermissions "%IIS7PoolUser40Web%" %localError% localError
Call:SetPermissions "%IIS7PoolUser40Broker%" %localError% localError
Call:SetPermissions "%IIS7PoolUser40RTA%" %localError% localError
Call:SetPermissions "%IIS7PoolUser40SDK%" %localError% localError
)

IF %IISVersion% EQU %IIS7% (
Call:SetPermissions "%IIS7PoolUser40%" %localError% localError
Call:SetPermissions "%IIS7PoolUser40Web%" %localError% localError
Call:SetPermissions "%IIS7PoolUser40Broker%" %localError% localError
Call:SetPermissions "%IIS7PoolUser40RTA%" %localError% localError
Call:SetPermissions "%IIS7PoolUser40SDK%" %localError% localError
)

IF %IISVersion% EQU %IIS6% (
Call:SetPermissions "%IIS6PoolUser%" %localError% localError
)

IF %IISVersion% EQU %IIS5% (
Call:SetPermissions "%IIS5PoolUser%" %localError% localError
)

::ETL Win Service Log permissions
IF NOT "%WinETLLogin%"=="" Call:SetPermissions "%WinETLLogin%" %localError% localError

::Other Win Service Log permissions
IF NOT "%WinSvcLogin%"=="" Call:SetPermissions "%WinSvcLogin%" %localError% localError

Echo done
exit /b %localError%
goto:oef

::---------------
:SetPermissions
SETLOCAL
SET WinUser=%~1
SET /A localError=%~2

::XP
if %WindowsNT% EQU %WinXP% (
ECHO WinXP
CALL :cacls "%WinUser%" %localError% localError
)

::Win2003
if %WindowsNT% EQU %Win2003% (
 if %SPLevel% LSS 2 (
  echo Win2003 sp1
  CALL :cacls "%WinUser%" %localError% localError
 ) else (
  echo Win2003 sp2
  CALL :icacls "%WinUser%" %localError% localError
 )
)

::Above 2003
if %WindowsNT% GTR %Win2003% (
 echo Above Win2003
 CALL :icacls "%WinUser%" %localError% localError
)

(
ENDLOCAL
set "%~3=%localError%"
)
goto:eof
::---------------


::---------------
:cacls
SETLOCAL
SET SvcLogin=%~1
SET /A LocalError=%~2
ECHO Setting permissions using cacls ...
ECHO Y|CACLS "%TargetFolder%" /E /G "%SvcLogin%":F
SET /A LocalError=%LocalError%+%errorlevel%
if not "%DRIVELETTER%"=="%mySystemDrive%" ECHO CACLS "%DRIVELETTER%" /E /G "%SvcLogin%":R
if not "%DRIVELETTER%"=="%mySystemDrive%" ECHO Y|CACLS "%DRIVELETTER%" /E /G "%SvcLogin%":R
SET /A LocalError=%LocalError%+%errorlevel%
(
ENDLOCAL
set "%~3=%localError%"
)
goto:eof
::---------------


::---------------
:icacls
SETLOCAL
SET SvcLogin=%~1
SET /A LocalError=%~2
ECHO Setting permissions using icacls ...
ECHO icacls "%TargetFolder%" /grant "%SvcLogin%":(OI)(CI)M
icacls "%TargetFolder%" /grant "%SvcLogin%":(OI)(CI)M
SET /A LocalError=%LocalError%+%errorlevel%
if not "%DRIVELETTER%"=="%mySystemDrive%" ECHO icacls "%DRIVELETTER%" /grant "%SvcLogin%":R
if not "%DRIVELETTER%"=="%mySystemDrive%" icacls "%DRIVELETTER%" /grant "%SvcLogin%":R
SET /A LocalError=%LocalError%+%errorlevel%
(
ENDLOCAL
set "%~3=%localError%"
)
goto:eof
::---------------