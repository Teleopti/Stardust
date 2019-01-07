#Defining install credentials
$username = "tfsintegration"
$domain = "toptinet"
$password = "m8kemew0rk"
$secstr = New-Object -TypeName System.Security.SecureString
$password.ToCharArray() | ForEach-Object {$secstr.AppendChar($_)}
$AdminCredentials = new-object -typename System.Management.Automation.PSCredential -argumentlist $domain\$username, $secstr

$result = Invoke-WebRequest $Env:JenkinsTriggerUrl -Credential $AdminCredentials -UseBasicParsing
if($result.StatusCode -ne 201) {
    Write-Error "fail to trigger jenkins auto deploy"
}
