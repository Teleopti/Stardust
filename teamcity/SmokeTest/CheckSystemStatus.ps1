$Url = "https://$env:CloudServiceName.teleopticloud.com/AuthenticationBridge/systemstatus.aspx"

function CheckSystemStatus
{
	param 
	(
		$Url
	)
	$minNumberOfSuccess = 5
	
	Write-Host "Checking Systemstatus on: '$Url'"
	
    $success = 0
	do
    {
        $Response = wget $Url -UseBasicParsing -ErrorAction SilentlyContinue
		$status = $Response.StatusCode
		if($status -eq 200) #ska vara 200-299
		{
			$success++
		}
		else 
		{
			$success = 0
		}
		Write-Output "Status Code: $status"
        
        Start-Sleep 1
    } until($success -ge $minNumberOfSuccess)
}

add-type @"
		using System.Net;
		using System.Security.Cryptography.X509Certificates;
		public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
			}
		}
"@

	[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
	[Net.ServicePointManager]::SecurityProtocol = 'Tls12'

CheckSystemStatus  $Url