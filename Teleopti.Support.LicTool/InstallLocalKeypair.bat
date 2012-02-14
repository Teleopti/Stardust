@echo off

::Get current path
SET LicenceToolPath=%~dp0

::Remove trailer slash
SET LicenceToolPath=%LicenceToolPath:~0,-1%

::Set keyFile and keyName
SET KeyFile=%LicenceToolPath%\Temporary.tmp
SET KeyName=Teleopti

::Delete current key
ECHO     Deleting existing keypair... 
"%LicenceToolPath%\sn.exe" -d %KeyName%
ECHO     Delete done!

::Install new key
set MyError = 0
ECHO     Creating new keypair...
"%LicenceToolPath%\sn.exe" -i "%KeyFile%" %KeyName%
SET MyError=%errorlevel%

::check myError and exit
if %MyError% neq 0 (
echo Failed to install keypair needed for license tool
ping 127.0.0.1 -n 5 >NUL
) else (
ECHO Successfully installed new key!
ping 127.0.0.1 -n 2 >NUL
)
EXIT %MyError%