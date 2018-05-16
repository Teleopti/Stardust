Param
(
	[Parameter(Mandatory=$true)]
	[String]$TimeZoneFriendlyName
)
	
If($TimeZoneFriendlyName)
{
	Invoke-Expression "tzutil.exe /s ""$TimeZoneFriendlyName"""
}