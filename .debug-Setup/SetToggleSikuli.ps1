param (
    
    [ValidateSet("ALL", "RC", "CUSTOMER")]
    [String]$SetToggleMode = "CUSTOMER",
    [String]$WebConfig = "c:\temp\AdminClient\Teleopti.Ccc.Web\Teleopti.Ccc.Web\web.config"
)

$SetToggleMode = $SetToggleMode.ToUpper()

if (Test-Path $webconfig) 
{
    $doc = (Get-Content $webConfig) -as [Xml]
}
else
{
    Write-Host "Could not find Webconfig in path: $webconfig"
    Exit 1
}

#Check if ToggleMode exists
$obj = $doc.configuration.appSettings.add | where {$_.Key -eq 'ToggleMode'}
    
if ($obj -ne $null) {Write-Host "ToggleMode is:" $obj.value}

       
if ($obj.value)
{
    Write-Host "Setting new ToggleMode to:" $SetToggleMode
    $obj.value = "$SetToggleMode"
    $doc.Save($webConfig)
}
    
else
{
    if (!($obj -ne $null))
    {
		Write-Host "ToggleMode key is not present in: $webconfig"
		Write-Host "Will add ToggleMode: $SetToggleMode to: $webconfig"
		$newAppSetting = $doc.CreateElement("add")
		$doc.configuration.appSettings.AppendChild($newAppSetting)
		$newAppSetting.SetAttribute("key","ToggleMode");
		$newAppSetting.SetAttribute("value","$SetToggleMode");
            
		$doc.Save($webConfig) | out-null
    }

}