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
debug = true

'Get current Wise properties
if debug Then
	WiseSqlServerName	= ".\teleoptiForecast"
	WiseSqlUser			= ""
	WiseSqlPass			= ""
	WiseSqlAuth			= "NT"
	DNS_ALIAS			= "http://Teleopti625/"
	IS_DMZ				= "YES"
	DB_WINGROUP			= "BUILTIN\Administrators"
	DB_CCC7				= "TeleoptiCCC7_Demo"
	DB_CCCAGG			= "TeleoptiCCC7Agg_Demo6"
	DB_ANALYTICS		= "TeleoptiAnalytics_Demo"
	DATABASE_UPGRADE	= "YES"
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
'	On Error Goto 0 'In debug; we break for any error.
	'However; ou need to set this one to "On Error Resume Next" in order to test DisplayCustomError(strMessage)
	
'	Call DNSHostNameOnlySet()
'	Call DBWinGroupsGet()
'	Call DBWinGroupCheck()
	Call CheckDbStuff()
	
End if

'====================================
'Subs called by Wise custom Actions
'====================================
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
	Const sqlEdition	= "SELECT CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))"
	Const isDbOwner		= "SELECT IS_ROLEMEMBER ('db_owner')"
	Const strUpgradeErrorFound		= "You got a problem connecting to one or more database. Please go back and review your database names."
	Const strNewDatabaseErrorFound	= "Some of the database you are trying to create already exist. Please go back and review your database names."

	Dim admConnectString
	Dim database
	Dim myArray(2)
	Dim DbError
	
	DbError=0
	myArray(0) = DB_CCC7
	myArray(1) = DB_ANALYTICS
	myArray(2) = DB_CCCAGG

	'admConnection string to the server
	admConnectString=ConnectionStringGet("master")
	
	If DATABASE_UPGRADE=strYES Then
		For Each database In myArray
			DbError = DbError + CheckUserDB(database, admConnectString)
		Next
		
		if DbError > 0 Then
				msgbox strUpgradeErrorFound, vbOkOnly + vbExclamation, "Error connecting"
		End If
	Else
		For Each database In myArray
			DbError = DbError + DBExist(database, admConnectString)
		Next
		if DbError > 0 Then
				msgbox strNewDatabaseErrorFound, vbOkOnly + vbExclamation, "Error connecting"
		End If
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
	Dim userDbConnection
	Dim objConnection
	Dim objRecordSet

	strUnableToConnect = "Sorry I can connect to the database [" &strDatabase& "]"
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
End Function

Public Function CheckSingleValue (strCon, strQuery) 

    'init as "don't have permissions"
	CheckSingleValue = 0

	Dim objConnection
	Dim objRecordSet
	
	Set objConnection = CreateObject("ADODB.Connection")
	Set objRecordSet = CreateObject("ADODB.Recordset")

	objConnection.Open strCon

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
	if err.number <> 0 Then DisplayCustomError("addToComboBox") End If
End Function

Function addToComboBox(ByVal ComboProp, ByVal ComboOrder, ByVal ComboValue, ByVal ComboText)

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
		if err.number <> 0 Then DisplayCustomError("view.Execute") End If
	End If
	
End Function

Function ConnectionStringGet(strDatabase)
	'admConnection string to the server
	If WiseSqlAuth = "NT" Then
		ConnectionStringGet="Provider=sqloledb;Data Source=" & WiseSqlServerName & ";Integrated Security=SSPI;Initial Catalog=" & strDatabase
	Else
		ConnectionStringGet="Provider=sqloledb;Data Source=" & WiseSqlServerName & ";User Id=" & WiseSqlUser & ";Password=" & WiseSqlPass & ";Initial Catalog=" & strDatabase
	End If
End Function

Function CanConnect(strDBConnectString)
	
	CanConnect=false
	Const strCheckConnectPermissions = "SELECT count(*) FROM fn_my_permissions (NULL, 'DATABASE')  WHERE permission_name = 'CONNECT'"
	
	if CheckSingleValue (strDBConnectString, strCheckConnectPermissions) = 1 Then
		CanConnect=true
	End If
End Function

Function DbExist(strDatabase,strAdmConnectString)
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
	isAzure=false

	Const strAzure = "SQL Azure"
	Const sqlEdition="SELECT CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))"
	
	If CheckSingleValue (strAdmConnectString, sqlEdition) = strAzure Then
		isAzure = true
	End If
End Function
