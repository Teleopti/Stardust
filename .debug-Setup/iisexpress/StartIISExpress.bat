@echo off
SET THISDIR=%~dp0\
SET IISExpressStartFolder="c:\Program Files\IIS Express"
if not exist SikuliApplicationhost.config (
	ren SikuliApplicationhost.config.txt SikuliApplicationhost.config)
tasklist /fi "imagename eq iisexpress.exe" | find ":" > nul
if not errorlevel 1 (
	cd %IISExpressStartFolder%
	start iisexpress /config:%THISDIR%SikuliApplicationhost.config /AppPool:Clr4IntegratedAppPool)