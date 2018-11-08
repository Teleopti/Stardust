Set-Executionpolicy remotesigned -s currentuser

# workaround scoop issue
# https://github.com/lukesampson/scoop/issues/2745
$envPsModulePath = [environment]::getEnvironmentVariable("PSModulePath","User")
if($null -eq $envPsModulePath) {
	[environment]::setEnvironmentVariable("PSModulePath","$home\Documents\WindowsPowerShell\Modules","User")
}

# Install scoop package manager
iex (new-object net.webclient).downloadstring('https://get.scoop.sh')

$powershellRoot = $(Split-Path $PROFILE)
$profileFile = Get-Content $PROFILE
$modulePath = "$powershellRoot\TeleoptiShell.psm1"
$matches = $profileFile | Select-String 'Import-Module TeleoptiShell' -AllMatches
# Write-Output $matches.Matches.Length

$WFMRepoPath = (Resolve-Path "$PSScriptRoot\..\").Path
Set-Content -Path "$powershellRoot\WFMPath.txt" -Value $WFMRepoPath

If ($matches.Matches.Length -gt 0) {
    Write-Warning "Already installed this module."
} Else {
    Add-Content -Path $PROFILE -Value "`r`n`r`nImport-Module TeleoptiShell"
}
