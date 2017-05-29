param
(
  [String]
  [Parameter(Mandatory=$true)]
  $UrlToTest,
  [int32]
  [Parameter(Mandatory=$false)]
  $WaitingTime=1
  )



function Check-URL ($UrlToCheck)
{
    
    $BailoutTime = "20"
    $TimeStart = Get-Date
    $TimeEnd = $timeStart.addminutes($BailoutTime)
    
    while ($true) {
        
		$TimeNow = Get-Date
        
        $username = 'toptinet\tfsintegration'
		$password = 'm8kemew0rk'
		Write-Output "Checking if $UrlToCheck is accessible..."
        # Ignore SSL cert 
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

        # Create the request.
        $HTTP_Request = [System.Net.WebRequest]::Create($UrlToCheck)

        # Pass default credentials (To be able to ping SDK)
        $HTTP_Request.Credentials = New-Object System.Net.NetworkCredential -ArgumentList $username, $password

        try  {
            # Response from site
            $HTTP_Response = $HTTP_Request.GetResponse()
        } catch [System.Net.WebException] {
            $HTTP_Response = $_.Exception.Response
            Write-Output "[$UrlToCheck]`t $($_.Exception.Message)"
        }
        
        # HTTP code as an integer.
        $HTTP_Status = [int]$HTTP_Response.StatusCode

        Write-Output "[$UrlToCheck]`tReturned status $HTTP_Status"

        if ($TimeNow -ge $TimeEnd) {Write-Output "Running for $BailoutTime minutes, quitting";exit 1}

        if ($HTTP_Status -eq 200)
        {
            break;
        }

        Start-Sleep -Seconds 2
    } 
    
   
    Write-Output "$UrlToCheck is up and running!"
    $HTTP_Response.Close()
}

<#
#Waiting for Autodeploy to complete
$Time=Get-Date
$DisplayTime = $WaitingTime/60
Write-Host "$Time" -ForegroundColor cyan
Write-Host ''
Write-Host "Waiting $DisplayTime min for AutoDeploy of: " -ForegroundColor Cyan -NoNewline; 
Write-Host "$URLtoTest" -ForegroundColor Yellow
Write-Host ''

$WaitingTime =  #Timeout value, 750sec = 12,5min, 900sec = 15min
foreach($i in (1..$WaitingTime)) {
    $percentage = $i / $WaitingTime
    $remaining = New-TimeSpan -Seconds ($WaitingTime - $i)
    $message = '{0:p0} complete, remaining time {1}' -f $percentage, $remaining
    Write-Progress -Activity $message -PercentComplete ($percentage * 100)
    Start-Sleep -Seconds 1
}
#>

workflow parallelCheckUrl {
param ($UrlToTest)

$UrlToCheck1 = $UrlToTest + '/Web'
$UrlToCheck2 = $UrlToTest + '/AuthenticationBridge'
$UrlToCheck3 = $UrlToTest + '/SDK/TeleoptiCCCSdkService.svc'

  parallel {
    Check-URL $UrlToCheck1
    Check-URL $UrlToCheck2
    Check-URL $UrlToCheck3
  }
}

parallelCheckUrl -UrlToTest $UrlToTest
