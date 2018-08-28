$powershellRoot = $(Split-Path $PROFILE)
$profileFile = Get-Content $PROFILE
$modulePath = "$powershellRoot\TeleoptiShell.psm1"
$matches = $profileFile | Select-String 'Import-Module .*TeleoptiShell.psm1' -AllMatches
# Write-Output $matches.Matches.Length

$WFMRepoPath = (Resolve-Path "$PSScriptRoot\..\").Path
Set-Content -Path "$powershellRoot\WFMPath.txt" -Value $WFMRepoPath

If ($matches.Matches.Length -gt 0) {
    Write-Warning "Already installed this module. Edit $PROFILE manually to fix."
}
Else {
    Copy-Item .\TeleoptiShell.psm1 "$powershellRoot"
    Add-Content -Path $PROFILE -Value "`r`n`r`nImport-Module $modulePath"
}
