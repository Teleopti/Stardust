param($FileName, $BlobStorage, $DestFolder)

$sourceFile=$BlobStorage+"/"+$FileName
$destFile=$DestFolder+"\"+$FileName

$PathExist = Test-Path $DestFolder
If ($PathExist -eq $True) {
	$web = New-Object net.webclient

	Write-Host "Download file: " $sourceFile " ..."
	$web.DownloadFile($sourceFile,$destFile);
	Write-Host "Download. Done"
}
Else
{
	Write-Host "the path: " $DestFolder " is missing! Abort ..."
}
