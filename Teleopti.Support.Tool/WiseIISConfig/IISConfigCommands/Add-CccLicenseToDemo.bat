@echo off
SET ROOTDIR=%~dp0
SQLCMD -S. -E -d"TeleoptiWFM_Demo" -i"%ROOTDIR%..\..\..\Database\Tools\Restore\tsql\AddLic.sql" -v LicFile="%ROOTDIR%..\..\..\LicenseFiles\Teleopti_RD.xml"