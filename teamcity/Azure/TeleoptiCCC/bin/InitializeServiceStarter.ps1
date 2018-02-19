$path = Get-Location
$scriptName = $MyInvocation.MyCommand.Name
$scriptLog = "$path\$scriptName.log"

$admin = 'ServiceStarterAdmin'
$taskName = 'ServiceStarter'
$FailedTaskSetup = "This means that task scheduler ServiceStarter failed..."

function New-RandomPassword() 
{
    [CmdletBinding()]
    param(
        [int]$Length = 9
    )
     $ascii=$NULL
     For ($a=48;$a -le 57;$a++) {$ascii+=,[char][byte]$a }
     For ($a=65;$a -le 90;$a++) {$ascii+=,[char][byte]$a }
     For ($a=97;$a -le 122;$a++) {$ascii+=,[char][byte]$a }
     For ($loop=1; $loop -le $length; $loop++) 
	{
		$RandomPassword+=($ascii | GET-RANDOM)
    }
    return $RandomPassword
}

Try 
{
    
    $userExits = Get-localuser -Name $admin -ErrorAction SilentlyContinue

    if ($userExits) {

        Remove-LocalUser -Name $admin | Out-null
    }

    $pwd = New-RandomPassword
    $SecurePassword = ConvertTo-SecureString "$pwd" -asplaintext -force 

    Write-Output "Creating temporary user '$admin' with password $pwd" | out-file $scriptLog -Append
    New-LocalUser -Name $admin -Password $SecurePassword -FullName "Service Starter" -Description "ServiceStarter" | out-null

    Write-Output "Adding user '$admin' to 'Administrators' group..." | out-file $scriptLog -Append
    Add-LocalGroupMember -Group "Administrators" -Member "$admin"
    

    Start-Service "task scheduler" | out-file $scriptLog -Append

    $taskExists = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

        if ($taskExists) {
        
            Write-Host "Removing Scheduled task '$taskName'..."
            Unregister-ScheduledTask -TaskName $taskName -Confirm:$false | out-file $scriptLog -Append
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

	$setupTask = schtasks /create /sc once /st "$setTime" /tn "$taskName" /f /ru "$admin" /rp "$pwd" /tr "$path\ServiceStarter.cmd" 2>&1 #| Out-File $scriptLog -Append

    $setupTask | out-file $scriptLog -Append
    
    if ($lastexitcode -ne 0) {
        
        New-EventLog -LogName Application -Source "InitializeServiceStarter" -ErrorAction SilentlyContinue
        Write-EventLog -LogName Application -Source "InitializeServiceStarter" -EntryType Error -EventId 666 -Message "$setupTask `n$FailedTaskSetup"
        Write-Output $setupTask   
    }
    
	Write-Host "ServiceStarter Scheduler task completed!"

}
Catch
{
    Write-Error $_.Exception
    Throw $_.Exception
}
