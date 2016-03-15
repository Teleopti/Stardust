taskkill.exe /IM iisexpress.exe /F /FI "memusage gt 2" /T 2>&1 | exit /B 0
taskkill.exe /IM iisexpresstray.exe /F /FI "memusage gt 2" /T 2>&1 | exit /B 0
