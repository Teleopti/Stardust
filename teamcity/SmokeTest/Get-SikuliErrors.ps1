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

$Eventlog = Invoke-Command -ComputerName $servername -ScriptBlock $command -Credential $cred | ft -Property EntryType, Source, Message -autosize

    If ($Eventlog) 
    {
        Write-Output 'Searching Eventlog/Application for Teleopti Errors:'
        Write-Output $Eventlog 
    }
  
    Else 
    {
        Write-Host "There are no Teleopti application errors found"
    }

   
# Search Teleopti.Support.Security.txt for FATAL errors
$TXTFile = 'C:\Program Files (x86)\Teleopti\DatabaseInstaller\Enrypted\Teleopti.Support.Security.txt'
$ReturnTxt = Invoke-Command -computername $servername -scriptblock {Select-String -Path $args[0] -pattern FATAL} -Credential $cred -ArgumentList $TXTFile | ft -Property Line -AutoSize -HideTableHeaders

    If ($ReturnTxt) 
    {
        Write-Output "Searching '$TXTFile' for FATAL errors:"
        Write-Output $ReturnTxt 
    }
  
    Else 
    {
        Write-Host "There are no FATAL errors found in $TXTFile"
    }

# Search Last 3 DBManagerLibrary*.Log for errors
$path = 'C:\Program Files (x86)\Teleopti\DatabaseInstaller'
$ReturnLog = Invoke-Command -ComputerName $servername -ScriptBlock {Get-ChildItem -Path $args[0] -Filter "DBManagerLibrary*.log" |  
    where-object { -not $_.PSIsContainer } |
	sort-object -Property $_.CreationTime |
	select-object -last 3 | 
    Select-String -Pattern 'error'} -Credential $cred -ArgumentList $path |
    ft -Property Line -autosize -HideTableHeaders
  
    If ($ReturnLog) 
    {
        Write-Output "Searching 'DBManagerLibrary*.Log' for errors:"
        Write-Output $ReturnLog
    }
  
    Else 
    {
        Write-Host "There are no errors found in DBManagerLibrary*.Log"
    }  

 # Make TC fail build if any Error
 If ($Eventlog -or $ReturnTxt -or $ReturnLog) 
 {
 Exit 1
 }