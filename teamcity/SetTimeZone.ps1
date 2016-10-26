Param
(
	[Parameter(Mandatory=$true,ParameterSetName='FriendlyName')]
	[Alias('friendly')][String]$TimeZoneFriendlyName,
	[Parameter(Mandatory=$true,ParameterSetName='ShowTimeZone')]
	[Alias('sh')][Switch]$ShowTimeZone
)
	
If($ShowTimeZone)
{
	$TimeZoneInfo = Invoke-Expression "tzutil.exe /l"
	For($i=0; $i -le $TimeZoneInfo.Count; $i++)
	{
		If($i%3 -eq 0)
		{
			$Obj = New-Object -TypeName PSObject
			$Obj | Add-Member -MemberType NoteProperty -Name TimeZone -Value $TimeZoneInfo[$i]
			$Obj | Add-Member -MemberType NoteProperty -Name FriendlyName -Value $TimeZoneInfo[$i+1]
			$Obj
		}
	}
}

If($TimeZoneFriendlyName)
{
	Invoke-Expression "tzutil.exe /s ""$TimeZoneFriendlyName"""
}