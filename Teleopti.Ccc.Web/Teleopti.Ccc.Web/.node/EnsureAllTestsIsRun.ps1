#Searching for the words fit & fdescribe in all spec.js files under web project. 
#To be sure that all tests is beeing run and no one accidentally commited it

$WFMRepoWebPath = (Resolve-Path "$PSScriptRoot\..\WFM").Path
function fcheck {
	$textSearch = "fdescribe\s*?\(|fit\s*?\("

	$MatchesJs = Get-ChildItem -Exclude vendor,node_modules -Path $WFMRepoWebPath |
	Get-ChildItem -Recurse -Filter *.spec.js |
	Select-String -Pattern $textSearch -List

	$MatchesTs = Get-ChildItem -Exclude vendor,node_modules -Path $WFMRepoWebPath |
	Get-ChildItem -Recurse -Filter *.spec.ts |
	Select-String -Pattern $textSearch -List

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