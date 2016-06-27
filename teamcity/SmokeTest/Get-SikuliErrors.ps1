# Used in Sikuli smoke
# Search application eventlog for Teleopti Errors last hour
$servername = $Env:ServerName
$username = 'toptinet\tfsintegration'
$password = 'm8kemew0rk'
$secstr = New-Object -TypeName System.Security.SecureString
$password.ToCharArray() | ForEach-Object {$secstr.AppendChar($_)}
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $username, $secstr

$command = {Get-EventLog -LogName Application -After (Get-Date).AddHours("-1") |
Where-Object {$_.EntryType -like 'Error' -and $_.Source -like 'Teleopti*' -and $_.Message -notlike "*product activation*"}}

$Eventlog = Invoke-Command -ComputerName $servername -ScriptBlock $command -Credential $cred | ft -autosize

    If ($Eventlog) 
    {
        Write-Output $Eventlog 
    }
  
    Else 
    {
        Write-Host "There is no Teleopti application errors found"
    }


# Search Teleopti.Support.Security.txt for FATAL errors
$TXTFile = 'C:\Program Files (x86)\Teleopti\DatabaseInstaller\Enrypted\Teleopti.Support.Security.txt'
Invoke-Command -computername $servername -scriptblock {Select-String -Path $args[0] -pattern FATAL} -Credential $cred -ArgumentList $TXTFile | ft -autosize


# Search Last 3 DBManagerLibrary*.Log for errors
$path = 'C:\Program Files (x86)\Teleopti\DatabaseInstaller'
Invoke-Command -ComputerName $servername -ScriptBlock {Get-ChildItem -Path $args[0] -Filter "DBManagerLibrary*.log" |  
    where-object { -not $_.PSIsContainer } |
	sort-object -Property $_.CreationTime |
	select-object -last 3 | 
    Select-String -Pattern 'error'} -Credential $cred -ArgumentList $path |
    ft -autosize |
    Write-Output