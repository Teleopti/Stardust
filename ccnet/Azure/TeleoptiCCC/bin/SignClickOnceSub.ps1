##===========
## Functions
##===========
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

function Test-Administrator
{
	[CmdletBinding()]
	param($currentUser)
	$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)

	# Get the security principal for the Administrator role
	$adminRole=[System.Security.Principal.WindowsBuiltInRole]::Administrator

	# Check to see if we are currently running "as Administrator"
	return ($myWindowsPrincipal.IsInRole($adminRole))
}

function DataSourceName-get {
    $computer = gc env:computername
	## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
	$DataSourceName = "teleopticcc-dev"
    }
    else {
	$DataSourceName = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName")
    }
	return $DataSourceName
}

function BlobPath-get {
    $computer = gc env:computername
    ## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
	$BlobPath = "teleopticcc-dev"
    }
    else {
	$BlobPath = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath")
    }
	return $BlobPath
}

function BlobPath-get {
    $computer = gc env:computername
    ## Local debug values
    if ($computer.ToUpper().StartsWith("TELEOPTI")) {
	$BlobPath = "teleopticcc-dev"
    }
    else {
	$BlobPath = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath")
    }
	return $BlobPath
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
		ContainerName	{$TeleoptiDriveMapProperty="teleopticcc/Settings"; break}
		AccountKey		{$TeleoptiDriveMapProperty="IqugZC5poDWLu9wwWocT42TAy5pael77JtbcZtnPcm37QRThCkdrnzOh3HEu8rDD1S8E6dU5D0aqS4sJA1BTxQ=="; break}
		DataSourceName	{$TeleoptiDriveMapProperty="teleopticcc-dev"; break}
		default			{$TeleoptiDriveMapProperty="Unknown Value"; break}
        }
     }
    else {
		switch ($name){
		BlobPath		{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.BlobPath"); break}
        ContainerName	{$TeleoptiDriveMapProperty="teleopticcc/Settings"; break}        
		AccountKey		{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.AccountKey"); break}
		DataSourceName	{$TeleoptiDriveMapProperty = [Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetConfigurationSettingValue("TeleoptiDriveMap.DataSourceName"); break}
		default			{$TeleoptiDriveMapProperty="Unknown Value"; break}
        }
           
    }
	return $TeleoptiDriveMapProperty
}

function EventlogSource-Create {
    param([string]$EventSourceName)
    $type = "Application"
    #create event log source
    if ([System.Diagnostics.EventLog]::SourceExists("$EventSourceName") -eq $false) {
        [System.Diagnostics.EventLog]::CreateEventSource("$EventSourceName", $type)
        }
}

function CopyFileFromBlobStorage {
    Param(
      [string]$destinationFolder,
      [string]$filename
      )
    $BlobPath = TeleoptiDriveMapProperty-get -name "BlobPath"
    $AccountKey = TeleoptiDriveMapProperty-get -name "AccountKey"
    $ContainerName = TeleoptiDriveMapProperty-get -name "ContainerName"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

	## Options to be added to AzCopy
	$OPTIONS = @("/XO","/Y","/sourceKey:$AccountKey")

    $BlobSource = $BlobPath + $ContainerName + "/" + $DataSourceName

	## Wrap all above arguments
	$cmdArgs = @("$BlobSource","$destinationFolder","$filename", $OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
        throw "AsCopy generated an error!"
    }
}

##===========
## Main
##===========
$directory = Get-ScriptDirectory
write-host $directory
$computer = gc env:computername

## Name of the job, name of source in Windows Event Log
$JOB = "Teleopti.Ccc.BlobStorageCopy"


Try
{
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")

	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		throw "User is not Admin!"
	}

    #create event log source
    EventlogSource-Create "$JOB"

    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"
    
    #Sign ClickOnce
    $ClickOnceSignPath="$directory\..\Tools\ClickOnceSign"
    CD e:
    Set-Location $ClickOnceSignPath
    $ClickOnceTool = $ClickOnceSignPath + "\ClickOnceSign.exe"

    $ClientPath="$directory\..\..\sitesroot\5"
    $MyTimePath="$directory\..\..\sitesroot\4"
    $pwd ="`"`""

	#Wait a bit
	Start-Sleep -s 30
	
    $cmdArgs = @("-s","-a Teleopti.Ccc.SmartClientPortal.Shell.application", "-m Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest","-u https://$DataSourceName.teleopticloud.com/Client/","-c $ClickOnceSignPath\TemporaryKey.pfx","-dir $ClientPath","-p $pwd")
    Remove-Item "$ClickOnceSignPath\SignAdminClient.bat"
	Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "E:"
	Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "CD $ClickOnceSignPath"
    Add-Content "$ClickOnceSignPath\SignAdminClient.bat" "$ClickOnceTool $cmdArgs"
    #&"$ClickOnceSignPath\SignAdminClient.bat"
    
    $cmdArgs = @("-s","-a Teleopti.Ccc.AgentPortal.application","-m Teleopti.Ccc.AgentPortal.exe.manifest","-u https://$DataSourceName.teleopticloud.com/MyTime/","-c $ClickOnceSignPath\TemporaryKey.pfx","-dir $MyTimePath","-p $pwd")
    Remove-Item "$ClickOnceSignPath\SignMyTime.bat"
	Add-Content "$ClickOnceSignPath\SignMyTime.bat" "E:"
	Add-Content "$ClickOnceSignPath\SignMyTime.bat" "CD $ClickOnceSignPath"
    Add-Content "$ClickOnceSignPath\SignMyTime.bat" "$ClickOnceTool $cmdArgs"
    #&"$ClickOnceSignPath\SignMyTime.bat"
 
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
    Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for detils"
}
Finally
{
    Write-Host "done"
}
