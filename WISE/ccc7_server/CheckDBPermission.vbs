Option Explicit
Dim intMessage
Dim dbCounter
Dim tempCounter
Dim databaseUpgrade
Dim connectString
Dim admConnectString
Dim objConnection,objRecordSet,permissionQuery
Dim server
Dim authModel
Dim sqlLogin
Dim password
Dim admSqlLogin
Dim admPassword
Dim admAuthModel
Dim DB_CCC7
Dim DB_CCCAGG
Dim DB_ANALYTICS
Dim dbExistQuery
Dim debug

'set to true if you need to debug. Note: Session-Object (a WISE specific object) is not avaiable from cscript host
debug = false

dbCounter = -1
tempCounter = 0
Const dbMaster="master"
Const TeleoptiPermissions = 6
Const AllDatabasesOK = 18

if debug Then
	server		= "."
	authModel 	= "SQL"
	sqlLogin	= "TeleoptiDemoUser"
	password	= "TeleoptiDemoPwd2"
	DB_CCC7		= "TeleoptiCCC7_Demo"
	DB_CCCAGG	= "TeleoptiCCC7Agg_Demo"
	DB_ANALYTICS= "TeleoptiAnalytics_Demo"
	databaseUpgrade="NEW"
	admSqlLogin	=""
	admPassword	=""
	admAuthModel="NT"
Else
	'Get Wise props
	server		= Session.Property("SQL_SERVER_NAME")
	authModel 	= Session.Property("SQL_USER_AUTH")
	sqlLogin	= Session.Property("SQL_USER_NAME")
	password	= Session.Property("SQL_USER_PASSWORD")
	DB_CCC7		= Session.Property("DB_CCC7")
	DB_CCCAGG	= Session.Property("DB_CCCAGG")
	DB_ANALYTICS= Session.Property("DB_ANALYTICS")
	databaseUpgrade=Session.Property("DATABASE_UPGRADE")
	admAuthModel=Session.Property("WiseSqlAuth")
	admSqlLogin	=Session.Property("WiseSqlUser")
	admPassword=Session.Property("WiseSqlPass")

	'set Wise props
	Session.Property("DB_CCC7_CONN_OK") = "NO"
	Session.Property("DB_AGG_CONN_OK") = "NO"
	Session.Property("DB_MART_CONN_OK") = "NO"
	Session.Property("DB_ALL_CONN_OK") = "NO"
End If

permissionQuery="SET NOCOUNT ON;SELECT count(*) FROM fn_my_permissions (NULL, 'DATABASE') WHERE permission_name in ('CONNECT','SELECT','INSERT','UPDATE','DELETE','EXECUTE')"
dbExistQuery="select count(*) from sys.databases where name in ('" & DB_CCC7 & "','" & DB_ANALYTICS & "','" & DB_CCCAGG & "')"

'Connection string to the server
If authModel = "NT" Then
	connectString="Provider=sqloledb;Data Source=" & server & ";Integrated Security=SSPI"
Else
	connectString="Provider=sqloledb;Data Source=" & server & ";User Id=" & sqlLogin & ";Password=" & password & ";"
End If

'admConnection string to the server
If admAuthModel = "NT" Then
	admConnectString="Provider=sqloledb;Data Source=" & server & ";Integrated Security=SSPI"
Else
	admConnectString="Provider=sqloledb;Data Source=" & server & ";User Id=" & admSqlLogin & ";Password=" & admPassword & ";"
End If

'count DBs
dbCounter = CheckSingleValue (admConnectString,dbExistQuery,dbMaster)

'If New => Should not exists
if databaseUpgrade = "NEW" Then
	If dbCounter > 0 Then
		intMessage = Msgbox("One or more database already exist in this SQL instance!" & vbCrLf & _
		"" & vbCrLf & _
		"You may continue the installation but the Create Databases for some databases will fail." & vbCrLf & _
		"(This might be needed in case you already have an existing Agg" & vbCrLf & _
		"but need a new CCC7 and Analytics DB to be created)" & vbCrLf & _
		"" & vbCrLf & _
		"Continue?", vbOkCancel + vbExclamation, "Some database(s) already exists!")
		If intMessage = vbOk Then
			If not debug Then
			Session.Property("DB_CCC7_CONN_OK") = "YES"
			Session.Property("DB_MART_CONN_OK") = "YES"
			Session.Property("DB_AGG_CONN_OK") = "YES"
			Session.Property("DB_ALL_CONN_OK") = "YES"
			End If
		End If
	End If
	If dbCounter = 0 And not debug Then
		Session.Property("DB_CCC7_CONN_OK") = "YES"
		Session.Property("DB_MART_CONN_OK") = "YES"
		Session.Property("DB_AGG_CONN_OK") = "YES"
		Session.Property("DB_ALL_CONN_OK") = "YES"
	End If
Else
	'ccc7
	if CheckSingleValue (connectString,permissionQuery,DB_CCC7) = TeleoptiPermissions Then
		if not debug Then Session.Property("DB_CCC7_CONN_OK") = "YES" End if
		tempCounter = tempCounter + TeleoptiPermissions
	End if

	'mart
	if CheckSingleValue (connectString,permissionQuery,DB_ANALYTICS) = TeleoptiPermissions Then
		If Not debug Then Session.Property("DB_MART_CONN_OK") = "YES" End If
		tempCounter = tempCounter + TeleoptiPermissions
	End if

	'agg
	if CheckSingleValue (connectString,permissionQuery,DB_CCCAGG) = TeleoptiPermissions Then
		If not debug Then Session.Property("DB_AGG_CONN_OK") = "YES" End If
		tempCounter = tempCounter + TeleoptiPermissions
	End if

	'All
	If tempCounter = AllDatabasesOK Then
		If not debug Then Session.Property("DB_ALL_CONN_OK") = "YES" End If
	End If
End if 'closing "UPGRADE"

'functions
Public Function CheckSingleValue (strCon, strQuery, strDatabase) 
    'init as "don't have permissions"
	CheckSingleValue = 0
	
	On Error Resume Next
	Set objConnection = CreateObject("ADODB.Connection") 
	Set objRecordSet = CreateObject("ADODB.Recordset") 
	objConnection.Open strCon & ";Initial Catalog=" & strDatabase
	
	objRecordSet.Open strQuery,objConnection
	If Err.Number = 0 Then
		objRecordSet.MoveFirst
		CheckSingleValue=objRecordset(0)
	End If
	objRecordSet.Close 
	objConnection.Close
	
	On Error Goto 0
	Set objConnection = Nothing 
	Set objRecordSet = Nothing 
End Function
