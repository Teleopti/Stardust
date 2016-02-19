param
(
  [String]
  [Parameter(Mandatory=$true)]
  $UrlToTest,
  [int32]
  [Parameter(Mandatory=$true)]
  $WaitingTime
  )

$UrlToCheck1 = $UrlToTest + '/Web'
$UrlToCheck2 = $UrlToTest + '/AuthenticationBridge'
$UrlToCheck3 = $UrlToTest + '/SDK/TeleoptiCCCSdkService.svc'
function Check-URL ($UrlToCheck)
{
    do {
start-sleep -seconds 2
Write-Host "Checking if $UrlToCheck is accessible..." -fore cyan
# Create the request.
$HTTP_Request = [System.Net.WebRequest]::Create($UrlToCheck)
# Pass default credentials (To be able to ping SDK)
$http_request.UseDefaultCredentials = $true
# Response from site
$HTTP_Response = $HTTP_Request.GetResponse()
# HTTP code as an integer.
$HTTP_Status = [int]$HTTP_Response.StatusCode

} until ($HTTP_Status -eq 200)
Write-Host "$UrlToCheck is up and running!" -fore green
Write-Host ''

$HTTP_Response.Close()
}

#Waiting for Autodeploy to complete
$Time=get-date
$DisplayTime = $WaitingTime/60
Write-Host "$Time" -fore cyan
Write-Host ''
Write-Host "Waiting $DisplayTime min for AutoDeploy of: " -Fore Cyan -nonewline; Write-Host "$URLtoTest" -fore Yellow
Write-Host ''

$WaitingTime =  #Timeout value, 750sec = 12,5min, 900sec = 15min
foreach($i in (1..$WaitingTime)) {
    $percentage = $i / $WaitingTime
    $remaining = New-TimeSpan -Seconds ($WaitingTime - $i)
    $message = '{0:p0} complete, remaining time {1}' -f $percentage, $remaining
    Write-Progress -Activity $message -PercentComplete ($percentage * 100)
    Start-Sleep 1
}

Check-URL $UrlToCheck1
Check-URL $UrlToCheck2
Check-URL $UrlToCheck3
