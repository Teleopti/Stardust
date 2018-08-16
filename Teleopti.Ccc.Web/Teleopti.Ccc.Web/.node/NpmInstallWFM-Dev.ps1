# author: emil.sundin@teleopti.com

Invoke-Expression .\UseNodeEnv.ps1

Invoke-Expression .\NpmChecks.ps1

[String]$depsChanged = Invoke-Expression .\CheckPackageJsonIsModifiedRefactor.ps1

if($depsChanged.Equals("ALL") -or $depsChanged.Equals("WFM")) {
    Write-Host "Installing npm deps"
    Set-Location $PSScriptRoot\..\WFM
    Invoke-Expression "npm ci"
    Set-Location $PSScriptRoot
} else {
    Write-Host "No need to install npm deps"
}
