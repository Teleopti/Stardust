param ([string] $applicationName = $(throw "applicationName is required"))
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration")

#----------
# functions
#----------
function fnChangeAppPoolVersion {
param ([System.Object]$arg0)
	$appPoolName = $arg0.ApplicationPoolName;
	Write-host $("Set .NET v4.0 as ManagedRuntimeVersion on: $appPoolName ")
	$apppoolobject = $iis.ApplicationPools[$appPoolName]
	$apppoolobject.ManagedRuntimeVersion = "v4.0";
	$iis.CommitChanges();
}

function fnLoopSiteApplication {
    # wait for web site(s) to be created
    do {
    	if ($iis.sites)
    	{
    	break
    	}
        Write-host $("No sites found. Sleeping for $sleep secs");
    	$attemptSiteCount++
    	Start-Sleep -s $sleep
    }
    while ($attemptSiteCount -le $MaxAttemptCount)

    foreach ($site_name in $iis.sites)
    { 
    	# wait for application(s) to be created
    	do {
    		if ($site_name.applications)
    		{
    		  break
    		}
            Write-host $("No applications found. Sleeping for $sleep secs ");
    		$attemptAppCount++
    		Start-Sleep -s $sleep
    	}
    	while ($attemptAppCount -le $MaxAttemptCount)

    	do {
    		foreach ($siteApplication in $site_name.applications)
             {
    		   if ($siteApplication.Path.EndsWith("/" + $applicationName))
    		   {
                    fnChangeAppPoolVersion($siteApplication);
                    Return $True
    			}
              }
            $attemptPathCount++
            Write-host $("siteFound is empty. Sleeping for $sleep secs");
            Start-Sleep -s $sleep
    	}
    	while ($attemptPathCount -le $MaxAttemptCount)
    }
    Return $False
}

#----------
# Main
#----------
$attemptSiteCount = 0;
$attemptAppCount = 0;
$attemptPathCount = 0;
$attemptSuccess = 0;
$MaxAttemptCount = 20;
$sleep = 30

do {

    try {
        $iis = New-Object Microsoft.Web.Administration.ServerManager 
        If (fnLoopSiteApplication) {
            write-host "Success!"
            break
        }else{
        write-host "Could not find the correct application. Trying again ... $attemptSuccess"
        $attemptSuccess++
        }
    }

    Catch {
     "Caught in a catch"
     $err=$Error[0]
     $err
     $err |Format-List *
     }
 
   finally {
    $iis.Dispose()
    }
}
while ($attemptSuccess -le $MaxAttemptCount)
