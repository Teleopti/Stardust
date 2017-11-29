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
    #CleanUpLog
	Write-Output "##teamcity[blockClosed name='<TeardownAndCleanup>']"
}

function global:CheckEditionAndVersion () 
{
    param (
    $query = ""

    )
    $ConnectionString="Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"

    Write-Output "$(Get-Date -f $timeStampFormat) - Checking SQL Version & Edition on [$global:SQLServerInstance]"		
	
    $CheckVersion = ExecuteSQLQuery $ConnectionString $query

    $global:SQLVersion = $CheckVersion.Column1

    $global:SQLVersionMajor = $global:sqlversion.split(".")[0]
    $global:SQLVersionMinor = $global:sqlversion.split(".")[1]
    $global:SQLVersion = $global:SQLVersionMajor + "." + $global:SQLVersionMinor
    
    $global:SQLEdition = $CheckVersion.Column2

    return $global:SQLVersion, $global:SQLEdition | out-null
}

function global:SettingsAndPreparation {
    
    if ($global:TestEdition -eq "SQL Azure") {
        
		Write-Output "TestEdition: SQL Azure selected..."
		
        $global:AdminSqlLogin = $AzureAdminSqlLogin
	    $global:AdminSqlPwd = $AzureAdminSqlPwd
    }

    $query = @("SELECT SERVERPROPERTY('productversion'), SERVERPROPERTY ('edition')")
    CheckEditionandVersion -query $query
    
    Write-Output "$(Get-Date -f $timeStampFormat) - ------------------------------------------------------------------"
    Write-Output "$(Get-Date -f $timeStampFormat) - SQL Version:                [$global:SQLVersion]"
    Write-Output "$(Get-Date -f $timeStampFormat) - SQL Edition:                [$global:SQLEdition]"
    Write-Output "$(Get-Date -f $timeStampFormat) - AdminSqlLogin:              [$global:AdminSqlLogin]"
    Write-Output "$(Get-Date -f $timeStampFormat) - AdminSqlPwd:                [$global:AdminSqlPwd]"
    Write-Output "$(Get-Date -f $timeStampFormat) - InstallAndPatchSqlLogin:    [$global:InstallAndPatchSqlLogin]"
    Write-Output "$(Get-Date -f $timeStampFormat) - InstallAndPatchSqlPwd:      [$global:InstallAndPatchSqlPwd]"
    Write-Output "$(Get-Date -f $timeStampFormat) - ApplicationDbLogin:         [$global:ApplicationDbLogin]"
    Write-Output "$(Get-Date -f $timeStampFormat) - ApplicationDbPwd:           [$global:ApplicationDbPwd]"
    Write-Output "$(Get-Date -f $timeStampFormat) - ------------------------------------------------------------------"

    # Check that SQL Edition is correct
    if (!($global:SQLEdition -eq $TestEdition)) { 
    
    Write-Output "Expected SQLEdition: [$TestEdition], but was: [$global:SQLEdition]" 
    exit 1
    }
            
        if ($global:SQLEdition -eq $SQLAzure) { 
    
    
        $global:InstallAndPatchSqlLogin = $AzureAdminSqlLogin
        $global:InstallAndPatchSqlPwd = $AzureAdminSqlPwd
        $global:AggDB = "$DbNamePrefix"+"_MartDB"

        }

        elseif (!($SQLEdition -eq $SQLAzure)) {

        $global:Query_CreateInstallAndPatchSqlLoginCommand = "CREATE LOGIN [$global:InstallAndPatchSqlLogin] WITH PASSWORD=N'$InstallAndPatchSqlPwd', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF"
        $global:AggDB = "$DbNamePrefix"+"_AggDB"
        $global:Query_DropDatabasesCommand = "ALTER DATABASE [$AppDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$AppDB];ALTER DATABASE [$MartDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$MartDB];ALTER DATABASE [$global:AggDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$global:AggDB]"
        $global:Query_RevokeServerRolesCommand = "ALTER SERVER ROLE [dbcreator] DROP MEMBER [$global:InstallAndPatchSqlLogin];ALTER SERVER ROLE [securityadmin] DROP MEMBER [$global:InstallAndPatchSqlLogin]"
        $global:Query_GrantServerRolesCommand = "ALTER SERVER ROLE [dbcreator] ADD MEMBER [$global:InstallAndPatchSqlLogin];ALTER SERVER ROLE [securityadmin] ADD MEMBER [$global:InstallAndPatchSqlLogin]"
        
        }

        $global:DBManagerString = "-S$global:SQLServerInstance -U$global:InstallAndPatchSqlLogin -P$global:InstallAndPatchSqlPwd -T -L$global:ApplicationDbLogin`:$global:ApplicationDbPwd"
        $global:SecurityExeString = "-DS$global:SQLServerInstance -DU$global:InstallAndPatchSqlLogin -DP$global:InstallAndPatchSqlPwd"
        
        #Check that SQL version is correct
        if (!($TestMajorMinor -eq $global:SQLVersion)) { 
    
        Write-Output "Expected SQL Version: [$global:TestMajorMinor], but was: [$global:SQLVersion]" 
        exit 1
        }

        elseif ($global:TestMajorMinor -eq $SQL2008R2) {

        $global:Query_GrantServerRolesCommand = "EXEC master..sp_addsrvrolemember @loginame = N'$global:InstallAndPatchSqlLogin', @rolename = N'dbcreator';EXEC master..sp_addsrvrolemember @loginame = N'$global:InstallAndPatchSqlLogin', @rolename = N'securityadmin'"
        $global:Query_RevokeServerRolesCommand = "EXEC master..sp_dropsrvrolemember @loginame = N'$global:InstallAndPatchSqlLogin', @rolename = N'dbcreator';EXEC master..sp_dropsrvrolemember @loginame = N'$global:InstallAndPatchSqlLogin', @rolename = N'securityadmin'"
       	
        }
}

function global:DropDatabase () {
   
    if (!($global:SQLEdition -eq $SQLAzure)) {

    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    
    # Check if Database exists
    $query = "SELECT COUNT(*) FROM sys.sysdatabases where name='$AppDB'"
    $count = RunAndRetryScalar $ConnectionString $query
    $count = $count[$count.Count - 1]

        if ($count -gt 0) {

        $global:Query_DropDatabasesCommand = "ALTER DATABASE [$AppDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$AppDB];ALTER DATABASE [$MartDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$MartDB];ALTER DATABASE [$global:AggDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$global:AggDB]"

        Write-OutPut "$(Get-Date -f $timeStampFormat) - Dropping Databases: [$AppDB] & [$MartDB] & [$global:AggDB] on $global:SQLServerInstance"
        RunAndRetryNonQuery $ConnectionString $global:Query_DropDatabasesCommand

        } 
    }

    elseif ($global:SQLEdition -eq $SQLAzure) {

    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    $Query_DropAzureDB_Mart = "DROP DATABASE $MartDB"
    $Query_DropAzureDB_App = "DROP DATABASE $AppDB"

    # Check if Database exists
    $query = "SELECT COUNT(*) FROM sys.sysdatabases where name='$AppDB'"
    $count = RunAndRetryScalar $ConnectionString $query
    $count = $count[$count.Count - 1]

        if ($count -gt 0) {

        Write-Output "$(Get-Date -f $timeStampFormat) - Dropping Database: $AppDB on $SQLAzure"
        RunAndRetryNonQuery $ConnectionString $Query_DropAzureDB_App        
        Write-OutPut "$(Get-Date -f $timeStampFormat) - Dropping Database: $MartDB on $SQLAzure"
        RunAndRetryNonQuery $ConnectionString $Query_DropAzureDB_Mart

        } 
    }
}

function global:CreateInstallAndPatchSqlLoginCommandTarget () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    $query_droplogin = "$global:DropInstallAndPatchSqlLoginCommand"
    $query_createlogin = "$global:Query_CreateInstallAndPatchSqlLoginCommand"

    $Login = Test-SQLLogin -SqlLogin "$global:InstallAndPatchSqlLogin"

        if ($Login) {
       
        Write-Output "$(Get-Date -f $timeStampFormat) - Dropping login: [$global:InstallAndPatchSqlLogin] on [$global:SQLServerInstance]"
        RunAndRetryNonQuery $ConnectionString $query_droplogin
           
        }

        else {
            
        Write-Output "$(Get-Date -f $timeStampFormat) - Create login: [$global:InstallAndPatchSqlLogin] on [$global:SQLServerInstance]"
        RunAndRetryNonQuery $ConnectionString $query_createlogin

        }

    }
}

function global:DropPatchLogin () {

    if (!($global:SQLEdition -eq $SQLAzure)) {
        
        $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
        $query_droplogin = "$global:DropInstallAndPatchSqlLoginCommand"

        Write-Output "$(Get-Date -f $timeStampFormat) - Dropping login: [$global:InstallAndPatchSqlLogin] on [$global:SQLServerInstance]"
        ExecuteSqlQuery $ConnectionString $query_droplogin
    }
}

function global:DropApplicationLogin () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

        $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
        $query = "$global:DropApplicationDbLoginCommand"

        Write-Output "$(Get-Date -f $timeStampFormat) - Dropping Application login: [$global:ApplicationDbLogin] on [$global:SQLServerInstance] "
        ExecuteSqlQuery $ConnectionString $query
    }
}   

function global:GrantServerRoles () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    $query = "$global:Query_GrantServerRolesCommand"

    Write-Output "$(Get-Date -f $timeStampFormat) - Adding: [$global:InstallAndPatchSqlLogin] as Admin on: [$global:SQLServerInstance]"
    RunAndRetryNonQuery $ConnectionString $query

    }
}

function global:CreateDatabaseWithSQLAdmin () {
    
    $Params = "$global:DBManagerString -C -D$MartDB -OTeleoptiAnalytics -F$DatabasePath"
    $Prms = $Params.Split(" ")
    & "$DbManagerExe" $Prms
	    
		if ($lastexitecode -ne 0) {
        Write-Host "Something went wrong during creation of: '$MartDB'"
        exit 1
        }

    $Params = "$global:DBManagerString -C -D$AppDB -OTeleoptiCCC7 -F$DatabasePath"
    $Prms = $Params.Split(" ")
    & "$DbManagerExe" $Prms
	    
		if ($lastexitecode -ne 0) {
            Write-Host "Something went wrong during creation of: '$AppDB'"
            exit 1
        }

    if (!($SQLEdition -eq $SQLAzure)) {

    $Params = "$global:DBManagerString -C -D$global:AggDB -OTeleoptiCCCAgg -F$DatabasePath"
    $Prms = $Params.Split(" ")
    & "$DbManagerExe" $Prms
		
		if ($lastexitecode -ne 0) {
            Write-Host "Something went wrong during creation of: '$global:AggDB'"
            exit 1
        }
    }
}

function global:RevokeServerRoles () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=master;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    $query = "$global:Query_RevokeServerRolesCommand"

    Write-Output "$(Get-Date -f $timeStampFormat) - Revoking: [$global:InstallAndPatchSqlLogin] as Admin on: [$global:SQLServerInstance]"
    RunAndRetryNonQuery $ConnectionString $query

    }
}

function global:PatchDatabaseWithDboOnly () {

    if (!($global:SQLEdition -eq $SQLAzure)) {

    $Params = "$global:DBManagerString -D$MartDB -OTeleoptiAnalytics -F$DatabasePath"
    $Prms = $Params.Split(" ")
    & "$DbManagerExe" $Prms
		
		if ($lastexitecode -ne 0) {
        Write-Host "Something went wrong during creation of: '$MartDB'"
        exit 1
        }
       
    $Params = "$global:DBManagerString -D$AppDB -OTeleoptiCCC7 -F$DatabasePath"
    $Prms = $Params.Split(" ")
    & "$DbManagerExe" $Prms

		if ($lastexitecode -ne 0) {
            Write-Host "Something went wrong during creation of: '$AppDB'"
            exit 1
        }
        
    $Params = "$global:DBManagerString -D$global:AggDB -OTeleoptiCCCAgg -F$DatabasePath"
    $Prms = $Params.Split(" ")
    & "$DbManagerExe" $Prms
		
		if ($lastexitecode -ne 0) {
            Write-Host "Something went wrong during creation of: '$global:AggDB'"
            exit 1
        }
    }
}

function global:DataModifications () {

    $Params = "$global:SecurityExeString -AP$AppDB -AN$MartDB -CD$global:AggDB"
    $Prms = $Params.Split(" ")
    & "$SecurityExe" $Prms
	
	if ($lastexitecode -ne 0) {
        Write-Host "Something went wrong during the running of security EXE on : '$AppDB'"
        exit 1
    }
}

function global:ScriptedTestsRunOnAllDBs () {
    
    if (!($global:SQLEdition -eq $SQLAzure)) {

    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=$MartDB;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    $query = Get-Content "$WorkingDirectory\..\Database\Tools\PK-namingStandard.sql"
    
    Write-Output "$(Get-Date -f $timeStampFormat) - Running script: [PK-namingStandard.sql] on [$MartDB]" 
    RunAndRetryNonQuery $ConnectionString $query

    $ConnectionString = "Data Source=$global:SQLServerInstance;Initial Catalog=$AppDB;User Id=$global:AdminSqlLogin;Password=$global:AdminSqlPwd"
    $query = Get-Content "$WorkingDirectory\..\Database\Tools\NoHeapsCheck.sql"
    
    Write-Output "$(Get-Date -f $timeStampFormat) - Running script: [NoHeapsCheck.sql] on [$AppDB]"
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
    
    Write-Output "$(Get-Date -f $timeStampFormat) - Running script: [AnalyticsReportsCompile.sql] on [$MartDB]"
    RunAndRetryNonQuery $ConnectionString $query
}

function global:CleanUpLog () {

    Get-ChildItem $WorkingDirectory\..\ -Filter DBManager*.log -Recurse | Remove-Item | Out-null

    Write-Output "$(Get-Date -f $timeStampFormat) - Cleaning DBManager*.log files from $WorkingDirectory\*"
}    
