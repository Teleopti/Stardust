Option Explicit
'====================================
'main
'====================================
On Error Resume Next 'Supress all errors since they will crash MsiExec

'declare
Dim strPath
Dim dummy
Dim oShell 
Dim username
Dim password
Dim AsServerName
Dim returnValue
Dim debug
Dim PmAuthMode
Const isAnonymous="Anonymous"
Const xmlaStatement="<Statement></Statement>"

'current path
strPath = Left(Wscript.ScriptFullName,(Len(Wscript.ScriptFullName)-Len(Wscript.ScriptName)-1))

'init
Set oShell = CreateObject("WScript.Shell")
debug = false
returnValue=0

'Get input
if not debug Then
	username = Session.Property("PM_ANONYMOUS_DOMAINUSER")
	password = Session.Property("PM_ANONYMOUS_PWD")
	AsServerName = Session.Property("AS_SERVER_NAME")
	PmAuthMode = Session.Property("PM_AUTH_MODE")
Else
	AsServerName	= "."
	username		= ".\TestUser"
	password		= "TestPwd"
	PmAuthMode		= isAnonymous
End If

'If PM in impersonate mode try connect to AS account thoose Windows credentials
if PmAuthMode = isAnonymous Then
	returnValue = oShell.Run(chr(34) & strPath & "\ascmd.exe" & chr(34) & " -S " & chr(34) & AsServerName & chr(34) & " -U " & chr(34) & username & chr(34) & " -P " & chr(34) & password & chr(34) & " -Q " & chr(34) & xmlaStatement & chr(34), 0, true)
	If Err.Number = 0 Then
		If returnValue <> 0 Then
			dummy=Msgbox("Could not connect to AS Server using Windows impersonation" &_
			vbCrLf & "AS Server: " & chr(34) & AsServerName & chr(34) &_
			vbCrLf & "Win user: " & chr(34)  & username & chr(34) &_
			vbCrLf &_
			vbCrLf & "Please go back and updated your PM user and/or password or disbable " &_
			"the PM-feature. You may also continue the installation," &_
			" but PM and ETL will not work until your verify and add " &_
			" the imporsonate account as a users in Analysis Services",48,"PM Impersonate - Windows user can't connect to AS Server")
		End If
	Else
		dummy=Msgbox("Could not find or execute ascmd.exe during MSI installation",48,"Can't run ascmd.exe")
	End If
End if
Set oShell = Nothing