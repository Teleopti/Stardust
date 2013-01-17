@ECHO OFF

SET DIRECTORY=%~dp0
SET FILE=%~n0
SET admin=ScheduleTaskAdmin
SET pwd=Admin!@#123

::Add new local user to administrators group
net user %admin% %pwd% /add
net localgroup Administrators %admin% /add

::try start the service before we contiune
net start "task scheduler" >> "%FILE%.log"

::Delete if exist
schtasks /Delete /F /TN "CopyPayrollDll" >> "%FILE%.log"

::Schedule task every 20 minutes
schtasks /create /sc minute /mo 20 /tn "CopyPayrollDll" /f /ru "%admin%" /rp "%pwd%" /tr "%DIRECTORY%CopyPayrollDLL.cmd" >> "%FILE%.log"

::Run for the first time
schtasks /RUN /TN "CopyPayrollDll" >> "%FILE%.log"