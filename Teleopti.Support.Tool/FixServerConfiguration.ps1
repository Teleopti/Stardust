$webConfig = '..\TeleoptiCCC\Administration\Web.config';
$doc = (Get-Content $webConfig) -as [Xml];
$root = $doc.get_DocumentElement();
$connectionStr = ($root.connectionStrings.add|where {$_.name -eq "Tenancy"}).connectionString;
.\Teleopti.Support.Tool.exe -CS "$connectionStr"