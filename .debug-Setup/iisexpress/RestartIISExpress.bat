timeout 3
cd "c:\Program Files\IIS Express\"
taskkill /F /IM iisexpress.exe
timeout 3
start iisexpress /config:c:\temp\IISExpress\SikuliApplicationhost.config /AppPool:Clr4IntegratedAppPool