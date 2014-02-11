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
cscript replace.vbs "/TeleoptiCCC/" "/" "%ContentDest%\TeleoptiCCC\index.html"

::Get ReportViewer
XCOPY /d /y "%Dependencies%\ReportViewer2010.exe" "%ContentDest%\TeleoptiCCC\bin"

::Get Eventlog register Source
XCOPY /d /y "%Dependencies%\RegisterEventLogSource.exe" "%ContentDest%\TeleoptiCCC\bin"

::Get Azure stuff
robocopy %AzureDependencies% "%ContentDest%\TeleoptiCCC\bin\ccc7_azure" /mir

::update config and run scpack
FOR /F %%G IN ('DIR /B Customer\*.txt') DO CALL :FuncDeployConfig %%G 
IF %ERRORLEV% NEQ 0 SET ERRORLEV=103 & GOTO error

GOTO :eof

:FuncDeployConfig
IF %ERRORLEV% EQU 0 CALL DeployConfig.bat %1
SET ERRORLEV=%ERRORLEVEL%
EXIT /b %ERRORLEV%

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 102 ECHO No version parameter given as input for batchfile: %~nx0 
IF %ERRORLEV% EQU 103 ECHO Error calling DeployConfig.bat

ECHO.
ECHO --------
GOTO :EOF

:EOF
EXIT /b %ERRORLEV%