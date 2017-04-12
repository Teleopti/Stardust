# Call this from inside a startup task/batch file as shown in the next two lines (minus the '# ')
# PowerShell -ExecutionPolicy Unrestricted .\DisableSslV3.ps1 >> log-DisableSslV3.txt 2>&1
# EXIT /B 0

# Credits: 
# http://azure.microsoft.com/blog/2014/10/19/how-to-disable-ssl-3-0-in-azure-websites-roles-and-virtual-machines/
# http://lukieb.blogspot.com/2014/11/tightening-up-your-azure-cloud-service.html

$nl = [Environment]::NewLine
$regkeys = @(
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0\Client",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0\Server",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Client",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Server",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Server",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 2.0",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 2.0\Client",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 2.0\Server",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 3.0",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 3.0\Client",
"HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 3.0\Server",
"HKLM:\SOFTWARE\Policies\Microsoft\Cryptography\Configuration\SSL\00010002"
)

# Cipher order as per Mozilla: https://wiki.mozilla.org/Security/Server_Side_TLS (Intermediate set - as mapped to Windows names)
$cipherorder = "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256_P256,"
$cipherorder += "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256_P384,"
$cipherorder += "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256_P521,"
$cipherorder += "TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384_P521,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256_P521,"
$cipherorder += "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256_P384,"
$cipherorder += "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256_P521,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA_P521,"
$cipherorder += "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA_P521,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384_P256,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384_P384,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384_P521,"
$cipherorder += "TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384_P521,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,"
$cipherorder += "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA_P521,"
$cipherorder += "TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA_P521,"
$cipherorder += "TLS_RSA_WITH_AES_128_CBC_SHA256,"
$cipherorder += "TLS_RSA_WITH_AES_128_CBC_SHA,"
$cipherorder += "TLS_RSA_WITH_AES_256_CBC_SHA256,"
$cipherorder += "TLS_RSA_WITH_AES_256_CBC_SHA"

# If any settings are changed, this will change to $True and the server will reboot
$reboot = $False

Function Set-CryptoSetting {
  param (
    $keyindex,
    $value,
    $valuedata,
    $valuetype,
    $restart
  )

    # For printing to console
    $regKey = $regkeys[$keyindex]

	# Check for existence of registry key, and create if it does not exist
	If (!(Test-Path -Path $regkeys[$keyindex])) {
		Write-Host "Creating key: $regKey$nl"
		New-Item $regkeys[$keyindex] | Out-Null
	}

	# Get data of registry value, or null if it does not exist
	$val = (Get-ItemProperty -Path $regkeys[$keyindex] -Name $value -ErrorAction SilentlyContinue).$value

	If ($val -eq $null) {
		# Value does not exist - create and set to desired value
		Write-Host "Value $regKey\$value does not exist, creating...$nl"
		New-ItemProperty -Path $regkeys[$keyindex] -Name $value -Value $valuedata -PropertyType $valuetype | Out-Null
		$restart = $True
	} Else {
		# Value does exist - if not equal to desired value, change it
		If ($val -ne $valuedata) {
		  Write-Host "Value $regKey\$value not correct, setting it$nl"
		  Set-ItemProperty -Path $regkeys[$keyindex] -Name $value -Value $valuedata
		  $restart = $True
		}
		Else
		{
			Write-Host "Value $regKey\$value already set correctly$nl"
		}
	}
	return $restart
}

# Special function that can handle keys that have a forward slash in them. Powershell changes the forward slash
# to a backslash in any function that takes a path.
Function Set-CryptoKey {
 param (
  $parent,
  $childkey,
  $value,
  $valuedata,
  $valuetype,
  $restart
 )

	$child = $parent.OpenSubKey($childkey, $true);

	If ($child -eq $null) {
		# Need to create child key
		$child = $parent.CreateSubKey($childkey);
	}

	# Get data of registry value, or null if it does not exist
	$val = $child.GetValue($value);

	If ($val -eq $null) {
		# Value does not exist - create and set to desired value
		Write-Host "Value $child\$value does not exist, creating...$nl"
		$child.SetValue($value, $valuedata, $valuetype);
		$restart = $True
	} Else {
		# Value does exist - if not equal to desired value, change it
		If ($val -ne $valuedata) {
			Write-Host "Value $child\$value not correct, setting it$nl"
			$child.SetValue($value, $valuedata, $valuetype);
			$restart = $True
		}
		Else
		{
			Write-Host "Value $child\$value already set correctly$nl"
		}
	}

	return $restart
}

# Check for existence of parent registry keys (SSL 2.0 and SSL 3.0), and create if they do not exist
For ($i = 9; $i -le 12; $i = $i + 3) {
	If (!(Test-Path -Path $regkeys[$i])) {
		New-Item $regkeys[$i] | Out-Null
	}
}

# Ensure SSL 2.0 disabled for client
$reboot = Set-CryptoSetting 10 DisabledByDefault 1 DWord $reboot

# Ensure SSL 2.0 disabled for server
$reboot = Set-CryptoSetting 11 Enabled 0 DWord $reboot

# Ensure SSL 3.0 disabled for client
$reboot = Set-CryptoSetting 13 DisabledByDefault 1 DWord $reboot

# Ensure SSL 3.0 disabled for server
$reboot = Set-CryptoSetting 14 Enabled 0 DWord $reboot

# Set cipher priority
$reboot = Set-CryptoSetting 15 Functions $cipherorder String $reboot

# We have to do something special with these keys if they contain a forward-slash since
# Powershell converts the forward slash to a backslash and it screws up the creation of the key!
#
# Just create these parent level keys first
$cipherskey = (get-item HKLM:\).OpenSubKey("SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Ciphers",$true)
If ($cipherskey -eq $null) {
	$cipherskey = (get-item HKLM:\).CreateSubKey("SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Ciphers")
}

$hasheskey = (get-item HKLM:\).OpenSubKey("SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Hashes",$true)
If ($hasheskey -eq $null) {
	$hasheskey = (get-item HKLM:\).CreateSubKey("SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Hashes")
}

# Then add sub keys using a different function
# Disable RC4, DES, EXPORT, eNULL, aNULL, PSK and aECDH
$reboot = Set-CryptoKey $cipherskey "RC4 128/128" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $cipherskey "Triple DES 168" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $cipherskey "RC2 128/128" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $cipherskey "RC4 64/128" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $cipherskey "RC4 56/128" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $cipherskey "RC2 56/128" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $cipherskey "DES 56" Enabled 0 DWord $reboot  # It's not clear whether the key is DES 56 or DES 56/56
$reboot = Set-CryptoKey $cipherskey "DES 56/56" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $cipherskey "RC4 40/128" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $cipherskey "RC2 40/128" Enabled 0 DWord $reboot

# Disable MD5, enable SHA (which should be by default)
$reboot = Set-CryptoKey $hasheskey "MD5" Enabled 0 DWord $reboot
$reboot = Set-CryptoKey $hasheskey "SHA" Enabled 0xFFFFFFFF DWord $reboot

$cipherskey.Close();
$hasheskey.Close();


# If any settings were changed, reboot
<#
If ($reboot) {
	Write-Host "Rebooting now..."
	# shutdown: restart, time 5 sec, comment "..", force running apps to close
	# machine readable reason, planned, 2:4 as reason
	shutdown.exe /r /t 5 /c "Crypto settings changed" /f /d p:2:4
}
#>