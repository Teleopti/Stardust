$parameters = new-object System.Collections.Specialized.NameValueCollection
$parameters.Add("UserName", "demo")
$parameters.Add("Password", "demo")
$parameters.Add("DataSource", "Teleopti WFM")
$parameters.Add("UseWindowsIdentity", "false")
$parameters.Add("ResultGuid", "DAC14836-EA85-4E03-B4A3-A6F900DF7C6B")

$webClient = new-object System.Net.WebClient
$webClient.Headers.Add("user-agent", "PowerShell Script")

$sdkUrl = $Env:UrlToTest + "/SDK/GetPayrollResultById.aspx"
$result = $webClient.UploadValues($sdkUrl, "POST", $parameters)
$enc = [system.Text.Encoding]::Unicode
$output = $enc.GetString($result)
    
 if ($output -eq "") {
     "Success to get payroll result from SDK"
     break;
} else {
    "Fail to get payroll result from SDK"
    Write-Error $_
    ##teamcity[buildStatus status='FAILURE']
    [System.Environment]::Exit(1)
}