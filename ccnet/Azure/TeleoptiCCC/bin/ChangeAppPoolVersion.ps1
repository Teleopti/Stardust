param ([string] $applicationName = $(throw "applicationName is required"))
Write-Host $applicationName

[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration")

$iis = New-Object Microsoft.Web.Administration.ServerManager 

$attemptSiteCount = 0;
$attemptAppCount = 0;
$attemptPathCount = 0;
$MaxAttemptCount = 20;

# functions
function fnChangeAppPoolVersion {
param ([System.Object]$arg0)
	$appPoolName = $arg0.ApplicationPoolName;
	Write-Output $("Set .NET v4.0 as ManagedRuntimeVersion on: $appPoolName")
	$apppoolobject = $iis.ApplicationPools[$appPoolName]
	$apppoolobject.ManagedRuntimeVersion = "v4.0";
	$iis.CommitChanges();
}

# =======
# Main
# =======
# wait for web site(s) to be created
do {
	if ($iis.sites)
	{
	break
	}
	$attemptSiteCount++
	Start-Sleep -s 30
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
		$attemptAppCount++
		Start-Sleep -s 30
	}
	while ($attemptAppCount -le $MaxAttemptCount)

	do {
		foreach ($siteApplication in $site_name.applications)
         {
		   if ($siteApplication.Path.EndsWith("/" + $applicationName))
		   {
                $siteFound = $siteApplication.Path.EndsWith("/" + $applicationName);
                fnChangeAppPoolVersion($siteApplication)
				break
			}
		}
			$attemptPathCount++
			if (!$siteFound)
            {Start-Sleep -s 30}
	}
	while ((!$siteFound) -and ($attemptPathCount -le $MaxAttemptCount))
}
