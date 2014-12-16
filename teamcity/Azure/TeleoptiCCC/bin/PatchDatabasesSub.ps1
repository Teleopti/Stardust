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
      [string]$SourceFolder,
      [string]$destinationFolder,
      [string]$filename
      )
    $BlobPath = TeleoptiDriveMapProperty-get -name "BlobPath"
    $AccountKey = TeleoptiDriveMapProperty-get -name "AccountKey"
    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"

	## Options to be added to AzCopy
	$OPTIONS = @("/XO","/Y","/sourceKey:$AccountKey")

    $BlobSource = $BlobPath + $SourceFolder

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

    $DBManagerFolder = $directory + "\..\Tools\Database"
    $settingsFile = "settings.txt"
    $fullPathsettingsFile =  $DBManagerFolder + "\" + $settingsFile

    $DataSourceName = TeleoptiDriveMapProperty-get -name "DataSourceName"
	
    $SettingContainer = "teleopticcc/Settings" + "/" + $DataSourceName
    $DbManagerContainer = "teleopticcc/dbmanager"
    
	
	#Remove-Item "$fullPathsettingsFile"
	if (Test-Path "$fullPathsettingsFile") {
		Remove-Item "$fullPathsettingsFile"
	}

    #Get customer specific config from BlobStorage
    CopyFileFromBlobStorage -SourceFolder "$SettingContainer" -destinationFolder "$DBManagerFolder" -filename "$settingsFile"

    #Get SQL Server info
	$FirstRow = (Get-Content $fullPathsettingsFile)[0 .. 0] | out-string
	$SecondRow = (Get-Content $fullPathsettingsFile)[1 .. 1] | out-string
	$ThirdRow = (Get-Content $fullPathsettingsFile)[2 .. 2] | out-string

	$a = ($FirstRow -split ";")
	$SQLServer = ($a[0] -split "=")[1]

	$SQLUser = ($a[3] -split "=")[1]

	$SQLPwd = ($a[4] -split "=")[1]
	$SQLPwd = $SQLPwd.Replace("`r","")
	$SQLPwd = $SQLPwd.Replace("`n","")

	$AnalyticsDB = $SecondRow.Substring($SecondRow.LastIndexOf("|") + 1)
	$AnalyticsDB = $AnalyticsDB.Replace("`r","")
	$AnalyticsDB = $AnalyticsDB.Replace("`n","")

	$CCC7DB = $ThirdRow.Substring($ThirdRow.LastIndexOf("|") + 1)
	$CCC7DB = $CCC7DB.Replace("`r","")
	$CCC7DB = $CCC7DB.Replace("`n","")

	$SQLServerFile = $SQLServer + ".txt"
	
	$fullPathSQLServerFile =  $DBManagerFolder + "\" + $SQLServerFile 

	#Get SQL Admin user and pwd
	#Remove-Item "$fullPathSQLServerFile"
	if (Test-Path "$fullPathSQLServerFile") {
		Remove-Item "$fullPathSQLServerFile"
	}

    CopyFileFromBlobStorage -SourceFolder "$DbManagerContainer" -destinationFolder "$DBManagerFolder" -filename "$SQLServerFile"

	$FirstRow = (Get-Content $fullPathSQLServerFile)[0 .. 0] | out-string
	$SecondRow = (Get-Content $fullPathSQLServerFile)[1 .. 1] | out-string

	$PATCHUSER = $FirstRow.Substring($FirstRow.LastIndexOf("|") + 1)
	$PATCHUSER = $PATCHUSER.Replace("`r","")
	$PATCHUSER = $PATCHUSER.Replace("`n","")

	$PATCHPWD = $SecondRow.Substring($SecondRow.LastIndexOf("|") + 1)
	$PATCHPWD = $PATCHPWD.Replace("`r","")
	$PATCHPWD = $PATCHPWD.Replace("`n","")

	#Call patch databases .bat file
	$PatchDBPath="$directory\..\Tools\Database"
	$PatchDBTool=$PatchDBPath + "\PatchAzureDatabases.bat"
    Remove-Item "$PatchDBPath\PatchDBs.bat"
	Add-Content "$PatchDBPath\PatchDBs.bat" "$PatchDBTool $SQLServer $CCC7DB $AnalyticsDB $PATCHUSER $PATCHPWD $SQLUser $SQLPwd"
    &"$PatchDBPath\PatchDBs.bat"
 
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