
Param (
		[Parameter(Mandatory=$True)]$listUrl
	)

function CheckSystemStatus
{
	param 
	(
		$Url
	)
	$minNumberOfSuccess = 5
	
    do
    {
        $listRead = $false
        try
        {
            $steps = Invoke-RestMethod -Uri $Url
            $listRead = $true 
        }
        catch
        {
			Write-Output "Waiting for the admin site to start..."
            Start-sleep -s 2
        }
    }until($listRead -eq $true)

    $success = 0
	do
    {
        $success++
        Foreach($step in $steps)
        {
            try
            {
                Invoke-RestMethod -Uri null
            }
            catch
            {
                Write-Host "ErrorDescription:" $error[0]
                $success = 0
                Start-sleep -s 2
            }
        }
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

CheckSystemStatus  $listUrl