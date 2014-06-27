@echo off

::Depends on outer variable: %version%
SET /A ERRORLEV=0
COLOR A

::input?
IF "%Version%"=="" SET /P Version=Provide version to build: 
IF "%version%"=="" (
SET ERRORLEV=102
goto error
)

SET AZUREDIR=%~dp0
SET AZUREDIR=%AZUREDIR:~0,-1%
SET DriveLetter=%AZUREDIR:~0,2%
ECHO AZUREDIR is: %AZUREDIR%

SET buildServerRoot=%DriveLetter%\installation
SET OUTDIR=%buildServerRoot%\AzurePackage

SET AzureWork=%AZUREDIR%\Temp
SET ConfigPath=%AzureWork%
SET ContentDest=%AzureWork%\AzureContent
SET ContentSource=%AZUREDIR%\..
SET output=%AZUREOUTDIR%\%version%
SET Dependencies=\\hebe\Installation\Dependencies\ccc7_server
SET AzureDependencies=\\hebe\Installation\Dependencies\ccc7_azure

::Get us to correct reletive location
%DriveLetter%
CD "%AZUREDIR%"

::Clean out previous build
IF EXIST "%AzureWork%" RMDIR /Q /S "%AzureWork%"

::create working + output dir
IF NOT EXIST "%AzureWork%" MKDIR "%AzureWork%"
IF NOT EXIST "%output%" MKDIR "%output%"

::Copy StartupTask and root dir
XCOPY /e /d /y "%AZUREDIR%\TeleoptiCCC" "%ContentDest%\TeleoptiCCC\"

::Get content from Previous build (1) to Content foler (2)
Echo Getting Previous build ...
for /f "tokens=1,2 delims=," %%g in (contentMapping.txt) do ECHO ROBOCOPY "%ContentSource%\%%g" "%ContentDest%\%%h" /mir /XF *.pdb*
for /f "tokens=1,2 delims=," %%g in (contentMapping.txt) do ROBOCOPY "%ContentSource%\%%g" "%ContentDest%\%%h" /mir /XF *.pdb*
Echo Getting Previous build. Done
ECHO copy done

::Get StartPage
XCOPY /S /d /y "%ContentSource%\StartPage" "%ContentDest%\TeleoptiCCC"

::Replace URL in index.html
cscript replace.vbs "/TeleoptiWFM/" "/" "%ContentDest%\TeleoptiCCC\index.html"

::Get ReportViewer
XCOPY /d /y "%Dependencies%\ReportViewer2010.exe" "%ContentDest%\TeleoptiCCC\bin"

::Get Eventlog register Source
XCOPY /d /y "%Dependencies%\RegisterEventLogSource.exe" "%ContentDest%\TeleoptiCCC\bin"

::Get Azure stuff
robocopy %AzureDependencies% "%ContentDest%\TeleoptiCCC\bin\ccc7_azure" /mir

::deploy config
ROBOCOPY "%AZUREDIR%\Customer" "%output%" *.cscfg

::run cxpack
echo building cspack ...
ECHO "c:\Program Files\Windows Azure SDK\v1.6\bin\cspack.exe" "ServiceDefinition.csdef" /role:TeleoptiCCC;Temp\AzureContent\TeleoptiCCC /out:"%output%\Azure-%version%.cspkg"
"c:\Program Files\Windows Azure SDK\v1.6\bin\cspack.exe" "ServiceDefinition.csdef" /role:TeleoptiCCC;Temp\AzureContent\TeleoptiCCC /out:"%output%\Azure-%version%.cspkg"
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=202
GOTO :Error
)
echo building cspack. done!

GOTO :eof

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 102 ECHO No version parameter given as input for batchfile: %~nx0 
IF %ERRORLEV% EQU 103 ECHO Error calling DeployConfig.bat
IF %ERRORLEV% EQU 202 ECHO Could not build using SCPACK.exe & exit /b 202

ECHO.
ECHO --------
GOTO :EOF

:EOF
EXIT /b %ERRORLEV%