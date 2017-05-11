@ECHO off
SET /A ERRORLEV=0
:: Runs from [repo]\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM
call ..\.node\npm run-script continuous
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error
:: Clear all dev dependencies and only install what we need for production
call ..\.node\npm install rimraf
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :error
call ..\.node\node node_modules\rimraf\bin.js node_modules
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error
call ..\.node\npm install --production
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