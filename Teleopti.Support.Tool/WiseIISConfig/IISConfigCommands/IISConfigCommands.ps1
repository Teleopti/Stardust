function IISAdmin {
	$ModuleName = "WebAdministration"
	$ModuleLoaded = $false
	$LoadAsSnapin = $false

	if ($PSVersionTable.PSVersion.Major -ge 2)
	{
		if ((Get-Module -ListAvailable | ForEach-Object {$_.Name}) -contains $ModuleName)
		{
			Import-Module $ModuleName
			if ((Get-Module | ForEach-Object {$_.Name}) -contains $ModuleName)
			{
				$ModuleLoaded = $true
			}
			else
			{
				$LoadAsSnapin = $true
			}
		}
		elseif ((Get-Module | ForEach-Object {$_.Name}) -contains $ModuleName)
		{
			$ModuleLoaded = $true
		}
		else
		{
			$LoadAsSnapin = $true
		}
	}
	else
	{
		$LoadAsSnapin = $true
	}

	if ($LoadAsSnapin)
	{
		if ((Get-PSSnapin -Registered | ForEach-Object {$_.Name}) -contains $ModuleName)
		{
			Add-PSSnapin $ModuleName
			if ((Get-PSSnapin | ForEach-Object {$_.Name}) -contains $ModuleName)
			{
				$ModuleLoaded = $true
			}
		}
		elseif ((Get-PSSnapin | ForEach-Object {$_.Name}) -contains $ModuleName)
		{
			$ModuleLoaded = $true
		}
	}
}

function Get-Banan {
return "banan"
}

function Get-Authentication {
    param(
        $path, #TeleoptiCCC/SDK
		$type #windowsAuthentication
    )
	$pspath = "MACHINE/WEBROOT/APPHOST/Default Web Site/" + $path
	
	$temp = Get-WebConfigurationProperty  -pspath $pspath -filter /system.webServer/security/authentication/$type -name enabled
	return $temp.Value
}

function Get-defaultDocument{
	Get-WebConfigurationProperty -Filter //defaultDocument/files/add -PSPath 'IIS:\Sites\Default Web Site' -Name value
}

function Uninstall{
     
     $exists64 = CheckRegistryUninstall64 
     $exists32=  CheckRegistryUninstall32  
        
     if($exists64 -eq 0 ) {
    	 $Args = @("/X{52613B22-2102-4BFB-AAFB-EF420F3A24B5}","/qn", "/L", "uninstall-server.log")
         & MsiExec.exe $Args | out-null
         return $LastExitCode
        }
    elseif($exists32 -eq 0 ){ 
         $Args = @("/X{52613B22-2102-4BFB-AAFB-EF420F3A24B5}","/qn", "/L", "uninstall-server.log")
         & MsiExec.exe $Args | out-null
         return $LastExitCode
        }
    else{    
        return 0
    }
       
}

function CheckRegistryUninstall64{
#reg query HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{52613B22-2102-4BFB-AAFB-EF420F3A24B5} /v DisplayName
    
    & reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{52613B22-2102-4BFB-AAFB-EF420F3A24B5}" /v DisplayName | out-null
    return $LastExitCode #0=found, 1=not found

}

function CheckRegistryUninstall32{
#reg query HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{52613B22-2102-4BFB-AAFB-EF420F3A24B5} /v DisplayName
    
    & reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{52613B22-2102-4BFB-AAFB-EF420F3A24B5}" /v DisplayName | out-null
    return $LastExitCode #0=found, 1=not found

}


