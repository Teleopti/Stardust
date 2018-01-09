$JenkinsUrl = New-Object -TypeName 'System.Uri' -ArgumentList $Env:JenkinsTriggerUrl

$username = "tfsintegration"
$password = "m8kemew0rk"
$URL = $JenkinsUrl.Host
$Port = $JenkinsUrl.Port

$h = @{}
$h.Add('Authorization', 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$(${username}):$(${password})")))

$Params = @{uri = "http://${URL}:${Port}/crumbIssuer/api/json";
            Method = 'Get';
            Headers = $h;}
$API_Crumb = Invoke-RestMethod @Params

$h.Add('Jenkins-Crumb', $API_Crumb.crumb)
$Params['uri'] = "$Env:JenkinsTriggerUrl"
$Params['Method'] = 'Post'
$Params['Headers'] = $h

$result = Invoke-WebRequest @Params
if($result.StatusCode -ne 201) {
    Write-Error "fail to trigger jenkins auto deploy"
}
