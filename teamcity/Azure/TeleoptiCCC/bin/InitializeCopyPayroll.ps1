$path = Get-Location
$scriptName = $MyInvocation.MyCommand.Name
$scriptLog = "$path\$scriptName.log"

$admin = 'ScheduleTaskAdmin'
$taskName = 'CopyPayrollDll'
#$taskName2 = 'ClickOnceSign'
$FailedTaskSetup = "This means that customized Payroll Exports will not function at all and that is a CRITICAL issue."

function New-RandomPassword() {
    [CmdletBinding()]
    param(
        [int]$Length = 6
    )
    $ascii=$NULL;For ($a=33;$a –le 126;$a++) {$ascii+=,[char][byte]$a }
    For ($loop=1; $loop –le $length; $loop++) {
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
    $SecurePassword = ConvertTo-SecureString "$pwd" –asplaintext –force 

    Write-Output "Creating temporary user '$admin' with password $pwd" | out-file $scriptLog -Append
    New-LocalUser -Name $admin -Password $SecurePassword -FullName "Payroll Automation" -Description "Payroll" | out-null

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
   
    #Schedule task every 20 minutes
    $setupTask = schtasks /create /sc minute /mo 20 /tn "$taskName" /f /ru "$admin" /rp "$pwd" /tr "$path\CopyPayrollDll.cmd" 2>&1 #| Out-File $scriptLog -Append
    $setupTask | out-file $scriptLog -Append
    
    if ($lastexitcode -ne 0) {
        
        New-EventLog -LogName Application -Source "InitilizeCopyPayroll" -ErrorAction SilentlyContinue
        Write-EventLog -LogName Application -Source "InitilizeCopyPayroll" -EntryType Error -EventId 666 -Message "$setupTask `n$FailedTaskSetup"
        Write-Output $setupTask   
    }
    
    #Run task for the first time
    schtasks /RUN /TN "$taskName" | Out-File $scriptLog
}
Catch
{
    Write-Error $_.Exception
    Throw $_.Exception
}
