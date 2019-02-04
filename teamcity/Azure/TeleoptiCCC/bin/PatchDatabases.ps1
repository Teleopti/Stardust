##===========
## Functions
##===========
function Virgin-Database {
    param (
        $connection,
        $ExePath,
        $DBmanagerParams,
        $DatabaseName
    ) 

    $CreateDatabaseVersion = (Get-Content "$scriptPath\..\Tools\Database\Create\CreateDatabaseVersion.sql") -join "`n"
    $lockResource = "VirginUpgrade"

    log-info "Try Get Applock: VirginUpgrade for $DatabaseName" 
    $gotLock = GetAppLock -Connection $connection -LockResource $lockResource
    if ($gotLock -eq $true)
    {
        log-info "got VirginUpgrade lock for $DatabaseName"
        if (Check-VirginDB -connection $connection)
        {
            log-info "Got a Virgin - create DB: " $CreateDatabaseVersion
            ExecuteNonQuery -connection $connection -cmdText $CreateDatabaseVersion

            log-info "Virgin patch"
            &"$ExePath" $DBmanagerParams[0] $DBmanagerParams[1] $DBmanagerParams[2] $DBmanagerParams[3] $DBmanagerParams[4] $DBmanagerParams[5] $DBmanagerParams[6] $DBmanagerParams[7] 
            log-info "DBManager came back as : $lastExitCode"

            log-info "Release Applock: Virgin Upgrade for $DatabaseName" 
            ReleaseAppLock -Connection $connection -LockResource $lockResource
            log-info "Release Applock: VirginUpgrade. Done!"
        }
        else
        {
        log-info "DB is already created and holds +1 tables"
        }
    }
    else
    {
        log-info "Could not Get Applock VirginUpgrade for: $DatabaseName" 
        $maxRetries = 40  ## 6min 40 secs
        log-info "Another process had the distributed lock for the virgin patch. Awaiting it to finish..."
        do
        {
            log-info "."
            Start-Sleep 10
            $maxRetries--
            $gotLock = GetAppLock -Connection $connection -LockResource $lockResource
        } until ($gotLock -or $maxRetries -lt 1)
		
        if ($gotLock)
        {
            log-info "VirginUpgrade lock on other process was released. Continuing without patching."
            ReleaseAppLock -Connection $connection -LockResource $lockResource
        }
        else
        {
            log-info "Warning, maximum wait time reached. Bailing out..."
            Return;
        }
    }
}


function Check-VirginDB {
    param ($connection) 
    if (!$connection) 
    { 
        write-Host "Check-VirginDB function called with no connection string" 
    }
    $ds = ExecQuery -connection $connection -cmdText "select count(*) as'TableCount' from sys.sysobjects where [type]='U'"
    $TableCount = $ds.Tables[0].Rows[0].TableCount

    if ($TableCount -eq 0)
    {
        return $True
    }
    else 
    {
        return $False
    }
}

function ExecuteNonQuery
{
    param ($connection, $cmdText)
        try 
        { 
            $Command = New-Object System.Data.SQLClient.SQLCommand 
            $Command.Connection = $connection 
            $Command.CommandText = $cmdText
             
            write-Host "Executing SQL Command..." 
            $Command.ExecuteNonQuery() 
        } 

        finally { 
        } 
}

function ExecQuery 
{ 
    param ($connection, $cmdText) 
    if (!$connection -or !$cmdText) 
    { 
        write-Host "ExecQuery function called with no connection and/or command text." 
    } 
    else 
    { 
        try 
        { 

            $Command = New-Object System.Data.SQLClient.SQLCommand 
            $Command.Connection = $connection 
            $Command.CommandText = $cmdText
             
            $adapter = New-Object System.Data.sqlclient.sqlDataAdapter $command
            $dataset = New-Object System.Data.DataSet
            $adapter.Fill($dataSet) | Out-Null

            return $dataSet
        } 

        finally {
        } 
    } 
}

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

    #$BlobSource = $BlobPath + $SourceFolder
	
	#Support AzCopy 6.3 version
	$BlobSourceArgs = "/Source:" + $BlobPath + $SourceFolder
	$destinationFolderArgs = "/Dest:" + $destinationFolder
	$filenameArgs = "/Pattern:" + $filename

	## Wrap all above arguments
	$cmdArgs = @("$BlobSourceArgs","$destinationFolderArgs","$filenameArgs", $OPTIONS)

	$AzCopyExe = $directory + "\ccc7_azure\AzCopy\AzCopy.exe"
	$AzCopyExe

	## Start the azcopy with above parameters and log errors in Windows Eventlog.
	& $AzCopyExe @cmdArgs
    $AzExitCode = $LastExitCode
    
    if ($LastExitCode -ne 0) {
		log-error "AzCopy generated an error!"
		log-info "AzCopy generated an error!"
        throw "AzCopy generated an error!"
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
	
	#Test - Tabort
	log-info "Environment variable DotJonsson = '$Env:DotJonsson'"

	##test if admin
	If (!(Test-Administrator($myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()))) {
		log-error "User is not Admin!"
		throw "User is not Admin!"
	}
	
	$Env:TeleoptiIsAzure = $true
	log-info "Environment variable 'TeleoptiIsAzure' is set to '$Env:TeleoptiIsAzure'"
		
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

    #find out if this master tenant is a fresh deployement from CloudOps
    #If so - patch the database once so that the Sercurity.exe can do it's thing on required tables
    $VirginConnApp = "Data Source=$SQLServer;Initial Catalog=$CCC7DB;User Id=$PATCHUSER;Password=$PATCHPWD;Current Language=us_english;Encrypt=True;trustServerCertificate=false"
    $VirginConnAnal = "Data Source=$SQLServer;Initial Catalog=$AnalyticsDB;User Id=$PATCHUSER;Password=$PATCHPWD;Current Language=us_english;Encrypt=True;trustServerCertificate=false"
    $command = $PatchDBPath + "\DBManager.exe"

    $DBMSQLServer="-S"+$SQLServer
    $DBMPATCHUSER = "-U"+$PATCHUSER
    $DBMPATCHPWD= "-P"+$PATCHPWD
    $DBMappDb = "-D"+$CCC7DB
    $DBManalDb =  "-D"+$AnalyticsDB
    $ExePath = $PatchDBPath + "\DBManager.exe"

    ##Analytics
    $connection = $null
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $VirginConnAnal
    $DBmanagerParams = @(
        $DBMSQLServer,
        $DBManalDb,
        "-OTeleoptiAnalytics",
        $DBMPATCHUSER,
        $DBMPATCHPWD,
        "-T",
        "-R",
        $SQLUserPwd
    )

    $connection.open()
        Virgin-Database -connection $connection -ExePath $ExePath -DBmanagerParams $DBmanagerParams -DatabaseName $AnalyticsDB
    $connection.Close()

    ##App
    $connection = $null
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $VirginConnApp
    $DBmanagerParams = @(
        $DBMSQLServer,
        $DBMappDb,
        "-OTeleoptiCCC7",
        $DBMPATCHUSER,
        $DBMPATCHPWD,
        "-T",
        "-R",
        $SQLUserPwd
    )

    $connection.open()
        Virgin-Database -connection $connection -ExePath $ExePath -DBmanagerParams $DBmanagerParams -DatabaseName $CCC7DB
    $connection.Close()

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
            if ($array[$i] -like '*User ID*') { 
                $cat = ($array[$i] -split "=")
                $SQLUser = $cat[1]
            }
            if ($array[$i] -like '*Password*') { 
                $cat = ($array[$i] -split "=")
                $SQLPwd = $cat[1]
            }
           if ($array[$i] -like '*Data Source*') { 
                $cat = ($array[$i] -split "=")
                $SQLServerTenant = $cat[1]
            }


        }

        #make sure all connection string have a valid (same) server as master tenant
        if (!$SQLServer.Contains($SQLServerTenant))
        {
            log-info "$SQLServerTenant not equal to the main server $SQLServer!!!"
            continue
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
        $SQLUserPwd = "-L$SQLUser$colon$SQLPwd"

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
	
    $cmd.CommandText = "UPDATE Tenant.Tenant SET ApplicationConnectionString = ApplicationConnectionString + ';Encrypt=True;trustServerCertificate=false' WHERE ApplicationConnectionString NOT LIKE '%Encrypt=True;trustServerCertificate=false%'"
    $rowsAffected = $cmd.ExecuteNonQuery()

    $cmd.CommandText = "UPDATE Tenant.Tenant SET AnalyticsConnectionString = AnalyticsConnectionString + ';Encrypt=True;trustServerCertificate=false' WHERE AnalyticsConnectionString NOT LIKE '%Encrypt=True;trustServerCertificate=false%'"
    $rowsAffected = $cmd.ExecuteNonQuery()

    # Releasing distributed lock
	ReleaseAppLock -Connection $con -LockResource $lockResource
	
    # Close the connection
    $con.Close()
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