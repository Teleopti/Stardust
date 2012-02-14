RegKey35="HKEY_LOCAL_MACHINE\Software\Microsoft\NET Framework Setup\NDP\v3.5\Install"
RegKeySp="HKEY_LOCAL_MACHINE\Software\Microsoft\NET Framework Setup\NDP\v3.5\SP"
Set oShell = CreateObject("WScript.Shell")
Session.Property("NET35SP1INSTALLED") = "NO"

''#If the key isn't there when we try to read it, an error will be generated 
On Error Resume Next 
value35 = oShell.RegRead(RegKey35) 
If Err.Number = 0 Then
		On Error GOTO 0
		If value35 = 1 Then
			On Error Resume Next 
			valueSp = oShell.RegRead(RegKeySp) 
			If Err.Number = 0 Then
				On Error GOTO 0
				If valueSp = 1 Then
					Session.Property("NET35SP1INSTALLED") = "YES"
				Else
					dummy=Msgbox(".NET 3.5 is missing SP1 on this server, please download and install!",48,"Can't find .NET 3.5 sp1")
				End If
			End If
		End If
Else
	On Error GOTO 0
	dummy=Msgbox("Teleopti CCC7 needs .NET 3.5 sp1 on this server, please download and install!",48,"Can't find .NET 3.5 sp1")
End If

Set oShell = Nothing