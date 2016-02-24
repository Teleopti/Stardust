param
(
  [String]
  [Parameter(Mandatory=$true)]
  $ServerName
 )

$ServerURL = "$ServerName" + ':8080'

$Check_webstatus = invoke-webrequest "http://$ServerURL/job/$ServerName-AutoDeploy/lastBuild/api/json"
$Job_status = ConvertFrom-Json $Check_webstatus

# Check if deploy is still running
do {($job_status.building)  

$Check_webstatus = invoke-webrequest "http://$ServerURL/job/$ServerName-AutoDeploy/lastBuild/api/json"
$Job_status = ConvertFrom-Json $Check_webstatus

"Waiting for Jenkins deploy to complete..."
Start-Sleep -Seconds 10

} while ($Job_status.building -eq "True")

#Check if deploy was success
$Check_webstatus = invoke-webrequest "http://$ServerURL/job/$ServerName-AutoDeploy/lastBuild/api/json"
$Job_status = ConvertFrom-Json $Check_webstatus

if ($Job_status.result -eq "SUCCESS") { Write-Host "Jenkins deploy successfully completed!" }

else { 
Write-Host "Jenkins deploy failed! Check http://$ServerURL/job/$ServerName-AutoDeploy for more information!"
Throw $_
$ErrorMessage = $_
Write-Host "##teamcity[buildProblem description='$ErrorMessage']"
Write-Error -Message $ErrorMessage
exit 1
}
