$SignTool = "C:\Program Files (x86)\Windows Kits\10\bin\x64\signtool"
$Target = "$WorkingDirectory\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\$Configuration\Teleopti.Ccc.SmartClientPortal.Shell.exe"
&$SignTool sign /debug /f "$WorkingDirectory\teamcity\Azure\TeleoptiCCC\bin\teleopti.pfx" /p T3l30pt1 $Target
&$SignTool timestamp /t "http://timestamp.verisign.com/scripts/timstamp.dll" $Target