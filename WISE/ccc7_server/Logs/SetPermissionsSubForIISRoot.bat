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
SET TargetFolder=%~3


::create the TargetFolder if missing
IF NOT EXIST "%TargetFolder%" (
ECHO target folder "%TargetFolder%" is missing... re-creating!
MKDIR "%TargetFolder%"
SET /A localError=%errorlevel%
)

Call:SetPermissions "IIS_IUSRS" %localError% localError
Call:SetPermissions "IUSR" %localError% localError

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
ECHO icacls "%TargetFolder%" /grant "%SvcLogin%":(OI)(CI)R
icacls "%TargetFolder%" /grant "%SvcLogin%":(OI)(CI)R
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