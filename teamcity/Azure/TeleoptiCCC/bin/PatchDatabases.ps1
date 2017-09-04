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
    Param([string]$EventSourceName)
	log-info "Creating event log..."
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
	log-info "Copying file from blob storage... $SourceFolder , $destinationFolder , $filename"
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
		log-error "AsCopy generated an error!"
        throw "AsCopy generated an error!"
    }
}

function GetAppLock {
	Param(
		[System.Data.SqlClient.SqlConnection] $Connection,
        [string] $LockResource
	)
	log-info "Getting distributed lock $LockResource"
	$cmd = New-Object System.Data.SqlClient.SqlCommand
    $cmd.Connection = $connection
	$cmd.CommandText = "sp_getapplock"
    $cmd.CommandType = [System.Data.CommandType]::StoredProcedure;
	$cmd.Parameters.AddWithValue("@Resource", $LockResource) | Out-Null
	$cmd.Parameters.AddWithValue("@LockMode", "Exclusive") | Out-Null
	$cmd.Parameters.AddWithValue("@LockOwner", "Session") | Out-Null
	$cmd.Parameters.AddWithValue("@LockTimeout", 0) | Out-Null
	$cmd.Parameters.AddWithValue("@DbPrincipal", "public") | Out-Null

    $outParameter = new-object System.Data.SqlClient.SqlParameter;
    $outParameter.ParameterName = "@Result";
    $outParameter.Direction = [System.Data.ParameterDirection]::ReturnValue
    $outParameter.DbType = [System.Data.DbType]::Int32;
    $cmd.Parameters.Add($outParameter) | Out-Null

    $affectedRows = $cmd.ExecuteNonQuery();
    $result = [int]$cmd.Parameters["@Result"].Value;
    log-info "Getting distributed lock result $result"
    return $result -eq 0;
}

function ReleaseAppLock {
	Param(
		[System.Data.SqlClient.SqlConnection] $Connection,
        [string] $LockResource
	)
    log-info "Releasing distributed lock $LockResource"
	$cmd = New-Object System.Data.SqlClient.SqlCommand
    $cmd.Connection = $connection
	$cmd.CommandText = "sp_releaseapplock"
    $cmd.CommandType = [System.Data.CommandType]::StoredProcedure;
	$cmd.Parameters.AddWithValue("@Resource", $LockResource) | Out-Null
	$cmd.Parameters.AddWithValue("@LockOwner", "Session") | Out-Null

    $outParameter = new-object System.Data.SqlClient.SqlParameter;
    $outParameter.ParameterName = "@Result";
    $outParameter.Direction = [System.Data.ParameterDirection]::ReturnValue;
    $outParameter.DbType = [System.Data.DbType]::Int32;
    $cmd.Parameters.Add($outParameter) | Out-Null

    $affectedRows = $cmd.ExecuteNonQuery();
    $result = [int]$cmd.Parameters["@Result"].Value;
    log-info "Releasing lock result $result"
}

##===========
## Main
##===========
$directory = Get-ScriptDirectory
$computer = gc env:computername

## Name of the job, name of source in Windows Event Log
$JOB = "Teleopti.Ccc.BlobStorageCopy"

Try
{
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime")
	
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

	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		log-error "User is not Admin!"
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

    $colon = ":"
    $SQLUserPwd = "-L$SQLUser$colon$SQLPwd"
    $CONNSTRINGBASE="-CSServer=$SQLServer;UID=$SQLUser;Password=$SQLPwd"

    log-info "SQLUserPwd " $SQLUserPwd

	$AnalyticsDB = $SecondRow.Substring($SecondRow.LastIndexOf("|") + 1)
	$AnalyticsDB = $AnalyticsDB.Replace("`r","")
	$AnalyticsDB = $AnalyticsDB.Replace("`n","")

	$CCC7DB = $ThirdRow.Substring($ThirdRow.LastIndexOf("|") + 1)
	$CCC7DB = $CCC7DB.Replace("`r","")
	$CCC7DB = $CCC7DB.Replace("`n","")

	$SQLServerFile = $SQLServer.Replace("tcp:", "") + ".txt"
	
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

	#log-info "Running PatchDBs.bat..."
	#Call patch databases .bat file
	$PatchDBPath="$directory\..\Tools\Database"
	#$PatchDBTool=$PatchDBPath + "\PatchAzureDatabases.bat"
    #Remove-Item "$PatchDBPath\PatchDBs.bat"
	#Add-Content "$PatchDBPath\PatchDBs.bat" "$PatchDBTool $SQLServer $CCC7DB $AnalyticsDB $PATCHUSER $PATCHPWD $SQLUser $SQLPwd"
    #&"$PatchDBPath\PatchDBs.bat"

    #log-info "Check so one tenant points to itself"
    $CCC7DB = $CCC7DB.Trim()
    $checkConn = "-TSServer=$SQLServer;Database=$CCC7DB;UID=$PATCHUSER;Password=$PATCHPWD"

    $command = $PatchDBPath + "\Enrypted\Teleopti.Support.Security.exe"
        &"$command" "-CT1" $checkConn

    log-info "Get databases to patch"
     # Create SqlConnection object and define connection string
    $con = New-Object System.Data.SqlClient.SqlConnection
    $con.ConnectionString = "Server=$SQLServer;Database=$CCC7DB;User ID=$PATCHUSER;Password=$PATCHPWD"
    $con.open()
    log-info "opened " $con.ConnectionString

    $cmd = New-Object System.Data.SqlClient.SqlCommand
    $cmd.Connection = $con

    $cmd.CommandText = "select count(*) from Tenant.Tenant"
    $NumberOfTenants = $cmd.ExecuteScalar();
    if ($NumberOfTenants -eq 1) {
		$ApplicationConnectionString = "Data Source=$SQLServer;Initial Catalog=$CCC7DB;User Id=$SQLUser;Password=$SQLPwd;Current Language=us_english;Encrypt=True;trustServerCertificate=false"
		$AnalyticsConnectionString = "Data Source=$SQLServer;Initial Catalog=$AnalyticsDB;User Id=$SQLUser;Password=$SQLPwd;Current Language=us_english;Encrypt=True;trustServerCertificate=false"

		$cmd.CommandText = "UPDATE Tenant.Tenant SET ApplicationConnectionString = '$ApplicationConnectionString', AnalyticsConnectionString = '$AnalyticsConnectionString'"
		$rowsAffected = $cmd.ExecuteNonQuery()
    }


	# Getting distributed lock to make sure no one else is updating the databases as well.
	$lockResource = "AzurePatchDatabaseLock"
	$gotLock = GetAppLock -Connection $con -LockResource $lockResource
	if ($gotLock -eq $false)
	{
		$maxRetries = 80
		log-info "Another process had the distributed lock for patching the database. Awaiting it to finish."
		do
		{
			log-info "."
			Start-Sleep 10
			$maxRetries--
			$gotLock = GetAppLock -Connection $con -LockResource $lockResource
		} until ($gotLock -or $maxRetries -lt 1)
		
		if ($gotLock)
		{
			log-info "Lock on other process was released. Continuing without patching because database was patched in another instance."
			ReleaseAppLock -Connection $con -LockResource $lockResource
		}
		else
		{
			log-info "Warning, maximum wait time reached. Bailing out..."
		}
		return;
	}

    # Create SqlCommand object, define command text, and set the connection
    $cmd = New-Object System.Data.SqlClient.SqlCommand
    $cmd.Connection = $con

 
    $cmd.CommandText = "UPDATE Tenant.Tenant SET ApplicationConnectionString = REPLACE(ApplicationConnectionString, 'Data Source=', 'Data Source=tcp:') WHERE ApplicationConnectionString NOT LIKE 'Data Source=tcp:%'"
    $rowsAffected = $cmd.ExecuteNonQuery()

    $cmd.CommandText = "UPDATE Tenant.Tenant SET AnalyticsConnectionString = REPLACE(AnalyticsConnectionString, 'Data Source=', 'Data Source=tcp:') WHERE AnalyticsConnectionString NOT LIKE 'Data Source=tcp:%'"
    $rowsAffected = $cmd.ExecuteNonQuery()

    $cmd.CommandText = "SELECT ApplicationConnectionString, AnalyticsConnectionString FROM Tenant.Tenant"
    # Create SqlDataReader
    $dr = $cmd.ExecuteReader()
    
    $SECSQLServer="-DS"+$SQLServer
    $SECPATCHUSER = "-DU"+$PATCHUSER
    $SECPATCHPWD= "-DP"+$PATCHPWD

    $SQLServer="-S"+$SQLServer
    $PATCHUSER = "-U"+$PATCHUSER
    $PATCHPWD= "-P"+$PATCHPWD

    If ($dr.HasRows)
    {
        log-info "Found databases"
      While ($dr.Read())
      {
        $array = ($dr["ApplicationConnectionString"] -split ";")
        for ($i=0; $i -lt $array.length; $i++) {
	        if ($array[$i] -like '*Initial Catalog*') { 
                $cat = ($array[$i] -split "=")
                $appDb = $cat[1]
            }
        }
        $array = ($dr["AnalyticsConnectionString"] -split ";")
        for ($i=0; $i -lt $array.length; $i++) {
	        if ($array[$i] -like '*Initial Catalog*') { 
                $cat = ($array[$i] -split "=")
                $analDb = $cat[1]
            }
        }
        $SECappDb = "-AP"+$appDb
        $SECanalDb =  "-AN"+$analDb
        $SECloggDb =  "-CD"+$analDb

        log-info "Patch databases: $appDb $analDb."

        $appDb = "-D"+$appDb
        $analDb =  "-D"+$analDb

        
        $command = $PatchDBPath + "\DBManager.exe"

        &"$command" $SQLServer $appDb "-OTeleoptiCCC7" $PATCHUSER $PATCHPWD "-T" "-R" $SQLUserPwd
        &"$command" $SQLServer $analDb "-OTeleoptiAnalytics" $PATCHUSER $PATCHPWD "-T" "-R" $SQLUserPwd

        $command = $PatchDBPath + "\Enrypted\Teleopti.Support.Security.exe"
        &"$command" $SECSQLServer $SECappDb $SECanalDb $SECloggDb $CONNSTRINGBASE $SECPATCHUSER $SECPATCHPWD
      }
    }
	
    # Close the data reader 
    $dr.Close()
	
    # Releasing distributed lock
	ReleaseAppLock -Connection $con -LockResource $lockResource
	
    # Close the connection
    $con.Close()
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
	log-error "$ErrorMessage"
    Write-EventLog -LogName Application -Source $JOB -EventID 1 -EntryType Error -Message "$ErrorMessage"
	Throw "Script failed, Check Windows event log for details"
}
Finally
{
    log-info "Done!"
}