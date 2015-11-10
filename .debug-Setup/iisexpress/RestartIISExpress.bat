SET THISDIR=%~dp0\
SET IISExpressStartFolder="c:\Program Files\IIS Express\"
timeout 3
cd %IISExpressStartFolder%
taskkill /F /IM iisexpress.exe
timeout 3
start iisexpress /config:%THISDIR%SikuliApplicationhost.config /AppPool:Clr4IntegratedAppPool