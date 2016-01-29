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

$username = "demo"
$password = "teleoptidemo"
$domain = "Toptinet"
$webClient = new-object System.Net.WebClient
$webclient.Credentials = new-object System.Net.NetworkCredential($username, $password, $domain)
$webClient.Headers.Add("user-agent", "PowerShell Script")

$pingStartTime = get-date
$sdkUrl = $Env:UrlToTest + "/SDK/TeleoptiCCCSdkService.svc"
$count = 0
$isSuccess = $false
while ($count -lt 30) {
    $output = ""
    $startTime = get-date
    $output = $webClient.DownloadString($sdkUrl)
    $endTime = get-date

    if ($output -like "*/SDK/TeleoptiCccSdkService.svc?singleWsdl*") {
        "Success to ping " + $sdkUrl + "`t`t" + $startTime.DateTime + "`t`t" + ($endTime - $startTime).TotalSeconds + " seconds"	
        $isSuccess = $true;	  
	    break;
    } else {
        "Fail (" + $count + ") to ping " + $sdkUrl + "`t`t" + $startTime.DateTime + "`t`t" + ($endTime - $startTime).TotalSeconds + " seconds"
    }
    sleep(10)
    $count++
}

if(!$isSuccess){
    $sdkUrl + " isn't available for 5 minutes."
}
