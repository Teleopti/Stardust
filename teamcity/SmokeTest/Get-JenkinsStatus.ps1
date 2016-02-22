param
(
  [String]
  [Parameter(Mandatory=$true)]
  $ServerName
 )

#$job_status = invoke-webrequest "http://tritonweb:8080/job/TritonWeb-AutoDeploy/lastBuild/api/json" | Select-String -pattern "`"result`":`"SUCCESS`""

$ServerURL = "$ServerName" + ':8080'

$Check_webstatus = invoke-webrequest "http://$ServerURL/job/$ServerName-AutoDeploy/lastBuild/api/json"

$Job_status = ConvertFrom-Json $Check_webstatus

$Job_status.result

if ($Job_status.result -eq "SUCCESS") {

Write-Host ""
Write-Host "Jenkins deploy successfully completed!" -ForegroundColor Green
Write-Host ""

}

else {

Write-Host ""
Write-Host "Jenkins deploy failure! Check http://$ServerURL/job/$ServerName-AutoDeploy for more information!" -ForegroundColor Red
Write-Host ""
Exit
}
