param($SqlServer, $AppDatabase, $AnalyticsDatabase, $seedAppDatabase, $seedAnalyticsDatabase)

function Wait-until-Done
{ 
    param ($conStr, $AppDatabase, $AnalyticsDatabase) 
    $rowCount = $null

    while ($rowCount -ne 0)
    {
        $ds = ExecQuery -conStr $conStr -cmdText "SELECT count(*) 'InOperation' FROM sys.dm_operation_status WHERE (major_resource_id = '$AppDatabase' OR major_resource_id = '$AnalyticsDatabase') AND state_desc = 'IN_PROGRESS'"
        $rowCount = $ds.Tables[0].Rows[0].InOperation
        sleep -Seconds 5
    }
}

function ExecQuery 
{ 
    param ($conStr, $cmdText) 
    if (!$conStr -or !$cmdText) 
    { 
        write-Host "ExecQuery function called with no connection string and/or command text." 
    } 
    else 
    { 
        $Connection = New-Object System.Data.SQLClient.SQLConnection 
        $Connection.ConnectionString = $conStr 
        try 
        { 
            $Connection.Open() 
            $Command = New-Object System.Data.SQLClient.SQLCommand 
            $Command.Connection = $Connection 
            $Command.CommandText = $cmdText
             
            $adapter = New-Object System.Data.sqlclient.sqlDataAdapter $command
            $dataset = New-Object System.Data.DataSet
            $adapter.Fill($dataSet) | Out-Null

            $connection.Close()
            return $dataSet
        } 

        finally { 
            if ($Connection.State -eq "Open") 
            { 
                $Connection.Close() 
            } 
        } 
    } 
}

function ExecNonQuery 
{ 
    param ($conStr, $cmdText) 
    if (!$conStr -or !$cmdText) 
    { 
        write-Host "ExecNonQuery function called with no connection string and/or command text." 
    } 
    else 
    { 
        $Connection = New-Object System.Data.SQLClient.SQLConnection 
        $Connection.ConnectionString = $conStr 
        try 
        { 
            $Connection.Open() 
            $Command = New-Object System.Data.SQLClient.SQLCommand 
            $Command.Connection = $Connection 
            $Command.CommandText = $cmdText
            $Command.CommandTimeout = 120 #Create database takes while
             
            write-Host "Executing SQL Command: $cmdText" 
            $Command.ExecuteNonQuery() 
        } 

        finally { 
            if ($Connection.State -eq "Open") 
            { 
               $Connection.Close() 
            } 
        } 
    } 
}

##=============
##main
##=============
[string]$global:scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

$password = Read-Host 'What is your Teleopti sysAdmin password?' -AsSecureString
$password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
$connenctionString = "Data Source=tcp:$SqlServer;Initial Catalog=master;User Id=Teleopti;Password=$password;Current Language=us_english;Encrypt=True;trustServerCertificate=false"

$ShortSQLServer = $SqlServer.Replace(".database.windows.net","")

##drop databases
$cmdText = "DROP DATABASE $AppDatabase"
ExecNonQuery -conStr $connenctionString -cmdText $cmdText
$cmdText = "DROP DATABASE $AnalyticsDatabase"
ExecNonQuery -conStr $connenctionString -cmdText $cmdText

##wait for drop to finish
Wait-until-Done -conStr $connenctionString -AppDatabase $AppDatabase -AnalyticsDatabase $AnalyticsDatabase

##create databases
$cmdText = "CREATE DATABASE $AppDatabase AS COPY OF $seedAppDatabase"
ExecNonQuery -conStr $connenctionString -cmdText $cmdText
$cmdText = "CREATE DATABASE $AnalyticsDatabase AS COPY OF $seedAnalyticsDatabase"
ExecNonQuery -conStr $connenctionString -cmdText $cmdText

##wait for create to finish
Wait-until-Done -conStr $connenctionString -AppDatabase $AppDatabase -AnalyticsDatabase $AnalyticsDatabase

##Add databases to failover group - still doesn't work!

<#
$databases[System.Collections.Generic.List]
$databases = @{$AppDatabase,$AnalyticsDatabase}

$failoverGroup = Get-AzureRmSqlDatabaseFailoverGroup -ResourceGroupName "Default-SQL-WestEurope" -ServerName "$ShortSQLServer" -FailoverGroupName "fgteleoptirnd"
##PS C:\> $databases = Get-AzureRmSqlElasticPoolDatabase -ResourceGroupName rg -ServerName primaryserver -ElasticPoolName pool1
$failoverGroup = $failoverGroup | Add-AzureRmSqlDatabaseToFailoverGroup -Database $AppDatabase


$failovergroupDatabase = Get-AzureRmSqlDatabase -ResourceGroupName "Default-SQL-WestEurope" -ServerName "$ShortSQLServer" -DatabaseName $AnalyticsDatabase
$failovergroupDatabase | Add-AzureRmSqlDatabaseToFailoverGroup -ResourceGroupName "Default-SQL-WestEurope" -ServerName "$ShortSQLServer" -FailoverGroupName "fgteleoptirnd"

#>