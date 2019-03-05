function New-RandomPassword {

    [CmdletBinding(DefaultParameterSetName='FixedLength',ConfirmImpact='None')]
    [OutputType([String])]
    Param
    (
        # Specifies minimum password length
        [Parameter(Mandatory=$false,
                   ParameterSetName='RandomLength')]
        [ValidateScript({$_ -gt 0})]
        [Alias('Min')] 
        [int]$MinPasswordLength = 8,
        
        # Specifies maximum password length
        [Parameter(Mandatory=$false,
                   ParameterSetName='RandomLength')]
        [ValidateScript({
                if($_ -ge $MinPasswordLength){$true}
                else{Throw 'Max value cannot be lesser than min value.'}})]
        [Alias('Max')]
        [int]$MaxPasswordLength = 12,

        # Specifies a fixed password length
        [Parameter(Mandatory=$false,
                   ParameterSetName='FixedLength')]
        [ValidateRange(1,2147483647)]
        [int]$PasswordLength = 8,
        
        # Specifies an array of strings containing charactergroups from which the password will be generated.
        # At least one char from each group (string) will be used.
        [String[]]$InputStrings = @('abcdefghijkmnpqrstuvwxyz', 'ABCEFGHJKLMNPQRSTUVWXYZ', '23456789', '!#%'),

        # Specifies a string containing a character group from which the first character in the password will be generated.
        # Useful for systems which requires first char in password to be alphabetic.
        [String] $FirstChar,
        
        # Specifies number of passwords to generate.
        [ValidateRange(1,2147483647)]
        [int]$Count = 1
    )
    Begin {
        Function Get-Seed{
            # Generate a seed for randomization
            $RandomBytes = New-Object -TypeName 'System.Byte[]' 4
            $Random = New-Object -TypeName 'System.Security.Cryptography.RNGCryptoServiceProvider'
            $Random.GetBytes($RandomBytes)
            [BitConverter]::ToUInt32($RandomBytes, 0)
        }
    }
    Process {
        For($iteration = 1;$iteration -le $Count; $iteration++){
            $Password = @{}
            # Create char arrays containing groups of possible chars
            [char[][]]$CharGroups = $InputStrings

            # Create char array containing all chars
            $AllChars = $CharGroups | ForEach-Object {[Char[]]$_}

            # Set password length
            if($PSCmdlet.ParameterSetName -eq 'RandomLength')
            {
                if($MinPasswordLength -eq $MaxPasswordLength) {
                    # If password length is set, use set length
                    $PasswordLength = $MinPasswordLength
                }
                else {
                    # Otherwise randomize password length
                    $PasswordLength = ((Get-Seed) % ($MaxPasswordLength + 1 - $MinPasswordLength)) + $MinPasswordLength
                }
            }

            # If FirstChar is defined, randomize first char in password from that string.
            if($PSBoundParameters.ContainsKey('FirstChar')){
                $Password.Add(0,$FirstChar[((Get-Seed) % $FirstChar.Length)])
            }
            # Randomize one char from each group
            Foreach($Group in $CharGroups) {
                if($Password.Count -lt $PasswordLength) {
                    $Index = Get-Seed
                    While ($Password.ContainsKey($Index)){
                        $Index = Get-Seed                        
                    }
                    $Password.Add($Index,$Group[((Get-Seed) % $Group.Count)])
                }
            }

            # Fill out with chars from $AllChars
            for($i=$Password.Count;$i -lt $PasswordLength;$i++) {
                $Index = Get-Seed
                While ($Password.ContainsKey($Index)){
                    $Index = Get-Seed                        
                }
                $Password.Add($Index,$AllChars[((Get-Seed) % $AllChars.Count)])
            }
            Write-Output -InputObject $(-join ($Password.GetEnumerator() | Sort-Object -Property Name | Select-Object -ExpandProperty Value))
        }
    }
}

Try 
{
    #Get local path
	$path = Get-Location
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
	
	$admin = 'ServiceStarterAdmin'
	$taskName = 'ServiceStarter'
	$FailedTaskSetup = "This means that task scheduler ServiceStarter failed..."
	
    $userExits = Get-localuser -Name $admin -ErrorAction SilentlyContinue

    if ($userExits) {

        Remove-LocalUser -Name $admin
    }

    $pwd = New-RandomPassword
    $SecurePassword = ConvertTo-SecureString "$pwd" -asplaintext -force 

    log-info "Creating temporary user '$admin'"
    New-LocalUser -Name $admin -Password $SecurePassword -FullName "Service Starter" -Description "ServiceStarter"

    log-info "Adding user '$admin' to 'Administrators' group..."
    Add-LocalGroupMember -Group "Administrators" -Member "$admin"
    
	log-info "Starting task scheduler..."
    Start-Service "task scheduler" 

    $taskExists = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

        if ($taskExists) {
        
            log-info "Removing Scheduled task '$taskName'..."
            Unregister-ScheduledTask -TaskName $taskName -Confirm:$false 
        }

 <#   $taskExists = Get-ScheduledTask -TaskName $taskName2 -ErrorAction SilentlyContinue

        if ($taskExists) {
        
            Write-Host "Removing Scheduled task '$taskName2'..."
            Unregister-ScheduledTask -TaskName $taskName2 -Confirm:$false | out-file $scriptLog -Append
        }
 #>
   
    #Set Scheduled task to start services once in 2 minutes
	$Time = Get-Date 
	$newTime = $Time.AddMinutes(2)
	$setTime = $newTime.tostring("HH:mm:ss")

	$setupTask = schtasks /create /sc once /st "$setTime" /tn "$taskName" /f /ru "$admin" /rp "$pwd" /tr "$path\ServiceStarter.cmd"

    log-info $setupTask 
    
    if ($lastexitcode -ne 0) {
        
        New-EventLog -LogName Application -Source "InitializeServiceStarter" -ErrorAction SilentlyContinue
        Write-EventLog -LogName Application -Source "InitializeServiceStarter" -EntryType Error -EventId 666 -Message "$setupTask `n$FailedTaskSetup"
        log-info $setupTask
		log-error $setupTask
    }
    
	log-info "ServiceStarter Scheduler task completed!"

}
Catch
{
    log-error "$_.Exception"
	log-info "$_.Exception"
    Throw $_.Exception
}
