::@ECHO off
:: =============================================
:: Author:		DJ
:: Create date: 2010-05-17
:: Description:	Used for setting "Modify" permission on the folders where the esent database is located.
::				The esent database is used and accessed by by the Teleopti SDK => DefaultAppPool user.
::				For figures of WindowsNT versions see: http://msdn.microsoft.com/en-us/library/aa370556(VS.85).aspx
:: 
:: =============================================
:: Change Log:
:: Date			By		Description
:: =============================================
:: 2010-10-27	DJ		Added another parameter SVCLOGIN used for Windows Authetication from WISE
:: 2010-10-28	DJ		Adding file permission for ServiceBus on local esent folders
:: 2011-02-19	DJ		Adding some documentation
:: 2011-11-23	DJ		#16802 - Permission problem when running payroll because read permissions on the root (driveletter) is required
:: 2012-01-10	AF		#17583 - Need to give disk perm to IIS APPPOOL\Teleopti ASP.NET v3.5
:: 2012-02-16	DJ		#18290 - Adding some EHCO fro output from batch file
:: 2012-05-14	DJ		possible bug where we now use quotes as input from Wise
:: 2012-10-23	DJ		Change AppPool name as part of .net
:: 2013-01-05	DJ		Drop esent stuff, go for payroll instead
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
ECHO Permissions.bat "%WindowsNT%" "%SPLevel%" "%IISVersion%" "%SVCLOGIN%"

::Get path to this batchfile
SET SDKPath=%~dp0

::Development
::SET SDKPath=C:\Program Files (x86)\Teleopti\TeleoptiCCC\SDK\
::SET WindowsNT=501
::SET SPLevel=1
::SET IISVersion=5

::no input
IF "%WindowsNT%"=="" GOTO desc

::Remove trailer slash
SET SDKPath=%SDKPath:~0,-1%
SET ServiceBusPath=%SDKPath%\..\ServiceBus

::Statics
SET PermissionStyle=""
SET WinXP=501
SET Win2003=502
SET IIS8=8
SET IIS7=7
SET IIS6=6
SET IIS7PoolUser=IIS APPPOOL\Teleopti SDK
SET IIS6PoolUser=NT AUTHORITY\Network Service
SET IIS5PoolUser=IUSR_%COMPUTERNAME%

::Use first IIS digit only
SET IISVersion=%IISVersion:~0,1%

::Ititials as IIS6
SET IISPoolUser=%IIS6PoolUser%

::If iis 7 update PoolUser
IF %IISVersion% GEQ %IIS7% SET IISPoolUser=%IIS7PoolUser%
IF %IISVersion% GEQ %IIS8% SET IISPoolUser=%IIS7PoolUser%

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
SET DRIVELETTER=%SDKPath:~0,2%

::make uppercase
for %%a in (%DRIVELETTER%) do set DRIVELETTER=%%~da
for %%a in (%SystemDrive%) do set mySystemDrive=%%~da

::Switch to drive letter
%DRIVELETTER%

::Fix read and write on different payroll folders, clean out all essent files
CALL :MAIN "%SDKPath%"
CALL :MAIN "%ServiceBusPath%"

GOTO EOF

:MAIN
::Make sure we are in the folder in question (need for the FOR loop)
SET FolderPath=%~1
ECHO FolderPath is %FolderPath%
CD %FolderPath%

::call correct permission style
ECHO PermissionStyle is: %PermissionStyle%
if "%PermissionStyle%"=="cacls" GOTO cacls
if "%PermissionStyle%"=="icacls" GOTO icacls

::---------------
:cacls
ECHO Setting folder permissions on payroll using cacls ...
ECHO.

::revoke all
ECHO FOR /D %%I IN (*payroll*) DO ECHO Y| CACLS "%%I" /E /R "%IISPoolUser%"
FOR /D %%I IN (*payroll*) DO ECHO Y| CACLS "%%I" /E /R "%IISPoolUser%"

::Add permissions again
ECHO FOR /D %%I IN (*payroll*) DO ECHO Y| CACLS "%%I" /E /G "%IISPoolUser%":C
FOR /D %%I IN (*payroll*) DO ECHO Y| CACLS "%%I" /E /G "%IISPoolUser%":C

ECHO Done
ping 127.0.0.1 -n 2 > NUL
GOTO EOF
::---------------

::---------------
:icacls
ECHO Setting permissions using icacls ...
ECHO.

::revoke all
ECHO FOR /D %%I IN (*payroll*) DO icacls "%%I" /remove:g "%IISPoolUser%"
FOR /D %%I IN (*payroll*) DO icacls "%%I" /remove:g "%IISPoolUser%"

::Add permissions again
ECHO FOR /D %%I IN (*payroll*) DO icacls "%%I" /grant "%IISPoolUser%":(OI)(CI)(M)
FOR /D %%I IN (*payroll*) DO icacls "%%I" /grant "%IISPoolUser%":(OI)(CI)(M)

ECHO Done
ping 127.0.0.1 -n 2 > NUL
GOTO EOF
::---------------

:desc
CLS
ECHO Need input parameters [VersionNt] [SPLevel] [IISVersion] [SVCLOGIN:optional]
ECHO Example for Win2008 Server:
ECHO Permissions.bat 600 0 7
PAUSE
CLS
ECHO - VersionNT:
ECHO 501 for Windows XP and all its service packs
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