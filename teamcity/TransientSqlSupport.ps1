$path = (get-item $PSScriptroot ).parent.Fullname
$NugetPath = "$WorkingDirectory\..\packages"
$NugetExe = "$WorkingDirectory.\\..\.nuget\nuget.exe"

. $NugetExe install EnterpriseLibrary.TransientFaultHandling.Configuration -o $NugetPath -Source "https://www.nuget.org/api/v2/" -ExcludeVersion

# Loading EnterpriseLibrary TransientFaultHandling Dll's
Add-Type -Path "$NugetPath\EnterpriseLibrary.TransientFaultHandling\lib\portable-net45+win+wp8\Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.dll"
Add-Type -Path "$NugetPath\EnterpriseLibrary.Common\lib\NET45\Microsoft.Practices.EnterpriseLibrary.Common.dll"
Add-Type -Path "$NugetPath\EnterpriseLibrary.TransientFaultHandling.Caching\lib\NET45\Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Caching.dll"
Add-Type -Path "$NugetPath\EnterpriseLibrary.TransientFaultHandling.Configuration\lib\NET45\Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration.dll"
Add-Type -Path "$NugetPath\EnterpriseLibrary.TransientFaultHandling.Data\lib\NET45\Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Data.dll"
Add-Type -Path "$NugetPath\EnterpriseLibrary.TransientFaultHandling.ServiceBus\lib\NET45\Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ServiceBus.dll"
Add-Type -Path "$NugetPath\EnterpriseLibrary.TransientFaultHandling.WindowsAzure.Storage\lib\NET45\Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.WindowsAzure.Storage.dll"

$timeStampFormat = "g"
$CommandTimeout = 60 * 180

function global:RunAndRetryNonQuery
{
    param (
        
        $connectionString,
		$query
    )

	Write-Output "$(Get-Date -f $timeStampFormat) - Running (retrying if needed) query"
	$retryPolicy = [Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy]::DefaultFixed
    $arguments = "$connectionString",$retryPolicy,$retryPolicy
    $connection = New-Object -TypeName Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection -ArgumentList $arguments

	$connection.Open() | Out-Null
	$command = $connection.CreateCommand()
	$command.CommandTimeout = $CommandTimeout
	$command.CommandText = $query
   	$command.ExecuteNonQuery() 
    return $returnValue
    $connection.Close()
	Write-Output "$(Get-Date -f $timeStampFormat) - Running query... Done."
}

function RunAndRetrySqlQuery
{
    Param
    (
        $ConnectionString,
        $Query
    )
        
    $Datatable = New-Object System.Data.DataTable
    
    $retryPolicy = [Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy]::DefaultFixed
    $arguments = "$ConnectionString",$retryPolicy,$retryPolicy
    $connection = New-Object -TypeName Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection -ArgumentList $arguments

	$connection.Open() | Out-Null
	$command = $connection.CreateCommand()
	$command.CommandTimeout = $CommandTimeout
	$command.CommandText = $query
    $Reader = $Command.ExecuteReader()
    $Datatable.Load($Reader)
    $Connection.Close()    

    return $Datatable
}

function RunSQLQuery
{
    Param
    (
        $ConnectionString,
        $Query
    )

$conn = New-Object System.Data.SqlClient.SqlConnection "$ConnectionString"; 
$conn.Open();

## Attach the InfoMessage Event Handler to the connection to write out the messages 
$handler = [System.Data.SqlClient.SqlInfoMessageEventHandler] {param($sender, $event) Write-Host $event.Message }; 
$conn.add_InfoMessage($handler); 
$conn.FireInfoMessageEventOnUserErrors = $true;
$cmd = $conn.CreateCommand(); 

$cmd.CommandText = $query;  
$res = $cmd.ExecuteNonQuery(); 
$conn.Close();

return $res

}
function global:RunAndRetryScalar {
    
    param (
       
        $connectionString,
		$query
    )

	Write-Output "$(Get-Date -f $timeStampFormat) - Running (retrying if needed) query"
	$retryPolicy = [Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy]::DefaultFixed
    $arguments = "$connectionString",$retryPolicy,$retryPolicy
    $connection = New-Object -TypeName Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.ReliableSqlConnection -ArgumentList $arguments

	$connection.Open() | Out-Null
	$command = $connection.CreateCommand()
	$command.CommandTimeout = $CommandTimeout
	$command.CommandText = $query
	$returnValue = $command.ExecuteScalar() 
	$connection.Close()
	return $returnValue
	Write-Output "$(Get-Date -f $timeStampFormat) - Running query... Done."
}

function global:SetDefaultRetryPolicy() {

    $defaultRetryStrategyName = "fixed"
    $retryCount = 20
    $retryInterval = New-TimeSpan -Seconds 5
    $retryIncrement = New-TimeSpan -Seconds 10
	
	Write-Output "$(Get-Date -f $timeStampFormat) - Setting default retry policy"
	Write-Output "Retry count: $retryCount"
	Write-Output "Retry interval: $retryInterval seconds"
	Write-Output "Retry increment: $retryIncrement"
    
    $strategy = New-Object -TypeName Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Incremental -ArgumentList $defaultRetryStrategyName, $retryCount, $retryInterval, $retryIncrement
    $strategies = New-Object System.Collections.Generic.List[Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryStrategy]
    $strategies.Add($strategy)
    $manager = New-Object -TypeName Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryManager -ArgumentList $strategies, $defaultRetryStrategyName
    [Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryManager]::SetDefault($manager, $false)
}

function global:ExecuteSqlQuery {
    
    Param (
        
        $ConnectionString,
        $Query
    )
        
    $Datatable = New-Object System.Data.DataTable
    
    $Connection = New-Object System.Data.SQLClient.SQLConnection
    $Connection.ConnectionString = $ConnectionString
    $Connection.Open()
    $Command = New-Object System.Data.SQLClient.SQLCommand
    $Command.Connection = $Connection
    $Command.CommandText = $Query
    $Reader = $Command.ExecuteReader()
    $Datatable.Load($Reader)
    $Connection.Close()    

    return $Datatable
}

Function Test-SQLLogin {
    param (
        [string]$SqlLogin
    )
    Add-Type -AssemblyName "Microsoft.SqlServer.Smo, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"
    $smo = New-Object Microsoft.SqlServer.Management.Smo.Server $global:SQLServerInstance
    $smo.ConnectionContext.LoginSecure=$false; 
    $smo.ConnectionContext.set_Login("$global:AdminSqlLogin"); 
    $smo.ConnectionContext.set_Password("$global:AdminSqlPwd") 
        
    if (($smo.logins).Name -contains $SqlLogin) {
        $true
        } 
        else {
        $false
        }
}  
