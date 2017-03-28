function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value;
    if($Invocation.PSScriptRoot)
    {
        $Invocation.PSScriptRoot;
    }
    Elseif($Invocation.MyCommand.Path)
    {
        Split-Path $Invocation.MyCommand.Path
    }
    else
    {
        $Invocation.InvocationName.Substring(0,$Invocation.InvocationName.LastIndexOf("\"));
    }
}

$directory = Get-ScriptDirectory

$webConfig = $directory + '\..\TeleoptiCCC\Administration\Web.config';
$doc = (Get-Content $webConfig) -as [Xml];
$root = $doc.get_DocumentElement();
$connectionStr = ($root.connectionStrings.add|where {$_.name -eq "Tenancy"}).connectionString;
.\Teleopti.Support.Tool.exe -CS "$connectionStr"