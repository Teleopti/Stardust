Set-Executionpolicy RemoteSigned -s CurrentUser

# Install scoop package manager
iex (new-object net.webclient).downloadstring('https://get.scoop.sh')

scoop install "https://gist.githubusercontent.com/krokofant/a63c7f182ee2236c94bc468a2f8c720a/raw/82f62493d2841e1f70117629432bce9eb82cf6d6/teleoptishell.json"

$powershellRoot = $(Split-Path $PROFILE)
$shouldImportModule = True
if ([System.IO.File]::Exists($PROFILE)) {
    $profileFile = Get-Content $PROFILE
    $matches = $profileFile | Select-String 'Import-Module TeleoptiShell' -AllMatches
    $script:shouldImportModule = $matches.Matches.Length -eq 0
}

if ($shouldImportModule) {
    Add-Content -Path $PROFILE -Value "`r`n`r`nImport-Module TeleoptiShell"
}

# Save repo path
$WFMRepoPath = (Resolve-Path "$PSScriptRoot\..\").Path
Set-Content -Path "$powershellRoot\WFMPath.txt" -Value $WFMRepoPath
