function FindAndReplace ([string]$FileName, [string]$searchText, [string]$replaceText)
{
	$file = Get-ChildItem $FileName -ErrorAction Stop
	foreach ($str in $file) 
	{
    	$content = Get-Content -path $str
    	$content | foreach {$_ -replace $searchText, $replaceText} | Set-Content $str
	}
}