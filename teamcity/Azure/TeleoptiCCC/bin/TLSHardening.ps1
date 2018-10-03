$RegKeyTLS10 = "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0\Server"
$RegKeyTLS11 = "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Server"

$Action = "Disable"

Function SetProto{
param(

[string[]]$TargetKey,
[string]$Action
)

foreach($key in  $TargetKey){
   try{
       Get-ItemProperty -Path $key -Name "Enabled" -ErrorAction Stop | Out-Null
       if($Action -eq "Disable"){
          Write-Host "`t`Updating $key"                     
          Set-ItemProperty -Path $key -Name "Enabled" -Value 0 -Type "DWord"
         }
       else{
          Write-Host "`t`Updating $key"
          Set-ItemProperty -Path $key -Name "Enabled" -Value 1 -Type "DWord"
         }
      }Catch [System.Management.Automation.PSArgumentException]{
          if($Action -eq "Disable"){
             Write-Host "`t`Creating $key"
             New-ItemProperty -Path $key -Name "Enabled" -Value 0 -PropertyType "DWord"
            }
          else{
             Write-Host "`t`Creating $key"
             New-ItemProperty -Path $key -Name "Enabled" -Value 1 -PropertyType "DWord"
           }
       }

try{
     Get-ItemProperty -Path $key -Name "DisabledByDefault" -ErrorAction Stop | Out-Null
     if($Action -eq "Disable"){
        Write-Host "`t`Updating $key"
        Set-ItemProperty -Path $key -Name "DisabledByDefault" -Value 1 -Type "DWord"
       }
     else{
        Write-Host "`t`Updating $key"
        Set-ItemProperty -Path $key -Name "DisabledByDefault" -Value 0 -Type "DWord"
        }
     }Catch [System.Management.Automation.PSArgumentException]{
        if($Action -eq "Disable"){
           Write-Host "`t`Creating $key"
           New-ItemProperty -Path $key -Name "DisabledByDefault" -Value 1 -PropertyType "DWord"
          }
        else{
           Write-Host "`t`Creating $key"
           New-ItemProperty -Path $key -Name "DisabledByDefault" -Value 0 -PropertyType "DWord"
          }
     }
  }
}

SetProto -TargetKey $RegKeyTLS10 -Action $Action
SetProto -TargetKey $RegKeyTLS11 -Action $Action

Write-Host "The operation completed successfully, reboot is required" -ForegroundColor Green