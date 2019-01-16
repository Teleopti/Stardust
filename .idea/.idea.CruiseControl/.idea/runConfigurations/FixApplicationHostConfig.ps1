[xml]$config = Get-Content "$PSScriptRoot\..\..\..\config\vs-applicationhost.config"

$sites = @{
    'Teleopti.Ccc.Sdk.Host'       = @{
        '/' = 'Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.Host'
    };
    'Teleopti.Ccc.Web-Site'       = @{
        '/TeleoptiWFM/Web'                     = 'Teleopti.Ccc.Web\Teleopti.Ccc.Web'
        '/TeleoptiWFM'                         = 'Teleopti.Ccc.Startpage'
        '/TeleoptiWFM/AuthenticationBridge'    = 'Teleopti.Ccc.Web.AuthenticationBridge'
        '/TeleoptiWFM/WindowsIdentityProvider' = 'Teleopti.Ccc.Web.WindowsIdentityProvider'
    }
    'Teleopti.Analytics.Portal'   = @{
        '/' = 'Teleopti.Analytics\WebPortal\Teleopti.Analytics.Portal\Teleopti.Analytics.Portal'
    }
    'Teleopti.Wfm.Administration' = @{
        '/' = 'Teleopti.Wfm.Administration'
    }
    'Teleopti.Wfm.Api'            = @{
        '/' = 'Teleopti.Wfm.Api'
    }

}

# Update absolute paths in the checked-in VS copy of the config and use that
foreach ($siteName in $sites.Keys) { 
    foreach ($vdKey in $sites[$siteName].Keys) {
        $relPath = $sites[$siteName][$vdKey]
        $absolutePath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\..\..\..\$relPath")
        $app = $config.SelectSingleNode("//site[@name='$siteName'] //application[@path='$vdKey']")
        $app.virtualDirectory.physicalPath = $absolutePath
        Write-Output "$siteName"
        Write-Output "`t $vdKey -> $absolutePath"
    }
    $config.Save("$PSScriptRoot\..\..\..\config\applicationhost.config")
}
