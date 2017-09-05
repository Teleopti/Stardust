$integrated = "false"

if($args[0] -eq "1")
{
    $integrated = "true"
}
else
{
    $PATCHUSER=$args[3]
    $PATCHPWD=$args[4]
}
$SQLServer=$args[1]
$CCC7DB=$args[2]

#log-info "Check so one tenant points to itself"

    $CCC7DB = $CCC7DB.Trim()
    $checkConn = "-TSServer=$SQLServer;Database=$CCC7DB;UID=$PATCHUSER;Password=$PATCHPWD;Trusted_Connection=$integrated"

    $command = $PSScriptRoot + "\..\Enrypted\Teleopti.Support.Security.exe"
        &"$command" "-CT1" $checkConn


#log-info "Get databases to patch"
     # Create SqlConnection object and define connection string
    $con = New-Object System.Data.SqlClient.SqlConnection
    $con.ConnectionString = "Server=$SQLServer;Database=$CCC7DB;User ID=$PATCHUSER;Password=$PATCHPWD;Integrated Security=$integrated"
    $con.open()
   # log-info "opened " $con.ConnectionString 

    # Create SqlCommand object, define command text, and set the connection
    $cmd = New-Object System.Data.SqlClient.SqlCommand
    $cmd.CommandText = "SELECT ApplicationConnectionString, AnalyticsConnectionString, AggregationConnectionString FROM Tenant.Tenant"
    $cmd.Connection = $con

    # Create SqlDataReader
    $dr = $cmd.ExecuteReader()
    
    $SECSQLServer="-DS"+$SQLServer
    if($integrated  -eq "true")
    {
        $SECPATCHUSER = "-EE"
    }
    else
    {    
        $SECPATCHUSER = "-DU"+$PATCHUSER
        $SECPATCHPWD= "-DP"+$PATCHPWD
    }

    $SQLServer="-S"+$SQLServer
    if($integrated  -eq "true")
    {
        $PATCHUSER = "-E"
    }
    else
    {
        $PATCHUSER = "-U"+$PATCHUSER
        $PATCHPWD= "-P"+$PATCHPWD
    }
    

    If ($dr.HasRows)
    {
        #log-info "Found databases"
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
        $array = ($dr["AggregationConnectionString"] -split ";")
        for ($i=0; $i -lt $array.length; $i++) {
	        if ($array[$i] -like '*Initial Catalog*') { 
                $cat = ($array[$i] -split "=")
                $aggDb = $cat[1]
            }
        }
        if($appDb.Trim() -ne $CCC7DB.Trim()) #don't patch the first database again
        {
            $SECappDb = "-AP"+$appDb
            $SECanalDb =  "-AN"+$analDb
            $SECloggDb =  "-CD"+$aggDb
            $colon = ":"
            $SQLUserPwd = "-L$SQLUser$colon$SQLPwd"
            if($SQLUserPwd -eq "-L:")
            {
                $SQLUserPwd = "-W" + [Environment]::UserDomainName + "\" + [Environment]::UserName
            }
            $CONNSTRINGBASE="Data Source=$SQLServer;User Id=$SQLUser;Password=$SQLPwd"

            #log-info "Patch databases: $appDb $analDb."

            $appDb = "-D"+$appDb
            $analDb =  "-D"+$analDb
            $aggDb =  "-D"+$aggDb
        
            $command = $PSScriptRoot + "\..\DBManager.exe"
        
            &"$command" $SQLServer $appDb "-OTeleoptiCCC7" $PATCHUSER $PATCHPWD "-T" "-R" $SQLUserPwd
            &"$command" $SQLServer $analDb "-OTeleoptiAnalytics" $PATCHUSER $PATCHPWD "-T" "-R" $SQLUserPwd
            &"$command" $SQLServer $aggDb "-OTeleoptiCCCAgg" $PATCHUSER $PATCHPWD "-T" "-R" $SQLUserPwd
        
            $command = $PSScriptRoot + "\..\Enrypted\Teleopti.Support.Security.exe"
            &"$command" $SECSQLServer $SECappDb $SECanalDb $SECloggDb $CONNSTRINGBASE $SECPATCHUSER $SECPATCHPWD
        }
        
      }
    }
# Close the data reader and the connection
$dr.Close()
$con.Close()

#$body = @{
#    "Tenant" = ""
#    "AdminUserName" = $user
#    "AdminPassword" = $pwd
#    "UseIntegratedSecurity" = $integrated
#}

#write $integrated
#Invoke-RestMethod -Method Post $url -Body ($body) -timeout 300

