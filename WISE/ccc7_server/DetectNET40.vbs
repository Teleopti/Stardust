Dim intMessage
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
			intMessage=Msgbox("Can't detect Install-flag in registry key: " & RegKey,48,"Can't find .NET 4.0")
		End If
Else
	On Error GOTO 0
	intMessage = Msgbox("Teleopti CCC Server needs .NET 4.0!" & vbCrLf & _
		"You may continue the installation but Teleopti CCC will NOT work" & vbCrLf & _
	"untill .NET 4.0 is properly installed and registered in IIS" & vbCrLf & _
	"" & vbCrLf & _
	"Would you like to access the Microsoft download page?"  & vbCrLf & _
	"http://www.microsoft.com/download/details.aspx?id=17718", vbYesNo + vbCritical, "Can't find .NET 4.0")

	If intMessage = vbYes Then
		oShell.Run("http://www.microsoft.com/download/details.aspx?id=17718")
	End If
End If

Set oShell = Nothing