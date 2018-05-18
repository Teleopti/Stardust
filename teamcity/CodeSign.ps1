$SignTool = "C:\Program Files (x86)\Windows Kits\10\bin\x64\signtool"
$TargetFolder = "$env:WorkingDirectory\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\$env:Configuration\"

$allItems = Get-ChildItem -Recurse -Path $TargetFolder -include *.dll,*.exe
ForEach ($item in $allItems)
{
    &$SignTool verify /q /pa $item
    If($lastexitcode -EQ 1)
    {
        &$SignTool sign /debug /f "$env:WorkingDirectory\teamcity\Azure\TeleoptiCCC\bin\teleopti.pfx" /p T3l30pt1 $item
        &$SignTool timestamp /t "http://timestamp.verisign.com/scripts/timstamp.dll" $item
    }
}
