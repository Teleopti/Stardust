# author: emil.sundin@teleopti.com

Invoke-Expression .\NpmChecks.ps1

[String]$depsChanged = Invoke-Expression .\CheckPackageJsonIsModifiedRefactor.ps1

if($depsChanged.Equals("ALL") -or $depsChanged.Equals("WFM")) {
    Write-Output "Installing npm deps"
    Set-Location $PSScriptRoot\..\WFM
    Invoke-Expression "npm install"
    Set-Location $PSScriptRoot
} else {
    Write-Output "No need to install npm deps"
}
