function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value;
    if($Invocation.PSScriptRoot)
    {
        $Invocation.PSScriptRoot;
    }
    Elseif($Invocation.MyCommand.Path)
    {
        Split-Path $Invocation.MyCommand.Path
    }
    else
    {
        $Invocation.InvocationName.Substring(0,$Invocation.InvocationName.LastIndexOf("\"));
    }
}

function TeleoptiDriveMapProperty-get {
    Param(
      [string]$name
      )
    $computer = gc env:computername
    ## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
		switch ($name){
		BlobPath		{$TeleoptiDriveMapProperty="http://teleopticcc7.blob.core.windows.net/"; break}
		AccountKey		{$TeleoptiDriveMapProperty="IqugZC5poDWLu9wwWocT42TAy5pael77JtbcZtnPcm37QRThCkdrnzOh3HEu8rDD1S8E6dU5D0aqS4sJA1BTxQ=="; break}
		DataSourceName	{$TeleoptiDriveMapProperty="teleopticcc-dev"; break}
		default			{$TeleoptiDriveMapProperty="Unknown Value"; break}
        }
     }
    else {
		switch ($name){
		BlobPath		{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath"); break}     
		AccountKey		{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.AccountKey"); break}
		DataSourceName	{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName"); break}
		default			{$TeleoptiDriveMapProperty="Unknown Value"; break}
        }
           
    }
	return $TeleoptiDriveMapProperty
}


function CopyFileToBlobStorage {
    Param(
      [string]$SourceFolder,
      [string]$destinationFolder,
      [string]$filename
      )
	
    log-info "Copying file(s) to blob storage... $SourceFolder , $destinationFolder , $filename"
    $BlobPath = TeleoptiDriveMapProperty-get -name "BlobPath"
    $AccountKey = TeleoptiDriveMapProperty-get -name "AccountKey"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

	## Options to be added to AzCopy
	$OPTIONS = @("/XO","/Y")

    #$BlobSource = $BlobPath + $SourceFolder
	
	#Support AzCopy 6.3 version
	$BlobSourceArgs = "/Source:" + $SourceFolder
	$destinationFolderArgs = "/Dest:" + $BlobPath + $destinationFolder
	$filenameArgs = "/Pattern:" + $filename
    $AccountKeyArgs = "/DestKey:" + $AccountKey

	## Wrap all above arguments
	$cmdArgs = @("$BlobSourceArgs","$destinationFolderArgs","$filenameArgs", $OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	
    log-info "Copying logfile: '$filename' to '$BlobPath$destinationFolder'..."
    & "$AzCopyExe" ""$BlobSourceArgs"" ""$destinationFolderArgs"" ""$filenameArgs"" ""$AccountKeyArgs"" /Y""

    
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
		log-error "AzCopy generated an error!"
		log-info "AzCopy generated an error!"
        throw "AzCopy generated an error!"
    }
}

# Main

$directory = Get-ScriptDirectory
$computer = $env:computername

## Name of the job, name of source in Windows Event Log
$JOB = "Teleopti.Ccc.BlobStorageCopy"

Try
{

    #Get local path
    [string]$global:scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
    [string]$global:ScriptFileName = $MyInvocation.MyCommand.Name
    Set-Location $scriptPath

    #start log4net
    $log4netPath = $scriptPath + "\log4net"
    Unblock-File -Path "$log4netPath\log4net.ps1"
    . "$log4netPath\log4net.ps1";
    $configFile = new-object System.IO.FileInfo($log4netPath + "\log4net.config");
    configure-logging -configFile "$configFile" -serviceName "$serviceName"
	
    log-info "running: $ScriptFileName"

    $sourceFolder = $scriptPath

    [Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")

	$DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"
    $SettingContainer = "teleopticcc/Settings" + "/" + $DataSourceName
    $destinationFolder = $SettingContainer

    if ($DataSourceName -match "teleoptirnd")
    {

        #Create zip logfiles
		copy-item $sourceFolder\StartupLog.txt $sourceFolder\StartuplogCopy.txt -Force
        $toZip = gci $sourceFolder\* -Include *.txt,*.log -exclude CopyLogfilesToBlob.txt,Startuplog.txt
        Compress-Archive -Path $toZip -DestinationPath "LogFiles_$env:computername.zip" -Force
		
		CopyFileToBlobStorage -SourceFolder "$sourceFolder" -destinationFolder "$destinationFolder" -filename "LogFiles_$env:computername.zip"  
    }
    else
    {
        log-info "This script will only run on 'teleoptirnd' cloudservice..."
    }

}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
	log-error "$ErrorMessage"
	log-info "$ErrorMessage"
    Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for details"
}
Finally
{
    log-info "End of Script."
}