# author: emil.sundin@teleopti.com

# use `npm ci` when npm 5.7.0 is available for lts
# until then use npm install
# https://docs.npmjs.com/cli/ci
# https://github.com/nodejs/node/pull/19840

[String]$depsChanged = Invoke-Expression .\CheckPackageJsonIsModifiedRefactor.ps1

if($depsChanged.Equals("ALL") -or $depsChanged.Equals("WFM")) {
    Write-Output "Installing npm deps"
    Set-Location $PSScriptRoot\..\WFM
    Invoke-Expression "npm install"
    Set-Location $PSScriptRoot
} else {
    Write-Output "No need to install npm deps"
}
