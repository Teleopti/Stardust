#Searching for the words fit & fdescribe in all spec.js files under web project. 
#To be sure that all tests is beeing run and no one accidentally commited it

$WFMRepoWebPath = (Resolve-Path "$PSScriptRoot\..\WFM").Path
function fcheck 
{

$MatchesJs = Get-ChildItem -Recurse -Filter *.spec.js $WFMRepoWebPath |
Select-String -Pattern "fdescribe\(|fit\(" -List
$MatchesTs = Get-ChildItem -Recurse -Filter *.spec.ts $WFMRepoWebPath |
Select-String -Pattern "fdescribe\(|fit\(" -List
$Paths = @($MatchesJs.Path) + @($MatchesTs.Path)
$Count = $MatchesJs.Matches.Count + $MatchesTs.Matches.Count
		
	if($Count -gt 0){
	Write-Host "Found $Count matches for fdescribe/fit in the following files:" -ForegroundColor Red
	foreach($path in $Paths) {
		Write-Host $path
	}
	exit 1
	}
}
	
fcheck