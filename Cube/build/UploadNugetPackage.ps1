$NugetExe = "$PSScriptRoot\..\..\.nuget\NuGet.exe"
$PAT = "c4a7hfpkb7ibp5zd67impuueu3hmrg3ta6yqrgrqcyvrmnxgep4q"
$Feed = "https://teleopti.pkgs.visualstudio.com/_packaging/TeleoptiNugets/nuget/v3/index.json"
$PackageName = 'Teleopti.Cube.Xmla'
Write-Output "Adding VSTS TeleoptiNuget as a source..."
& $NugetExe sources remove -Name TeleoptiNugets
& $NugetExe sources add -Name TeleoptiNugets -Source $Feed -UserName 'VssSessionToken' -Password $PAT

$Version = & $NugetExe list $PackageName -s $Feed

$Version = $Version | Select-String -Pattern 'Teleopti.Cube'

if($Version -match "(?<=Teleopti.Cube.Xmla \d+\.\d+\.)(?<bv>\d+)")
{

    Write-Host "Old NuGet pkg found on VSTS: $Version" 
    $NewNugetPkg = $Version -replace "(?<=Teleopti.Cube.Xmla \d+\.\d+\.)(\d+)", ("{0:000}" -f (([int]::Parse($matches.bv)+1)))

    $NewNugetPkg = $NewNugetPkg.split(" ")
    $NugetPkgName = $NewNugetPkg[0]
    $NugetPkgVersion = $NewNugetPkg[1]
    
    $NugetNuspec = $PSScriptRoot + '\' + $NugetPkgName + '.Nuspec'
            
    Write-Host "Will create a new NuGet pkg: '$NewVersion'"
    & $NugetExe pack $NugetNuspec -Version "$NugetPkgVersion"
    
    $NewPkgToPush = $NewVersion + '.nupkg'
    Write-Host "Pushing NuGet pkg to VSTS: '$NewPkgToPush'..."
    & $NugetExe -Source "TeleoptiNugets" -ApiKey AzureDevOps $NewPkgToPush
            
} 
else
{
    Write-Host "Couldn´t retrive NuGet pkg name from VSTS"
}