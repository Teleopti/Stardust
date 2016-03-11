@ECHO OFF

taskkill.exe /IM chromedriver.exe /F /FI "memusage gt 2" /t
taskkill.exe /IM iisexpress.exe /F /FI "memusage gt 2" /t
taskkill.exe /IM chrome.exe /F /FI "memusage gt 2" /t
taskkill.exe /IM iisexpresstray.exe /F /FI "memusage gt 2" /T 2>&1 | exit /B 0
