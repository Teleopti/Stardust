Dim dummy
Dim oShell 
Dim value
Dim DetectNET40
Dim RegKey

RegKey="HKEY_LOCAL_MACHINE\Software\Microsoft\NET Framework Setup\NDP\v4\Full\Install"
Set oShell = CreateObject("WScript.Shell")

Session.Property("NET40INSTALLED") = "NO"
''#If the key isn't there when we try to read it, an error will be generated 
On Error Resume Next 
value = oShell.RegRead(RegKey) 
If Err.Number = 0 Then
		On Error GOTO 0
		If value = 1 Then
			Session.Property("NET40INSTALLED") = "YES"
		Else
			dummy=Msgbox("Can't detect Install-flag in registry key: " & RegKey,48,"Can't find .NET 4.0")
		End If
Else
	On Error GOTO 0
	dummy=Msgbox("AgentPortalWeb needs .NET 4.0 on this server, please download and install!",48,"Can't find .NET 4.0")
End If

Set oShell = Nothing