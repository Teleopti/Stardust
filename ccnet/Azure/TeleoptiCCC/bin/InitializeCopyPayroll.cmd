@ECHO OFF

SET DIRECTORY=%~dp0
SET FILE=%~n0
SET admin=ScheduleTaskAdmin
SET taskName=CopyPayrollDll

::generate password
net user administrator /random > %temp%\pwd.txt
set /P pwd=< %temp%\pwd.txt
set pwd=%pwd:~31,39%
echo Temporary user is %admin% with password %pwd% >> "%FILE%.log"
del %temp%\pwd.txt

::Add new local user to administrators group
net user %admin% %pwd% /del
net user %admin% %pwd% /add
net localgroup Administrators %admin% /add

::try start the service before we contiune
net start "task scheduler" >> "%FILE%.log"

::Delete if exist
schtasks /Delete /F /TN "%taskName%" >> "%FILE%.log"

::Schedule task every 20 minutes
schtasks /create /sc minute /mo 20 /tn "%taskName%" /f /ru "%admin%" /rp "%pwd%" /tr "%DIRECTORY%CopyPayrollDll.cmd" >> "%FILE%.log"

::Run for the first time
schtasks /RUN /TN "%taskName%" >> "%FILE%.log"