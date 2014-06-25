@echo off
SET ROOTDIR=%~dp0
SET /P InstanceName=Please provide your SQL Server instance [ServerName\InstanceName]:
ECHO Note: You must be able to access SQL Instance: [%InstanceName%] with Windows Auth!
Echo ... or edit this batchfile to enter a valid SQL login and Password
ECHO .

::Un-comment and switch to SQL Login if needed
SET Login=-E
::SET Login=-Usa -PMyPwd

SQLCMD -S%InstanceName% %Login% -Q"SET NOCOUNT ON;SELECT IS_SRVROLEMEMBER('sysadmin');" > "%TEMP%\out.txt"
IF %ERRORLEVEL% NEQ 0 GOTO :error

FindStr /C:"1" "%TEMP%\out.txt"
IF %ERRORLEVEL% EQU 0 (
ECHO Succesfully connected as SysAdmin to: %InstanceName%
SQLCMD -S%InstanceName% %Login% -v BakDir = "%ROOTDIR%" -i"RestoreDemo.sql"
SQLCMD -S%InstanceName% %Login% -v BakDir = "%ROOTDIR%" -i"RestoreUsers.sql" -v CurrentUser=""
GOTO :done
) ELSE (
GOTO :error
)
:error
ECHO.
ECHO ------------------
ECHO Can't connect to: %InstanceName% as SysAdmin. Abort!
ECHO ------------------
GOTO :end

:Done
ECHO.
ECHO done!
GOTO :end

:end
PAUSE
