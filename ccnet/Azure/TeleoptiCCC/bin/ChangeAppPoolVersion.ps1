param ([string] $applicationName = $(throw "applicationName is required"))
Write-Host $applicationName

[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration")

$iis = New-Object Microsoft.Web.Administration.ServerManager 

$attemptSiteCount = 0;
$attemptAppCount = 0;
$MaxAttemptCount = 20;
			
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

    foreach ($siteApplication in $site_name.applications)
         {
               if ($siteApplication.Path.EndsWith($applicationName))
			   {
					$appPoolName = $siteApplication.ApplicationPoolName;
					Write-Output $("Set .NET v4.0 as ManagedRuntimeVersion on: $appPoolName")
					$apppoolobject = $iis.ApplicationPools[$appPoolName]
					$apppoolobject.ManagedRuntimeVersion = "v4.0";
					$iis.CommitChanges();
                    break
				}
       }
}
