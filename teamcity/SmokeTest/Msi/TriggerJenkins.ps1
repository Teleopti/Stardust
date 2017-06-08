$result = Invoke-WebRequest $Env:JenkinsTriggerUrl -UseBasicParsing
if($result.StatusCode -ne 201) {
    Write-Error "fail to trigger jenkins auto deploy"
}