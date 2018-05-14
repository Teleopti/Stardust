# author: emil.sundin@teleopti.com

Invoke-Expression .\NpmChecks.ps1

Invoke-Expression .\NpmInstallWFM-Dev.ps1

Set-Location $PSScriptRoot\..\WFM
Invoke-Expression "npx grunt dist --no-color"
Invoke-Expression "npx grunt buildForDesktop --no-color"
Set-Location $PSScriptRoot
