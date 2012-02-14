@ECHO off
:: =============================================
:: Author:	DJ
:: Create date: 2010-10-27
:: Description:	Used for setting "Modify" permission on the log file used by ETL Service
:: 
:: =============================================
:: Change Log:
:: Date			By		Description
:: =============================================
:: when			who		what
::
:: =============================================
SET WindowsNT=%1
SET SPLevel=%2
SET SVCLOGIN=%3

::Get path to this batchfile
SET ETLSvcPath=%~dp0

::Remove trailer slash
SET ETLSvcPath=%ETLSvcPath:~0,-1%

::Logfile name
SET LogFile=EtlServiceLog.txt

::create the log file if it doesn exists
IF EXIST "%ETLSvcPath%\%LogFile%" (
COPY "%ETLSvcPath%\%LogFile%" "%ETLSvcPath%\%LogFile%.old"
DEL "%ETLSvcPath%\%LogFile%" /Q
)

::recreate
ECHO init ELL-log file > "%ETLSvcPath%\%LogFile%"

::Statics
SET WinXP=501
SET Win2003=502

::Get Install drive letter
SET DRIVELETTER=%ETLSvcPath:~0,2%

::Switch to drive letter
%DRIVELETTER%

::Make sure we are in the SDK root dir (need for the FOR loop)
CD "%ETLSvcPath%"

::icacls does not exist on WinXP and Win2003 - sp1, in that case use cacls instead
if %WindowsNT% EQU %Win2003% (
if %SPLevel% LSS 2 GOTO cacls
)

if %WindowsNT% EQU %WinXP% (GOTO cacls) else (GOTO icacls)

::---------------
:cacls
ECHO Setting permissions using cacls ...

::revoke all
CACLS "%LogFile%" /E /R "%SVCLOGIN%"

::Add permissions again
CACLS "%LogFile%" /E /G "%SVCLOGIN%":C

ECHO Done
GOTO END
::---------------

::---------------
:icacls
ECHO Setting permissions using icacls ...

::revoke all
icacls "%LogFile%" /remove:g "%SVCLOGIN%"

::Add permissions again
icacls "%LogFile%" /grant:r "%SVCLOGIN%":M

ECHO Done
GOTO END
::---------------

::---------------
:END
ping 127.0.0.1 -n 5 > NUL
::---------------