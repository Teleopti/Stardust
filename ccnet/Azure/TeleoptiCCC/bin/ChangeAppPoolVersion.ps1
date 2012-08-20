param ([string] $applicationName = $(throw "applicationName is required"))
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration")

#----------
# functions
#----------
function fnLoopSiteApplication {
    foreach ($site_name in $iis.sites)
    { 
        foreach ($siteApplication in $site_name.applications)
        {
            write-host "$siteApplication `r"
            if ($siteApplication.Path.EndsWith("/" + $applicationName))
            {
                fnChangeAppPoolVersion($siteApplication);
                Return $True
            }
        }
    }
    Return $False
}

function fnChangeAppPoolVersion {
param ([System.Object]$arg0)
	$appPoolName = $arg0.ApplicationPoolName;
	$apppoolobject = $iis.ApplicationPools[$appPoolName]
	$apppoolobject.ManagedRuntimeVersion = "v4.0";
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
        If (fnLoopSiteApplication) {
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