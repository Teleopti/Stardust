# author: emil.sundin@teleopti.com

Invoke-Expression .\UseNodeEnv.ps1

Invoke-Expression .\NpmChecks.ps1

Invoke-Expression .\NpmInstallWFM-Dev.ps1

Set-Location $PSScriptRoot\..\WFM
Invoke-Expression "npm run prod:build"
Set-Location $PSScriptRoot
