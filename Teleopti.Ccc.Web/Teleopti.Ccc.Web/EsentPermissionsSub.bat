::@ECHO off
:: =============================================
:: Author:		MattiasE
:: Create date: 2012-05-14
:: Description:	Used for setting "Modify" permission on the folders where the esent database is located.
:: 
:: =============================================
:: Change Log:
:: Date			By		Description
:: =============================================
:: 2012-05-14		MattiasE	Copied from SDK and modified	
:: =============================================
SET WindowsNT=%~1
SET SPLevel=%~2
SET IISVersion=%~3
SET SVCLOGIN=%~4

ECHO WindowsNT is: %WindowsNT%
ECHO SPLevel is: %SPLevel%
ECHO IISVersion is: %IISVersion%
ECHO SVCLOGIN is: %SVCLOGIN%

ECHO.
ECHO Call was:
ECHO EsentPermissions.bat "%WindowsNT%" "%SPLevel%" "%IISVersion%" "%SVCLOGIN%"

::Get path to this batchfile
SET WebPath=%~dp0

::Development
::SET WebPath=C:\Program Files (x86)\Teleopti\TeleoptiCCC\Web\
::SET WindowsNT=501
::SET SPLevel=1
::SET IISVersion=5

::no input
IF "%WindowsNT%"=="" GOTO desc

::Remove trailer slash
SET WebPath=%WebPath:~0,-1%

::restart IIS
IISRESET /RESTART

::Statics
SET PermissionStyle=""
SET WinXP=501
SET Win2003=502
SET IIS6=6
SET IIS7=7
SET IIS7PoolUser=IIS APPPOOL\Teleopti ASP.NET v4.0
SET IIS6PoolUser=NT AUTHORITY\Network Service
SET IIS5PoolUser=IUSR_%COMPUTERNAME%

::Use first IIS digit only
SET IISVersion=%IISVersion:~0,1%

::Ititials as IIS6
SET IISPoolUser=%IIS6PoolUser%

::If iis 7 update PoolUser
IF %IISVersion% EQU %IIS7% SET IISPoolUser=%IIS7PoolUser%

::But, if User choosed Windows Authetication during CCC installation
IF NOT "%SVCLOGIN%"=="" (
SET IISPoolUser=%SVCLOGIN%
)

::Set icacls or cacls depending on WinXP and Win2003 - sp1 etc.
if %WindowsNT% EQU %Win2003% (
if %SPLevel% LSS 2 SET PermissionStyle=cacls
)
if %WindowsNT% EQU %WinXP% (SET PermissionStyle=cacls) else (SET PermissionStyle=icacls)

::Get Install drive letter
SET DRIVELETTER=%WebPath:~0,2%

::Switch to drive letter
%DRIVELETTER%

::Fix read and write on different essent folders, clean out all essent files
CALL :MAIN "%WebPath%"

GOTO EOF


:MAIN
::Make sure we are in the folder in question (need for the FOR loop)
SET FolderPath=%1
ECHO FolderPath is %FolderPath%
CD %FolderPath%

::some output
CLS
ECHO Adding file essent file permissions for %IISPoolUser%
ECHO.

::call correct permission style
ECHO PermissionStyle is: %PermissionStyle%
if "%PermissionStyle%"=="cacls" GOTO cacls
if "%PermissionStyle%"=="icacls" GOTO icacls

::---------------
:cacls
ECHO Setting permissions using cacls ...

::revoke all
FOR /D %%I IN (*.esent) DO ECHO Y| CACLS "%%I" /E /R "%IISPoolUser%"

::Add permissions again
FOR /D %%I IN (*.esent) DO ECHO Y| CACLS "%%I" /E /G "%IISPoolUser%":C

ECHO Done
ping 127.0.0.1 -n 2 > NUL
GOTO TruncateEsent
::---------------

::---------------
:icacls
ECHO Setting permissions using icacls ...

::revoke all
FOR /D %%I IN (*.esent) DO icacls "%%I" /remove:g "%IISPoolUser%"

::Add permissions again
FOR /D %%I IN (*.esent) DO icacls "%%I" /grant "%IISPoolUser%":(OI)(CI)(M)

ECHO Done
ping 127.0.0.1 -n 2 > NUL
GOTO TruncateEsent
::---------------

::---------------
:TruncateEsent
FOR /D %%I IN (*.esent) DO DEL "%%I" /Q /S
GOTO EOF 
::---------------

::---------------
:EOF
::---------------
ECHO 502 for Windows Server 2003 and all its service packs
ECHO 600 for Windows Server 2008 and Windows Vista
ECHO 601 for Windows 7
ECHO.
ECHO - SPLevel:
ECHO The current Service Pack level of the OS
ECHO.
ECHO - IISVersion:
ECHO 5 Windows XP Professional
ECHO 6 Windows Server 2003
ECHO 7 Windows Server 2008 (and Windows Vista)
ECHO 7 Windows Server 2008 R2 (and Windows 7)
ECHO.
ECHO - SVCLOGIN:
ECHO Optional
ECHO when Teleopti is installed with Integrated Security
ECHO use this parameter to set account to be used by App Pool service.
ECHO This paramter could also be used if you need to run this
ECHO script _after_ patch/installation (scenario: customer use custom account)
PAUSE
GOTO EOF
::---------------
:EOF
::---------------