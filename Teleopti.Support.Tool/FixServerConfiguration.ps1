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
if(!(Test-Path $webConfig)){
    $webConfig = $directory + '\..\..\..\sitesroot\8\Web.config';
}
$doc = (Get-Content $webConfig) -as [Xml];
$root = $doc.get_DocumentElement();

$SupportTool = $directory + "\Teleopti.Support.Tool.exe"
$conn = ($root.connectionStrings.add|where {$_.name -eq "Tenancy"}).connectionString;
$args = @"
"-CS" "$conn"
"@
$p = Start-Process $SupportTool -ArgumentList $args -wait -NoNewWindow -PassThru
$p.HasExited
$LastExitCode = $p.ExitCode

if ($LastExitCode -ne 0) {
	throw "SupportTool generated an error!"
}
