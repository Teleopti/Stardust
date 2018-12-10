param (
	[string] $applicationName = $(throw "applicationName is required"),
	[string] $managedRuntimeVersion = $(throw "ManagedRuntimeVersion is required")
	)
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration")

#----------
# functions
#----------
function fnLoopSiteApplication ([string]$applicationName, [string]$netVersion)
{
    foreach ($site_name in $iis.sites)
    { 
        foreach ($siteApplication in $site_name.applications)
        {
            write-host "$siteApplication `r"
            if ($siteApplication.Path.EndsWith("/" + $applicationName))
            {
                fnChangeAppPoolVersion $siteApplication $netVersion
                Return $True
            }
        }
    }
    Return $False
}

function fnChangeAppPoolVersion ([System.Object]$site, [string]$ManagedRuntimeVersion)
{
	$site.SetAttributeValue("preloadEnabled", $true)
	$appPoolName = $site.ApplicationPoolName;
	$apppoolobject = $iis.ApplicationPools[$appPoolName]
	$apppoolobject.ManagedRuntimeVersion = $ManagedRuntimeVersion;
	$apppoolobject.startMode = "AlwaysRunning";
	$apppoolobject.autoStart = $True;
	$apppoolobject.ProcessModel.IdleTimeout = New-TimeSpan;
	$iis.CommitChanges();
}

#----------
# Main
#----------
$attemptSuccess = 0;
$MaxAttemptCount = 20;
$sleep = 30
do {

    try {
        Write-host "$(get-date -format 'yyyy-MM-dd HH:mm:ss.ms')`r"
        $iis = New-Object Microsoft.Web.Administration.ServerManager 
        If (fnLoopSiteApplication $applicationName $managedRuntimeVersion) {
            Write-host "Success!"
            break
        }
        else
        {
        Write-host "Could not find the correct application. Trying again ... $attemptSuccess `r"
        $attemptSuccess++
        Start-Sleep -s $sleep
        }
    }

    Catch {
     $err=$Error[0]
     Write-host "$err"
     break
     }
 
   finally {
    $iis.Dispose();
    }
}
while ($attemptSuccess -le $MaxAttemptCount)
Write-host "$(get-date -format 'yyyy-MM-dd HH:mm:ss.ms')"