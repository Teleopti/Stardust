Framework "4.0"

properties {
	
    #Default Properties
    $global:SQLServerInstance = "$env:SQLServerInstance"        #Name of SQL Server for exampel: s8v4m110k9.database.windows.net or Erebus\SQL2008R2
    $global:TestEdition = "$env:TestEdition"                    #Name of SQL Edition for exampel: SQL Azure or "Standard Edition (64-bit)
    $global:TestMajorMinor = "$env:TestMajorMinor"              #Name of SQL Version for exampel: 12.0 or 10.50
    $global:Configuration = "$env:Configuration"
	$global:AdminSqlLogin = "$env:AdminSqlLogin"
	$global:AdminSqlPwd = "$env:AdminSqlPwd"
    
    $global:InstallAndPatchSqlLogin = 'teamcityInstallAndPatch'
	$global:InstallAndPatchSqlPwd = 'asldkj345HHG'
	$global:ApplicationDbLogin = 'teamcityApplication'
	$global:ApplicationDbPwd = 'V3rrYH@Passw0rd!'
	
	#Constants
    $WorkingDirectory = $PSScriptRoot
    $AzureAdminSqlLogin = 'teleopti'
	$AzureAdminSqlPwd = 'T3l30pt1'
	$SQL2008R2 = '10.50'
	$SQLAzure = 'SQL Azure'
	$DbNamePrefix = 'teamcity'
	$AppDB = "$DbNamePrefix"+"_AppDB"
	$MartDB = "$DbNamePrefix"+"_MartDB"
	$DbManagerExe = "$WorkingDirectory\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\$Configuration\DBManager.exe"
    $SecurityExe = "$WorkingDirectory\..\Teleopti.Support.Security\bin\$Configuration\Teleopti.Support.Security.exe"
	$DatabasePath = "$WorkingDirectory\..\database"
    $global:DropInstallAndPatchSqlLoginCommand = "DROP LOGIN [$global:InstallAndPatchSqlLogin]"
	$global:DropApplicationDbLoginCommand = "DROP LOGIN [$global:ApplicationDbLogin]"
}

# Dot sourcing support files
    . "$PSScriptroot\TransientSqlSupport.ps1"

# Initialize Default Retry Policy in TransientSqlSupport.ps1
	SetDefaultRetryPolicy

# Set-ExecutionPolicy bypass -force
    Import-module .\teamcity.psm1 -Force

TaskSetup {
    
    TeamCity-ReportBuildProgress "Running task $($psake.context.Peek().currentTaskName)"
}

task default -depends InitSetup,`
                      TeardownAndPrepare,`
                      CreateDatabase,`
                      PatchDatabase,`
                      DataModifications,`
                      RunTestsOnAllDB,`
                      TeardownAndCleanup
 
task InitSetup -description "Checking SQL Version & Edition" {
	
	Write-Output "##teamcity[blockOpened name='<InitSetup>']"
	CleanUpLog
    SettingsAndPreparation
	Write-Output "##teamcity[blockClosed name='<InitSetup>']"
}

task TeardownAndPrepare -depends InitSetup -description "Drop Database & Create logins" {

	Write-Output "##teamcity[blockOpened name='<TeardownAndPrepare>']"
	DropDatabase
	CreateInstallAndPatchSqlLoginCommandTarget
    GrantServerRoles
	Write-Output "##teamcity[blockClosed name='<TeardownAndPrepare>']"
}

task CreateDatabase -depends InitSetup, TeardownAndPrepare -description "Create databases with Sql Admin" {

	Write-Output "##teamcity[blockOpened name='<CreateDatabase>']"
    CreateDatabaseWithSQLAdmin
    RevokeServerRoles
	Write-Output "##teamcity[blockClosed name='<CreateDatabase>']"
}

task PatchDatabase -depends InitSetup, TeardownAndPrepare, CreateDatabase -description "Patch Database With Dbo Only" {
	
	Write-Output "##teamcity[blockOpened name='<PatchDatabase>']"
    PatchDatabaseWithDboOnly
	Write-Output "##teamcity[blockClosed name='<PatchDatabase>']"
}

task DataModifications -depends InitSetup, TeardownAndPrepare, CreateDatabase, PatchDatabase -description "Run Security Modifications" {

	Write-Output "##teamcity[blockOpened name='<DataModifications>']"
    Datamodifications
	Write-Output "##teamcity[blockClosed name='<DataModifications>']"
}

task RunTestsOnAllDB -depends InitSetup, TeardownAndPrepare, CreateDatabase, PatchDatabase, DataModifications -description "Run Tests" {

	Write-Output "##teamcity[blockOpened name='<RunTestsOnAllDB>']"
    ScriptedTestsRunOnAllDBs
    AnalyticsReportsTest
	Write-Output "##teamcity[blockClosed name='<RunTestsOnAllDB>']"
}

task TeardownAndCleanup -depends InitSetup, TeardownAndPrepare, CreateDatabase, PatchDatabase, DataModifications, RunTestsonAllDB -description "Drop logins and clean logs" {

	Write-Output "##teamcity[blockOpened name='<TeardownAndCleanup>']"
    DropDatabase
    DropPatchLogin
    DropApplicationLogin
    Write-Output "##teamcity[blockClosed name='<TeardownAndCleanup>']"
}

function global:CheckEditionAndVersion () 
{
    param (
    $query = ""

    )
    
	$ConnectionString="Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    Log "Checking SQL Version & Edition on [$global:SQLServerInstance]"		
	$CheckVersion = ExecuteSQLQuery $ConnectionString $query
	$global:SQLVersion = $CheckVersion.Column1
    $global:SQLVersionMajor = $global:sqlversion.split(".")[0]
    $global:SQLVersionMinor = $global:sqlversion.split(".")[1]
    $global:SQLVersion = $global:SQLVersionMajor + "." + $global:SQLVersionMinor
	$global:SQLEdition = $CheckVersion.Column2

    return $global:SQLVersion, $global:SQLEdition | out-null
}

function global:SettingsAndPreparation {
    
    if ($global:TestEdition -eq $SQLAzure) {
        
        $global:AdminSqlLogin = $AzureAdminSqlLogin
	    $global:AdminSqlPwd = $AzureAdminSqlPwd
    }
	
	#Checking SQL edition and product version
    $query = @("SELECT SERVERPROPERTY('productversion'), SERVERPROPERTY ('edition')")
    CheckEditionandVersion -query $query
    
    Log "------------------------------------------------------------------"
    Log "SQL Version:                [$global:SQLVersion]"
    Log "SQL Edition:                [$global:SQLEdition]"
    Log "AdminSqlLogin:              [$global:AdminSqlLogin]"
    Log "AdminSqlPwd:                [$global:AdminSqlPwd]"
    Log "InstallAndPatchSqlLogin:    [$global:InstallAndPatchSqlLogin]"
    Log "InstallAndPatchSqlPwd:      [$global:InstallAndPatchSqlPwd]"
    Log "ApplicationDbLogin:         [$global:ApplicationDbLogin]"
    Log "ApplicationDbPwd:           [$global:ApplicationDbPwd]"
    Log "------------------------------------------------------------------"

    # Check that SQL Edition is correct
    if (!($global:SQLEdition -eq $TestEdition)) { 
    
		Log "Expected SQLEdition: [$TestEdition], but was: [$global:SQLEdition]" 
		exit 1
    }
            
        if ($global:SQLEdition -eq $SQLAzure) { 
    
			$global:InstallAndPatchSqlLogin = $AzureAdminSqlLogin
			$global:InstallAndPatchSqlPwd = $AzureAdminSqlPwd
			$global:AggDB = "$DbNamePrefix"+"_MartDB"
		}
		
        elseif (!($SQLEdition -eq $SQLAzure)) {
		
			#SQL Setup Queries on Ground
			$global:CreateGroundLoginCmd = "CREATE LOGIN [$global:InstallAndPatchSqlLogin] WITH PASSWORD=N'$InstallAndPatchSqlPwd', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF"
			$global:AggDB = "$DbNamePrefix"+"_AggDB"
        
			$global:DropDbCmd = "ALTER DATABASE [$AppDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$AppDB];ALTER DATABASE [$MartDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$MartDB];ALTER DATABASE [$global:AggDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$global:AggDB]"
			$global:RevokeSrvRoleCmd = "ALTER SERVER ROLE [dbcreator] DROP MEMBER [$global:InstallAndPatchSqlLogin];ALTER SERVER ROLE [securityadmin] DROP MEMBER [$global:InstallAndPatchSqlLogin]"
			$global:GrantSrvRoleCmd = "ALTER SERVER ROLE [dbcreator] ADD MEMBER [$global:InstallAndPatchSqlLogin];ALTER SERVER ROLE [securityadmin] ADD MEMBER [$global:InstallAndPatchSqlLogin]"
        }
		
		#Setting DB Manager & Teleopti.Support.Security.exe parameters
        $global:DBManagerString = "-S$global:SQLServerInstance -U$global:InstallAndPatchSqlLogin -P$global:InstallAndPatchSqlPwd -T -L$global:ApplicationDbLogin`:$global:ApplicationDbPwd"
        $global:SecurityExeString = "-DS$global:SQLServerInstance -DU$global:InstallAndPatchSqlLogin -DP$global:InstallAndPatchSqlPwd"
        
        #Check that SQL version is correct
        if (!($TestMajorMinor -eq $global:SQLVersion)) { 
    
			Log "Expected SQL Version: [$global:TestMajorMinor], but was: [$global:SQLVersion]" 
			exit 1
        }

        elseif ($global:TestMajorMinor -eq $SQL2008R2) {

			$global:GrantSrvRoleCmd = "EXEC master..sp_addsrvrolemember @loginame = N'$global:InstallAndPatchSqlLogin', @rolename = N'dbcreator';EXEC master..sp_addsrvrolemember @loginame = N'$global:InstallAndPatchSqlLogin', @rolename = N'securityadmin'"
			$global:RevokeSrvRoleCmd = "EXEC master..sp_dropsrvrolemember @loginame = N'$global:InstallAndPatchSqlLogin', @rolename = N'dbcreator';EXEC master..sp_dropsrvrolemember @loginame = N'$global:InstallAndPatchSqlLogin', @rolename = N'securityadmin'"
       	}
}

function checkIfDbExists {
    
    Param (
        $DB 
    )

    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"    
    
    $query = "SELECT COUNT(*) FROM sys.sysdatabases where name='$DB'"
    $count = RunAndRetryScalar $ConnectionString $query
    $count = $count[$count.Count - 1]
    return $count
}

function global:DropDatabase () {
   
    if (!($global:SQLEdition -eq $SQLAzure)) {

		$ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    
		# Check if Databases exists on Ground
		$appDbExists = checkIfDbExists $AppDB
		$martDbExists = checkIfDbExists $MartDB
		$aggDbExists = checkIfDbExists $global:AggDB

        if ($appDbExists -or $martDbExists -or $aggDbExists -gt 0) {

			$global:DropDbCmd = "ALTER DATABASE [$AppDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$AppDB];ALTER DATABASE [$MartDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$MartDB];ALTER DATABASE [$global:AggDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$global:AggDB]"

			Log "Dropping Databases: [$AppDB] & [$MartDB] & [$global:AggDB] on $global:SQLServerInstance"
			RunAndRetryNonQuery $ConnectionString $global:DropDbCmd
        } 
    }

    elseif ($global:SQLEdition -eq $SQLAzure) {

		$ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
		$Query_DropAzureDB_Mart = "DROP DATABASE $MartDB"
		$Query_DropAzureDB_App = "DROP DATABASE $AppDB"

		# Check if Databases exists on Azure
		$appDbExists = checkIfDbExists $AppDB
		$martDbExists = checkIfDbExists $MartDB
		
		if ($appDbExists -gt 0) {

			Log "Dropping Database: $AppDB on $SQLAzure"
			RunAndRetryNonQuery $ConnectionString $Query_DropAzureDB_App        
        } 
    	if ($martDbExists -gt 0) {
			
			Log "Dropping Database: $MartDB on $SQLAzure"
			RunAndRetryNonQuery $ConnectionString $Query_DropAzureDB_Mart
		}
	}
}

function global:CreateInstallAndPatchSqlLoginCommandTarget () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

		$ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
		$query_droplogin = "$global:DropInstallAndPatchSqlLoginCommand"
		$query_createlogin = "$global:CreateGroundLoginCmd"

		$Login = Test-SQLLogin -SqlLogin "$global:InstallAndPatchSqlLogin"

		if ($Login) {
       
			Log "Dropping login: [$global:InstallAndPatchSqlLogin] on [$global:SQLServerInstance]"
			RunAndRetryNonQuery $ConnectionString $query_droplogin
        }
        
        Log "Create login: [$global:InstallAndPatchSqlLogin] on [$global:SQLServerInstance]"
		RunAndRetryNonQuery $ConnectionString $query_createlogin
		
    }
}

function global:DropPatchLogin () {

    if (!($global:SQLEdition -eq $SQLAzure)) {
        
        $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
        $query_droplogin = "$global:DropInstallAndPatchSqlLoginCommand"

        Log "Dropping login: [$global:InstallAndPatchSqlLogin] on [$global:SQLServerInstance]"
        ExecuteSqlQuery $ConnectionString $query_droplogin
    }
}

function global:DropApplicationLogin () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

        $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
        $query = "$global:DropApplicationDbLoginCommand"

        Log "Dropping Application login: [$global:ApplicationDbLogin] on [$global:SQLServerInstance] "
        ExecuteSqlQuery $ConnectionString $query
    }
}   

function global:GrantServerRoles () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

		$ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
		$query = "$global:GrantSrvRoleCmd"

		Log "Adding: [$global:InstallAndPatchSqlLogin] as Admin on: [$global:SQLServerInstance]"
		RunAndRetryNonQuery $ConnectionString $query
    }
}

function global:CreateDatabaseWithSQLAdmin () {
    
    $Params = "$global:DBManagerString -C -D$MartDB -OTeleoptiAnalytics -F$DatabasePath"
    $Prms = $Params.Split(" ")
    & "$DbManagerExe" $Prms

    $Params = "$global:DBManagerString -C -D$AppDB -OTeleoptiCCC7 -F$DatabasePath"
    $Prms = $Params.Split(" ")
    & "$DbManagerExe" $Prms

    if (!($SQLEdition -eq $SQLAzure)) {

		$Params = "$global:DBManagerString -C -D$global:AggDB -OTeleoptiCCCAgg -F$DatabasePath"
		$Prms = $Params.Split(" ")
		& "$DbManagerExe" $Prms
    }
}

function global:RevokeServerRoles () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

		$ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
		$query = "$global:RevokeSrvRoleCmd"

		Log "Revoking: [$global:InstallAndPatchSqlLogin] as Admin on: [$global:SQLServerInstance]"
		RunAndRetryNonQuery $ConnectionString $query
    }
}

function global:PatchDatabaseWithDboOnly () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

		$Params = "$global:DBManagerString -D$MartDB -OTeleoptiAnalytics -F$DatabasePath"
		$Prms = $Params.Split(" ")
		& "$DbManagerExe" $Prms
       
		$Params = "$global:DBManagerString -D$AppDB -OTeleoptiCCC7 -F$DatabasePath"
		$Prms = $Params.Split(" ")
		& "$DbManagerExe" $Prms
        
		$Params = "$global:DBManagerString -D$global:AggDB -OTeleoptiCCCAgg -F$DatabasePath"
		$Prms = $Params.Split(" ")
		& "$DbManagerExe" $Prms
    }
}

function global:DataModifications () {

    $Params = "$global:SecurityExeString -AP$AppDB -AN$MartDB -CD$global:AggDB"
    $Prms = $Params.Split(" ")
    & "$SecurityExe" $Prms
}

function global:ScriptedTestsRunOnAllDBs () {
    
    if (!($global:SQLEdition -eq $SQLAzure)) {

		$ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=$MartDB;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
		$query = Get-Content "$WorkingDirectory\..\Database\Tools\PK-namingStandard.sql"
    
		Log "Running script: [PK-namingStandard.sql] on [$MartDB]" 
		RunAndRetryNonQuery $ConnectionString $query

		$ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=$AppDB;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
		$query = Get-Content "$WorkingDirectory\..\Database\Tools\NoHeapsCheck.sql"
    
		Log "Running script: [NoHeapsCheck.sql] on [$AppDB]"
		RunAndRetryNonQuery $ConnectionString $query
    }
}

function global:AnalyticsReportsTest () {
    
    $path = "$WorkingDirectory\..\Database\Tools\AnalyticsReportsCompile.sql"
    $word = "BEGIN TRY"
    $replacement = ""
    $text = get-content $path 
    $newText = $text -replace $word,$replacement
    $newText > $path
    
    $query = Get-Content "$WorkingDirectory\..\Database\Tools\AnalyticsReportsCompile.sql"
    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=$MartDB;User Id=$global:ApplicationDbLogin;Password=$global:ApplicationDbPwd"
    
    Log "Running script: [AnalyticsReportsCompile.sql] on [$MartDB]"
    RunAndRetryNonQuery $ConnectionString $query
}

function global:CleanUpLog () {

    Get-ChildItem $WorkingDirectory\..\ -Filter DBManager*.log -Recurse | Remove-Item | Out-null

    Log "Cleaning DBManager*.log files from $WorkingDirectory\*"
}   

function Log
{
	param($message)
	$timeStampFormat = "yyyy-MM-dd HH:mm:ss"
	Write-Output "$(Get-Date -f $timeStampFormat) - $message"
} 
