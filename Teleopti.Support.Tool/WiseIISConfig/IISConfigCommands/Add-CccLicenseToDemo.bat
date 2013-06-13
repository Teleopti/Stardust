@echo off
SET ROOTDIR=%~dp0
SQLCMD -S. -E -d"TeleoptiCCC7_Demo" -i"%ROOTDIR%..\..\..\Database\Tools\Restore\tsql\AddLic.sql" -v LicFile="%ROOTDIR%..\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"