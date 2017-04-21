Param (

    [Parameter(Mandatory=$True)][string]$Server = $env:servername

    )

.nuget\nuget.exe install AutoItX -o packages -version "3.3.12"
import-module "$PSScriptroot\packages\AutoItX.3.3.12.0\lib\AutoItX3.psd1"

try
{
Write-Host "Downloading ClickOnce client from http://$Server"

$url = "http://$Server/TeleoptiWFM/Client/Teleopti.Ccc.SmartClientPortal.Shell.application"
$output =  "$PSScriptroot\Teleopti.Ccc.SmartClientPortal.Shell.application"

$wc = New-Object System.Net.WebClient
$wc.DownloadFile($url, $output)

#Download Client
$p = Start-Process "$PSScriptroot\Teleopti.Ccc.SmartClientPortal.Shell.application" -PassThru
$Title = "Application Install - Security Warning"
Wait-AU3Win -Title $Title | out-null
$winHandle = Get-AU3WinHandle -Title $Title
Show-AU3WinActivate -WinHandle $winHandle | out-null
$controlHandle = Get-AU3ControlHandle -WinHandle $winhandle -Control "&Install"
Invoke-AU3ControlClick -WinHandle $winHandle -ControlHandle $controlHandle | out-null

#Verify Client
$Title = "WFM Login"
Wait-AU3Win -Title $Title | out-null
Write-Host "Starting ClickOnce client..."
Start-sleep -s 5
$winHandle = Get-AU3WinHandle -Title $Title
Show-AU3WinActivate -WinHandle $winHandle | out-null

$Assert = Assert-AU3WinExists -Title $Title
Start-sleep -s 5

    if ($Assert -eq 1) 
    {
        Write-Host "Teleopti WFM ClickOnce client successfully installed!"
    }
    else
    {
        Write-Host "Could not install ClickOnce client."
        exit 1
    }
}
catch
{

}
finally
{

Write-Host "Cleaning up ClickOnce client installation..."
#Kill clients
$kill = tasklist /M Teleopti.CCC.smartClientportal* | FIND "INFO: No tasks"
    if ($kill -ne $null)
    {
        $killClient = taskkill /f /im Teleopti.Ccc.SmartClientPortal.Shell.exe /fi "memusage gt 2"
    }

Start-sleep -s 5
$kill = tasklist /M Teleopti.CCC.AgentPortal* | FIND "INFO: No tasks" 

    if ($kill -ne $null)
    {
        $killClient = taskkill /f /im Teleopti.Ccc.AgentPortal.exe /fi "memusage gt 2"
    }
Start-sleep -s 5

#UnInstall Client
rundll32.exe dfshim.dll,ShArpMaintain Teleopti.Ccc.SmartClientPortal.Shell.application, Culture=neutral, PublicKeyToken=1680e69d5f0df38a, processorArchitecture=msil
rundll32.exe dfshim.dll,ShArpMaintain Teleopti.Ccc.AgentPortal.application, Culture=neutral, PublicKeyToken=1680e69d5f0df38a, processorArchitecture=msil
Start-sleep -s 5

#Remove files
Remove-Item "$env:temp\..\apps\2.0" -recurse -force 
Write-Host "Done cleaning up installation!"

}