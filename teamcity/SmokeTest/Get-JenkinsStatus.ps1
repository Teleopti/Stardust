﻿param
(
  [String]
  [Parameter(Mandatory=$true)]
  $ServerName
 )


if ($ENV:JOB_NAME -ne $null) 
{
	$JenkinsURL = "http://" + $ServerName + ":8080/job/$ENV:JOB_NAME/lastBuild/api/json"
}
if ($ENV:JenkinsJobName -ne $null) 
{
	$JenkinsURL = "http://" + $ServerName + ":8080/job/$ENV:JenkinsJobName/lastBuild/api/json"
}
else
{
	$ServerURL = "$ServerName" + ':8080'
	$JenkinsURL = "http://$ServerURL/job/$ServerName-AutoDeploy/lastBuild/api/json"
	
}

$Check_webstatus = invoke-webrequest $JenkinsURL -UseBasicParsing
$Job_status = ConvertFrom-Json $Check_webstatus

# Check if deploy is still running
do {($job_status.building)  

$Check_webstatus = invoke-webrequest $JenkinsURL -UseBasicParsing
$Job_status = ConvertFrom-Json $Check_webstatus

"Waiting for Jenkins deploy to complete..."
Start-Sleep -Seconds 15

} while ($Job_status.building -eq "True")

#Check if deploy was successful
$Check_webstatus = invoke-webrequest $JenkinsURL -UseBasicParsing
$Job_status = ConvertFrom-Json $Check_webstatus

if ($Job_status.result -eq "SUCCESS") { Write-Host "Jenkins deploy successfully completed!" }

else { 
$linkurl = "http://" + $ServerName + ":8080/job/$ENV:JOB_NAME"
Write-Host "Jenkins deploy failed! Check $linkurl for more information!"
$ErrorMessage = $Error[0].Exception.Message
Write-Host $( '##teamcity[message text=''{0}'']' -f $ErrorMessage ) 
exit 1
}
