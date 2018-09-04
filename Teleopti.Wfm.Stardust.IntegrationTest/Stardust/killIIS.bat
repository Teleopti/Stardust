@ECHO OFF
 
taskkill.exe /IM iisexpress.exe /F /FI "memusage gt 2" /t
taskkill.exe /IM Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost.exe /F /FI "memusage gt 2" /t
taskkill.exe /IM iisexpresstray.exe /F /FI "memusage gt 2" /T 2>&1 | exit /B 0