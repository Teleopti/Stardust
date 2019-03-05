function SetRegAcl 
{
    Param (
        $Service,
        $Rights
    )
    
    log-info "Setting acl in registry for service: '$Service', Users: 'BuiltIn\Users', AccessControl: '$Rights'"

    $acl = Get-Acl "HKLM:\SYSTEM\CurrentControlSet\Services\$Service\Performance"
    $person = [System.Security.Principal.NTAccount]"BuiltIn\Users"          
    $access = [System.Security.AccessControl.RegistryRights]"$Rights"
    $inheritance = [System.Security.AccessControl.InheritanceFlags]"ContainerInherit,ObjectInherit"
    $propagation = [System.Security.AccessControl.PropagationFlags]"None"
    $type = [System.Security.AccessControl.AccessControlType]"Allow"
    $rule = New-Object System.Security.AccessControl.RegistryAccessRule($person,$access,$inheritance,$propagation,$type)
    $acl.AddAccessRule($rule)
    $acl |Set-Acl
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
    
    SetRegAcl -Service "BITS" -Rights "ReadPermission"
    SetRegAcl -Service "lsa" -Rights "ReadPermission"
    SetRegAcl -Service "Perfnet" -Rights "ReadPermission"
}

Catch
{
    log-error "$_.Exception"
	log-info "$_.Exception"
    Throw $_.Exception
}