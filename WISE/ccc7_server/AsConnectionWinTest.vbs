Option Explicit
'====================================
'main
'====================================
On Error Resume Next 'With out this, any error will crash MsiExec

'declare
Dim dummy
Dim strPath
Dim oShell 
Dim AsServerName
Dim returnValue
Dim debug
Dim PmAuthMode
Dim INSTALLDIR
Dim USERNAME
Const xmlaStatement="<Statement></Statement>"

'current path
strPath = Left(Wscript.ScriptFullName,(Len(Wscript.ScriptFullName)-Len(Wscript.ScriptName)-1))


'init
Set oShell = CreateObject("WScript.Shell")
debug = false
returnValue=0

'Get input
if not debug Then
	AsServerName = Session.Property("AS_SERVER_NAME")
	INSTALLDIR = Session.Property("INSTALLDIR")
	USERNAME = Session.Property("USERNAME")
Else
	AsServerName	= "."
	INSTALLDIR		= "C:\Program Files (x86)\Teleopti"
	USERNAME		= "Jim"
End If

'Connect using current windows user
returnValue = oShell.Run(chr(34) & strPath & "\ascmd.exe" & chr(34) & " -S " & chr(34) & AsServerName & chr(34) & " -Q " & chr(34) & xmlaStatement & chr(34), 0, true)
If Err.Number = 0 Then
	If returnValue <> 0 Then
			dummy=Msgbox("Could not connect to AS Server using current Windows account" &_
			vbCrLf & "AS Server: " & chr(34) & AsServerName & chr(34) &_
			vbCrLf & "Win user: " & chr(34)  & username & chr(34) &_
			vbCrLf &_
			vbCrLf & "You will not be able to create and process the cube during this installation!" &_
			" Please verify that you AS server is running and that you can connect with your current Windows credentials." &_
			vbCrLf &_
			"You may continue the installation but in that case you manually need to deploy and process the Teleopti PM cube using:"  &_
			vbCrLf & INSTALLDIR & "\Cube\DropCreateProcess.bat",48,"Current Windows user can't connect to AS Server")
	End If
Else
	dummy=Msgbox("Could not execute ascmd.exe during MSI installation",48,"Can't run ascmd.exe")
End If

Set oShell = Nothing