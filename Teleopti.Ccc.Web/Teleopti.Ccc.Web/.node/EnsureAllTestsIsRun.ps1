#Searching for the words fit & fdescribe in all spec.js files under web project. 
#To be sure that all tests is beeing run and no one accidentally commited it

$WFMRepoWebPath = "$env:WorkingDirectory\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM"

function fcheck {
 $Matches = Get-ChildItem -Recurse -Filter *.spec.ts $WFMRepoWebPath |
 Select-String -Pattern "fdescribe\(|fit\(" -List
 $Paths = $Matches.Path
 $Count = $Matches.Matches.Count
 if($Count -gt 0) {
 Write-Host "Found $Count matches for fdescribe/fit in the following files:" -ForegroundColor Red
 Write-Host $Paths
 exit 1
 }

fcheck