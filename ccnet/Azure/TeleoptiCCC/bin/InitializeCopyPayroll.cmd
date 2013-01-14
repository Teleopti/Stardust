SET DIRECTORY=%~dp0
:: Schedule task every 20 minutes
schtasks /create /sc minute /mo 20 /tn "CopyPayrollDll" /tr "%DIRECTORY%CopyPayrollDLL.cmd"
::Run for the first time
"%DIRECTORY%CopyPayrollDll.cmd"