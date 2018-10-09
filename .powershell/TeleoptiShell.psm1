# Perform some setup checks
$powershellRoot = $(Split-Path $PROFILE)
$WFMRepoPath = (Get-Content "$powershellRoot\WFMPath.txt")
# $hasRepoPath = [bool](Get-Variable 'WFMRepoPath' -Scope 'Global' -EA 'Ig')
$hasError = $false
# if(!$hasRepoPath) {
#     Write-Warning "You should define `$WFMRepoPath in your profile"
#     $hasError = $true
# }

if(!$hasError) {
    # Define a few relative project paths
    $WFMRepoWebPath = $WFMRepoPath + '\Teleopti.Ccc.Web\Teleopti.Ccc.Web'
    $WFMRepoWebWFMPath = $WFMRepoPath + '\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM'
}

# Utils
function installIfMissing {
    param
      (
          [string] $ModuleName
      )
      if (!(Get-Module -ListAvailable -Name $ModuleName)) {
        Write-Output "Installing missing module $ModuleName"
        Install-Module -Name $ModuleName -Scope CurrentUser
      }
}
# function waitForInput { [void]($Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')) }

# Install and load Visual Studio helpers
installIfMissing posh-vsdev
Import-Module posh-vsdev

function cdwfm { Set-Location $WFMRepoWebWFMPath }
function wfmcleanbuild {
    wfmbuild
    wfmrestoretolocal
}
function wfmlatestbuild {
    cdwfm
    git checkout master
    git pull
    wfmbuild
    wfmrestoretolocal
}


function wfmbuild {
    Set-Location $WFMRepoPath
    Use-VisualStudioEnvironment
    MSBuild.exe CruiseControl.sln /t:build /m
    Reset-VisualStudioEnvironment
}
function wfmbuildweb {
    Set-Location $WFMRepoWebPath
    Use-VisualStudioEnvironment
    MSBuild.exe Teleopti.Ccc.Web.csproj /t:rebuild
    Reset-VisualStudioEnvironment
}
function wfmbuildusertexts {
    Set-Location ($WFMRepoPath + "\Teleopti.Ccc.UserTexts")
    Use-VisualStudioEnvironment
    resgen .\Resources.resx /str:c#,Teleopti.Ccc.UserTexts,Resources,Resources.Designer.cs /publicclass
    Remove-Item Resources.resources -ErrorAction Ignore
    MSBuild.exe Teleopti.Ccc.UserTexts.csproj /t:rebuild
    Reset-VisualStudioEnvironment
}
function wfmrestoretolocal {
    Set-Location ($WFMRepoPath + "\.debug-Setup")
    $env:IFFLOW = "y"
    & '.\Restore to Local.bat'
    $env:IFFLOW = ""
    [Console]::ResetColor()
}
function fcheck { # search for fdescribe and fit
    $Matches = Get-ChildItem -Recurse -Filter *.spec.ts $WFMRepoWebPath |
    Select-String -Pattern "fdescribe\(|fit\(" -List
    $Paths = $Matches.Path
    $Count = $Matches.Matches.Count

    if($Count -gt 0) {
        Write-Host "Found $Count matches for fdescribe/fit in the following files:" -ForegroundColor Red
        Write-Host $Paths
    }
}