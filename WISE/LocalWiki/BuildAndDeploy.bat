@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

SET Dependencies=\\hebe\Installation\Dependencies\localWiki
SET Deployment=\\A380\T-Files\Product\Teleopti CCC\v7\LocalWiki\Latest

SET WorkingFolder=C:\temp\localWiki
SET WebURL=http://wiki.teleopti.com/TeleoptiCCC
SET OutputFolder=wiki.teleopti.com\TeleoptiCCC

::all good so far
SET /A ERRORLEV=0
COLOR A
cls

::prepare an archive folder
For /f "tokens=1-2 delims=/:" %%a in ("%TIME%") do (set mytime=%%a%%b)
For /f "tokens=1-4 delims=-" %%a in ("%date%") do (set mydate=%%a%%b%%c)
set DeploymentArchive=\\A380\T-Files\Product\Teleopti CCC\v7\LocalWiki\Archive\%mydate%_%mytime%

::archive previous wiki
if not exist "%Deployment%" mkdir "%Deployment%"
if exist "%DeploymentArchive%" rmdir "%DeploymentArchive%" /S /Q
mkdir "%DeploymentArchive%"
move "%Deployment%\*.*" "%DeploymentArchive%"

::create working dir
if not exist "%WorkingFolder%" mkdir "%WorkingFolder%"

::copy needed tools to working folder
copy "%Dependencies%\7za.exe" "%WorkingFolder%\"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

copy "%Dependencies%\wget.exe" "%WorkingFolder%\"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

::deploy needed files for local setup of wiki
copy "%Dependencies%\7za.exe" "%Deployment%\"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

copy "%ROOTDIR%\InstallWiki.bat" "%Deployment%\"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

copy "%ROOTDIR%\UninstallWiki.bat" "%Deployment%\"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

::add read me.txt to the folder
echo copy "%ROOTDIR%\Readme.txt" "%Deployment%\"
copy "%ROOTDIR%\Readme.txt" "%Deployment%\"

::starting
echo Start time: %date% %time% > "%Deployment%\export.log"
echo wiki export via wget.exe at computer: %COMPUTERNAME%, user is: %USERDOMAIN%\%USERNAME% >> "%Deployment%\export.log"
echo please wait ...  >> "%Deployment%\export.log"

copy "%ROOTDIR%\ReplaceString\bin\Release\ReplaceString.exe" "%WorkingFolder%\"

::crawl the website and export to working folder
ECHO "%WorkingFolder%\wget.exe" -e robots=off -k -P"%WorkingFolder%" -r -R '*Special*' -R '*Help*' -E "%WebURL%"
"%WorkingFolder%\wget.exe" -e robots=off -k -P"%WorkingFolder%" -r -R '*Special*' -R '*Help*' -E "%WebURL%" 
SET /A wgetError=%ERRORLEVEL%
IF %wgetError% NEQ 0 SET /A ERRORLEV=2 & GOTO :error

:: remove index.php files because they are of no use.
echo cmd /C "%WorkingFolder:~0,2% & CD "%WorkingFolder%" & del index.php* /S"
cmd /C "%WorkingFolder:~0,2% & CD "%WorkingFolder%" & del index.php* /S"

::add special web.config to the folder
echo copy "%ROOTDIR%\web.config" "%WorkingFolder%\%OutputFolder%\"
copy "%ROOTDIR%\web.config" "%WorkingFolder%\%OutputFolder%\"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

::copy javascript file for replacement of translated links
echo copy "%ROOTDIR%\DynamicLinkReplace.js" "%WorkingFolder%\%OutputFolder%"
copy "%ROOTDIR%\DynamicLinkReplace.js" "%WorkingFolder%\%OutputFolder%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

::replace strings and URLs
echo "%WorkingFolder%\ReplaceString.exe" "%WorkingFolder%\%OutputFolder%"
"%WorkingFolder%\ReplaceString.exe" "%WorkingFolder%\%OutputFolder%"
SET /A replaceError=%ERRORLEVEL%
IF %replaceError% NEQ 0 SET /A ERRORLEV=4 & GOTO :error

::zip static web files into the deployment folder
ECHO "%WorkingFolder%\7za.exe" a "%Deployment%\TeleoptiCCCWiki_%mydate%_%mytime%.zip" "%WorkingFolder%\%OutputFolder%\*"
"%WorkingFolder%\7za.exe" a "%Deployment%\TeleoptiCCCWiki_%mydate%_%mytime%.zip" "%WorkingFolder%\%OutputFolder%\*"
SET /A zipError=%ERRORLEVEL%
IF %zipError% NEQ 0 SET /A ERRORLEV=3 & GOTO :error

::done
del "%Deployment%\export.log"

::clean up
rmdir "%WorkingFolder%" /S /Q

GOTO :Finish

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO Generall file or folder error, please run batch file manually to find out what goes wrong!
IF %ERRORLEV% EQU 2 ECHO wget.exe failed with error level: %wgetError%
IF %ERRORLEV% EQU 3 ECHO 7za.exe failed with error level: %7zaError%
IF %ERRORLEV% EQU 4 ECHO ReplaceString.exe failed with error level: %replaceError%
ECHO.
ECHO --------
GOTO :Finish

:Finish
::Exit with errorlevel to MsBuild
ECHO ErrorLevel is: %ERRORLEV%
::exit %ERRORLEV%
