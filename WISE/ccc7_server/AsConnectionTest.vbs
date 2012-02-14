'declare
Dim dummy
Dim oShell 
Dim args
Dim username
Dim password
Dim AsServerName
Dim returnValue
Dim xmlaStatement
Dim strDelim
Dim strArgs
Dim arrArgs

dummy=Msgbox("hepp")

'strDelim = ","
'strArgs = Session.Property("CustomActionData")
'arrArgs = Split(strArgs, strDelim)

username = Session.Property("PM_ANONYMOUS_DOMAINUSER")
password = Session.Property("PM_ANONYMOUS_PWD")
AsServerName = Session.Property("AS_SERVER_NAME")
returnValue=0
'Get args
'AsServerName	= "antonov\sql2005test" 'args.Item(0)
'username		= "toptinet\TFSIntegration" 'args.Item(1)
'password		= "m8kemew0rk" 'args.Item(2)

'init
xmlaStatement="<Statement></Statement>"
Set oShell = CreateObject("WScript.Shell")

'disable error handling for the next line
On Error Resume Next
dummy=Msgbox("ascmd.exe -S " & chr(34) & AsServerName & chr(34) & " -U " & chr(34) & username & chr(34) & " -P " & chr(34) & password & chr(34) & " -Q " & chr(34) & xmlaStatement & chr(34),48,"Command")
'returnValue = oShell.Run("ascmd.exe -S " & chr(34) & AsServerName & chr(34) & " -U " & chr(34) & username & chr(34) & " -P " & chr(34) & password & chr(34) & " -Q " & chr(34) & xmlaStatement & chr(34), 0, true)

If Err.Number = 0 Then
	On Error GOTO 0
	If returnValue = 0 Then
		Session.Property("ACC") = "YES"
	Else
		dummy=Msgbox("Could not execute connect to the give ASServer name, " & vbCrLf & AsServerName & vbCrLf & "Please go back and updated your AS server or disbable PM-feature to continue.",48,"Can't connect to ASServer")
	End If
Else
	On Error GOTO 0
	dummy=Msgbox("Could not execute ascmd.exe during MSI installation. Please disbale the PM-feature to continue.",48,"Can't run ascmd.exe")
End If
dummy=Msgbox("ReturnValue from ascmd is: " & returnValue,48,"final")
Set oShell = Nothing