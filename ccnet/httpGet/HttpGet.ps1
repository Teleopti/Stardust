param($FileName, $BlobStorage, $DestFolder, $UserName, $Password)

$sourceFile=$BlobStorage+"/"+$FileName
$destFile=$DestFolder+"\"+$FileName

$PathExist = Test-Path $DestFolder
If ($PathExist -eq $True) {
	Write-Host "Create webClient ..."
	$web = New-Object net.webclient
	Write-Host "Create webClient. Done!"
	
	if ($UserName)
	{
	Write-Host "Set Credentials ..."
	# $web.Credentials = new-object System.Net.NetworkCredential($UserName, $Password)
	$web.Credentials = new-object System.Net.NetworkCredential("tfsintegration", "m8kemew0rk")
	Write-Host "Set Credentials. Done!"
	}

	Write-Host "Download file: " $sourceFile " ..."
	$web.DownloadFile($sourceFile,$destFile);
	Write-Host "Download. Done"
}
Else
{
	Write-Host "the path: " $DestFolder " is missing! Abort ..."
}
