@ECHO off

:: Set nodevars so node/npm/npx is available via path
call ..\..\..\packages\NodeEnv.1.0.7\nodevars.bat


SET /A ERRORLEV=0
:: Runs from [repo]\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM
npm test
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error
:: Clear all dev dependencies and only install what we need for production
call npm install rimraf
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :error
npx rimraf node_modules
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error
npm install --production
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=4 & GOTO :error
echo %errorlevel%

GOTO :EOF

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!

ECHO.
ECHO --------
Exit /B %ERRORLEV%
GOTO :EOF