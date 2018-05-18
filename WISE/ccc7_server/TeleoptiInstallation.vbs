Option Explicit
'With out this, any error will crash MsiExec.
On Error Resume Next 'Use err.number to check for errors where needed!!

'use global variables in order to fetch Wise Props ones
Dim debug
Dim WiseSqlServerName
Dim WiseSqlUser
Dim WiseSqlPass
Dim WiseSqlAuth
Dim DNS_ALIAS
Dim IS_DMZ
Dim DB_WINGROUP
Dim DB_CCC7
Dim DB_CCCAGG
Dim DB_ANALYTICS
Dim DATABASE_UPGRADE

Const strYES="YES"
Const strNO="NO"

'set to true if you need to debug. Note: Session-Object (a WISE specific object) is not avaiable from cscript host
debug = false

'Get current Wise properties
if debug Then
	WiseSqlServerName	= ".\TeleoptiForecast"
	WiseSqlUser			= "sa"
	WiseSqlPass			= "cadadi"
	WiseSqlAuth			= "SQL"
	DNS_ALIAS			= "http://Teleopti625/"
	IS_DMZ				= "YES"
	DB_WINGROUP			= "BUILTIN\Administrators"
	DB_CCC7				= "TeleoptiCCC7_Demo"
	DB_CCCAGG			= "TeleoptiCCC7Agg_Demo"
	DB_ANALYTICS		= "TeleoptiAnalytics_Demo"
	DATABASE_UPGRADE	= "UPGRADE"
Else
	'Get Wise props
	WiseSqlServerName	= Session.Property("WiseSqlServerName")
	WiseSqlAuth			= Session.Property("WiseSqlAuth")
	WiseSqlUser			= Session.Property("WiseSqlUser")
	WiseSqlPass			= Session.Property("WiseSqlPass")
	DNS_ALIAS			= Session.Property("DNS_ALIAS")
	IS_DMZ				= Session.Property("IS_DMZ")
	DB_WINGROUP			= Session.Property("DB_WINGROUP")
	DB_CCC7				= Session.Property("DB_CCC7")
	DB_CCCAGG			= Session.Property("DB_CCCAGG")
	DB_ANALYTICS		= Session.Property("DB_ANALYTICS")
	DATABASE_UPGRADE	= Session.Property("DATABASE_UPGRADE")	
End If

'In debug => Run your Custom Action here:
if debug then
	On Error Goto 0 'In debug; we break for any error.
	'However; ou need to set this one to "On Error Resume Next" in order to test DisplayCustomError(strMessage)
	
	Call DNSHostNameOnlySet()
	Call DBWinGroupsGet()
	Call DBWinGroupCheck()
	Call CheckDbStuff()
	Call ValidateURL()
	Call DetectNET40()
End if

'====================================
'Subs called by Wise custom Actions
'====================================
Sub DetectNET40()
	Dim intMessage
	Dim oShell 
	Dim value
	Dim DetectNET40
	Dim RegKey

	RegKey="HKEY_LOCAL_MACHINE\Software\Microsoft\NET Framework Setup\NDP\v4\Full\Install"
	Set oShell = CreateObject("WScript.Shell")

	'init as "false"
	Call WisePropertySet("NET40INSTALLED",strNO)

	''#If the key isn't there when we try to read it, an error will be generated 
	value = oShell.RegRead(RegKey) 
	If Err.Number = 0 Then
			If value = 1 Then
					Call WisePropertySet("NET40INSTALLED",strYES)
			Else
				intMessage=Msgbox("Can't detect Install-flag in registry key: " & RegKey,48,"Can't find .NET 4.0")
			End If
	Else
		intMessage = Msgbox("Teleopti WFM Server needs .NET 4.0!" & vbCrLf & _
			"You may continue the installation but Teleopti WFM will NOT work" & vbCrLf & _
		"untill .NET 4.0 is properly installed and registered in IIS" & vbCrLf & _
		"" & vbCrLf & _
		"Would you like to access the Microsoft download page?"  & vbCrLf & _
		"http://www.microsoft.com/download/details.aspx?id=17718", vbYesNo + vbCritical, "Can't find .NET 4.0")

		If intMessage = vbYes Then
			oShell.Run("http://www.microsoft.com/download/details.aspx?id=17718")
		End If
	End If

	Set oShell = Nothing
End Sub

Sub ValidateURL()

	Dim SearchHttp, SearchHttps, MyPos, http, https,msgBoxOK
	Const strX		= "x"
	Const strValid	= "Valid"
	Const strTrue	= "True"
	
	'init
	SearchHttp	= "http://"
	SearchHttps	= "https://"
	Call WisePropertySet("SDK_CHECK_URL",strX)

	http = CBool(Instr(DNS_ALIAS, SearchHttp))
	https = CBool(Instr(DNS_ALIAS, SearchHttps))

	'http or https?
	if http or https Then
		Call WisePropertySet("SDK_CHECK_URL",strValid)
	else
		msgBoxOK=msgbox("Validate your URL, it does not seem begin with http!",16,"URL must start with http:// or https://")
	End If

	'https
	if https Then
		Call WisePropertySet("SDK_SSL",strTrue)
		msgBoxOK=MsgBox("A SSL certificate should have been purchased by the customer from certification authority (CA). The customer is also responsible for implementing the certificate at IIS server." & chr(13) & chr(13) & "Please make sure this has been done before you continue with SSL!",48,"Please make a SSL cert is implemented on IIS server")
	End If

	'Check if the URL is valid
	If Not URLExists(DNS_ALIAS) Then
		msgBoxOK=MsgBox("I can't verify the default website specified: " & chr(13) & chr(13) & DNS_ALIAS & chr(13) & chr(13) & "Please make sure your have the correct hostname and that no firewalls blocks the call.",48,"Cant verify default website")
	End If

End Sub


Sub DNSHostNameOnlySet()
	Dim SearchHttp, SearchHttps, MyPos, http, https,msgBoxOK,DNSHOSTNAMEONLY

	'init
	SearchHttp	= "http://"
	SearchHttps	= "https://"

	http = CBool(Instr(DNS_ALIAS, SearchHttp))
	https = CBool(Instr(DNS_ALIAS, SearchHttps))

	If IS_DMZ=strYES Then
		if http Then
			DNSHOSTNAMEONLY = Mid(DNS_ALIAS, 8, Len(DNS_ALIAS)-8)
		End If

		if https Then
			DNSHOSTNAMEONLY = Mid(DNS_ALIAS, 9, Len(DNS_ALIAS)-9)
		End If
	Else
		DNSHOSTNAMEONLY="localhost"
	End if

	Call WisePropertySet("DNSHOSTNAMEONLY",DNSHOSTNAMEONLY)
	
End Sub

Sub DBWinGroupsGet()

	Dim intMessage
	Dim objConnection,objRecordSet
	Dim dbCounter
	Dim strEdition
	Dim admConnectString

	Const strAzure = "SQL Azure"
	Const sqlEdition="SELECT CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))"
	
	'admConnection string to the server
	admConnectString=ConnectionStringGet("master")
	
	strEdition = CheckSingleValue (admConnectString,sqlEdition)

	Const displayGroups="SET NOCOUNT ON;SELECT ROW_NUMBER() OVER (ORDER BY name) AS ComboOrder, name from sys.syslogins where isntgroup = 1 and name <> 'NT SERVICE\MSSQLSERVER' and name <> 'NT SERVICE\SQLSERVERAGENT'"

	'empty ServerName
	If WiseSqlServerName="" Then
		DisplayCustomError("I can't connect to blank SQL Server name, please specify a valid SQL Server")
	End If

	'empty Username
	If (WiseSqlAuth = "SQL" and WiseSqlUser="") Then
		DisplayCustomError("I can't connect blank SQL User Name, please specify a valid login or use Windows NT Authentication")
	End If

	'Loop Windows groups on SQL Server
	If strEdition<>strAzure Then
		AvailableGroupsGet admConnectString, displayGroups
	End If
End Sub

Sub DBWinGroupCheck()
	Dim intMessage
	Dim strGroupExist
	Dim boolGroupExist
	Dim admConnectString

	strGroupExist="SET NOCOUNT ON;SELECT count(name) from sys.syslogins where isntgroup = 1 and name = '" & DB_WINGROUP & "'"

	'admConnection string to the server
	admConnectString=ConnectionStringGet("master")

	'Check if wingroup is count = 1
	boolGroupExist=CheckSingleValue (admConnectString,strGroupExist)
	if boolGroupExist = 1 Then
		Call WisePropertySet("DB_WINGROUP_OK",strYES)
	Else
		Call WisePropertySet("DB_WINGROUP_OK",strNO)
	End If
End Sub 

Sub CheckDbStuff()
	Const strUpgradeErrorFound		= "You got a problem connecting to one or more database. DB-patch might fail. Please go back and review your database names and/or login account."
	Const strNewDatabaseErrorFound	= "Some of the database you are trying to create, already exist. DB-patch will fail. Please go back and review your database names."
	Const strUpgrade = "UPGRADE"
	Const strNew = "NEW"

	Dim admConnectString
	Dim database
	Dim myArray(2)
	Dim countDbError
	
	countDbError=0
	myArray(0) = DB_CCC7
	myArray(1) = DB_ANALYTICS
	myArray(2) = DB_CCCAGG

	admConnectString=ConnectionStringGet("master")

	For Each database In myArray

		If DATABASE_UPGRADE=strUpgrade Then
			countDbError = countDbError + CheckUserDB(database, admConnectString)
		End If
		
		If DATABASE_UPGRADE=strNew Then
			If DBExist(database, admConnectString) Then
				msgbox "Database " & database & " already exist!"
				countDbError = countDbError + 1
			End if
		End If
	
	Next

	If countDbError > 0 And DATABASE_UPGRADE=strUpgrade Then
		msgbox strUpgradeErrorFound, vbOkOnly + vbExclamation, "Error connecting as dbo"
	End If

	If countDbError > 0 And DATABASE_UPGRADE=strNew Then
		msgbox strNewDatabaseErrorFound, vbOkOnly + vbExclamation, "Databases already exists"
	End If
		
End Sub
'====================================
'Subs
'====================================
Sub WisePropertySet(strName,strValue)
	if debug Then
		msgBox "Set property " & strName & " = " & strValue
	Else
		Session.Property(strName) = strValue
	End If
End Sub

Sub DisplayCustomError(strMessage)

    strMessage = VbCrLf & strMessage & VbCrLf & VbCrLf & _
      "Number (dec) : " & Err.Number & VbCrLf & _	
      "Number (hex) : &H" & Hex(Err.Number) & VbCrLf & _
      "Description  : " & Err.Description & VbCrLf & _
      "Source       : " & Err.Source
    Err.Clear
	
	msgbox strMessage, vbOk + vbExclamation, "Error in MSI calling vbscript!"
	wscript.quit
	
End Sub

'====================================
'Functions
'====================================
Public Function CheckUserDB(strDatabase,strAdmConnectString)
On Error Resume Next
CheckUserDB=0

	Const strAzure		= "SQL Azure"
	Dim strUnableToConnect
	Dim strDatabaseDoesNotExist
	Dim strUpgradeDboErrorFound
	Dim userDbConnection
	Dim objConnection
	Dim objRecordSet

	strUnableToConnect = "Sorry I can connect to the database [" &strDatabase& "]"
	strUpgradeDboErrorFound	= "You are not dbo in database  [" &strDatabase& "] This is a requirment. Patch might fail"	
	strDatabaseDoesNotExist = "The database name specified: [" &strDatabase& "], does not exist on this SQL Server!"
	
	Set objConnection = CreateObject("ADODB.Connection")
	Set objRecordSet = CreateObject("ADODB.Recordset")

	userDbConnection	= ConnectionStringGet(strDatabase)

	If isAzure(strAdmConnectString) Then
		objConnection.Open userDbConnection
		if err.number <> 0 Then
			msgbox strUnableToConnect
			CheckUserDB=CheckUserDB+1
		End If
	Else
		objConnection.Open userDbConnection
		if err.number <> 0 Then
			If not DbExist(strDatabase,strAdmConnectString) Then
				msgbox strDatabaseDoesNotExist
				CheckUserDB=CheckUserDB+1
			End If
		End If	
	End If

	If Not isDbo(strDatabase) Then
		msgbox strUpgradeDboErrorFound
		CheckUserDB=CheckUserDB+1
	End If

End Function

Public Function CheckSingleValue (strCon, strQuery) 
On Error Resume Next
	Dim objConnection
	Dim objRecordSet
	
	Set objConnection = CreateObject("ADODB.Connection")
	Set objRecordSet = CreateObject("ADODB.Recordset")

	objConnection.Open strCon
	if err.number <> 0 Then DisplayCustomError("objConnection.Open") End If	

	objRecordSet.Open strQuery,objConnection
	If Err.Number = 0 Then
		objRecordSet.MoveFirst
		CheckSingleValue=objRecordset(0)
	End If

	objRecordSet.Close 
	objConnection.Close

	Set objConnection = Nothing 
	Set objRecordSet = Nothing 
End Function

Public Function AvailableGroupsGet(strCon, strQuery) 
On Error Resume Next
	Dim strGroupNames
	Dim field
	Dim objConnection
	Dim objRecordSet
	
	Set objConnection = CreateObject("ADODB.Connection")
	if err.number <> 0 Then DisplayCustomError("Set objConnection") End If
	
	Set objRecordSet = CreateObject("ADODB.Recordset")
	if err.number <> 0 Then DisplayCustomError("Set objRecordSet") End If
	
	objConnection.Open strCon
	if err.number <> 0 Then DisplayCustomError("objConnection.Open") End If
	
	objRecordSet.Open strQuery,objConnection
	if err.number <> 0 Then DisplayCustomError("objRecordSet.Open") End If
	
	'Empty Resultset, exit
	If objRecordSet.EOF Then
		Combo 1, "No Windows Group found!"
		Exit Function
	End If
	  
	objRecordSet.MoveFirst
	if err.number <> 0 Then DisplayCustomError("objRecordSet.MoveFirst") End If
	
	While Not objRecordSet.EOF
		Combo objRecordSet.Fields.Item("ComboOrder").Value, objRecordSet.Fields.Item("name").Value
		objRecordSet.MoveNext
	wend

	objRecordSet.Close 
	objConnection.Close

	Set objConnection = Nothing 
	Set objRecordSet = Nothing
End Function

Function Combo(intComboOrder,strComboValueAndText)
On Error Resume Next
' This function initializes and then sets variables to set values for the four columns
' of the ComboBox table (Property, Order, Value and Text).  Then a function is called to add
' the row of values to the table.

'  Initialize variables used for each column of the ComboBox table
	Dim ComboProp
	Dim ComboOrder
	Dim ComboValue
	Dim ComboText

'  Set properties for each of the four columns

	ComboProp = "DB_WINGROUP"
	ComboOrder = intComboOrder
	ComboValue = strComboValueAndText
	ComboText = strComboValueAndText

'  Call function to add this row of values to the ComboBox table
	addToComboBox ComboProp, ComboOrder, ComboValue, ComboText
End Function

Function addToComboBox(ByVal ComboProp, ByVal ComboOrder, ByVal ComboValue, ByVal ComboText)
On Error Resume Next
' This function takes values passed into it from the function call and uses these values to create
' and execute a view within the current session of the Windows Installer object.  This view is based
' on a SQL query constructed using the values passed into the function.  If you wish to get a deeper
' understanding of how this function works you should read up on the Windows Installer object
' model in the Windows Installer SDK.

' Initialize variables
	Dim query
	Dim view

' Construct query based on values passed into the function.
' NOTE:  The ` character used is not a single quote, but rather a back quote.  This character is typically
' found on the key used for the ~ symbol in the upper left of the keyboard.

	query = "INSERT INTO `ComboBox` (`Property`, `Order`, `Value`, `Text`) VALUES ('" & ComboProp & "', " & ComboOrder & ", '" & ComboValue & "', '" & ComboText & "') TEMPORARY"

	If debug Then
		msgbox ComboOrder & " : " & ComboValue
	else
		'This statement creates the view object based on our query
		Set view = Session.Database.OpenView(query)

		'This statement executes the view, which actually adds the row into the ComboBox table.
		view.Execute
	End If
	
End Function

Function ConnectionStringGet(strDatabase)
On Error Resume Next
	'admConnection string to the server
	If WiseSqlAuth = "NT" Then
		ConnectionStringGet="Provider=sqloledb;Data Source=" & WiseSqlServerName & ";Integrated Security=SSPI;Initial Catalog=" & strDatabase
	Else
		ConnectionStringGet="Provider=sqloledb;Data Source=" & WiseSqlServerName & ";User Id=" & WiseSqlUser & ";Password=" & WiseSqlPass & ";Initial Catalog=" & strDatabase
	End If
End Function

Function CanConnect(strDBConnectString)
On Error Resume Next	
	CanConnect=false
	Const strCheckConnectPermissions = "SELECT count(*) FROM fn_my_permissions (NULL, 'DATABASE')  WHERE permission_name = 'CONNECT'"
	
	if CheckSingleValue (strDBConnectString, strCheckConnectPermissions) = 1 Then
		CanConnect=true
	End If
End Function

Function DbExist(strDatabase,strAdmConnectString)
On Error Resume Next
	DbExist=false
	Dim dbExistQuery
	dbExistQuery = "select count(*) from sys.databases where name = '" & strDatabase & "'"
	
	If not isAzure(strAdmConnectString) Then
		If CheckSingleValue (strAdmConnectString, dbExistQuery) = 1 Then
			DbExist = true
		End If
	End If
End Function

Function isAzure(strAdmConnectString)
On Error Resume Next
	isAzure=false

	Const strAzure = "SQL Azure"
	Const sqlEdition="SELECT CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))"
	
	If CheckSingleValue (strAdmConnectString, sqlEdition) = strAzure Then
		isAzure = true
	End If
End Function

Function isDbo(strDatabase)
On Error Resume Next
	isDbo=false
	Const sqlDbOwner = "SELECT IS_ROLEMEMBER ('db_owner')"
	Dim userDbConnection
	userDbConnection = ConnectionStringGet(strDatabase)
	
	If CheckSingleValue (userDbConnection, sqlDbOwner) = "1" Then
		isDbo = true
	Else
		
	End If
End Function

Public Function URLExists(strURL)
On Error Resume Next
	dim objHTTP,strResult
	set objHTTP = CreateObject("Microsoft.XMLHTTP")
	
	objHTTP.open "GET", strURL, false
	objHTTP.send
	if Err.Number<>0 then
		' URL Invalid
		Err.Clear
		URLExists=false
	else
		strResult=objHTTP.responseText
		if Instr(1,"1234567890",Left(strResult,1))>0 then
			' HTTP Error code returned
			URLExists=false
		else
			if ContainsNotAvailableText(strResult) then
				URLExists=false
			else
				URLExists=true
			end if
		end if
	end if
	
	set objHTTP=Nothing
end Function

Public Function ContainsNotAvailableText(strSource)
On Error Resume Next
	' Check for common messages when a URL is not available
	dim NAText, i
	
	NAText=Array(_
		"The URL you requested is not available",_
		"the page you requested cannot be found")
	
	for i=0 to UBound(NAText)
		if InStr(1,strSource,LCase(NAText(i)))>0 then
			ContainsNotAvailableText=true
			exit function
		end if
	next
	ContainsNotAvailableText=false
end function