Option Explicit

Dim tempCounter
Dim connectString
Dim objConnection,objRecordSet,permissionQuery
Dim server
Dim trusted
Dim sqlLogin
Dim password
Dim DB_CCC7
Dim DB_CCCAGG
Dim DB_ANALYTICS

tempCounter = 0
Const TeleoptiPermissions = 6
Const AllDatabasesOK = 18
server		= Session.Property("SQL_SERVER_NAME")
trusted 	= Session.Property("SQL_USER_AUTH")
sqlLogin	= Session.Property("SQL_USER_NAME")
password	= Session.Property("SQL_USER_PASSWORD")
DB_CCC7		= Session.Property("DB_CCC7")
DB_CCCAGG	= Session.Property("DB_CCCAGG")
DB_ANALYTICS= Session.Property("DB_ANALYTICS")

permissionQuery="SET NOCOUNT ON;SELECT count(*) FROM fn_my_permissions (NULL, 'DATABASE') WHERE permission_name in ('CONNECT','SELECT','INSERT','UPDATE','DELETE','EXECUTE')"

'Connection string to the server
If trusted = "NT" Then
	connectString="Provider=sqloledb;Server=" & server & ";Trusted_Connection=yes"
	tempCounter = tempCounter + TeleoptiPermissions
Else
	connectString="Provider=sqloledb;Server=" & server & ";Uid=" & sqlLogin & ";Pwd=" & password & ";"
End If

'ccc7
if CheckPermission (connectString,permissionQuery,DB_CCC7) = TeleoptiPermissions Then
	Session.Property("DB_CCC7_CONN_OK") = "YES"
	tempCounter = tempCounter + TeleoptiPermissions
	
Else
	Session.Property("DB_CCC7_CONN_OK") = "NO"
End if

'mart
if CheckPermission (connectString,permissionQuery,DB_ANALYTICS) = TeleoptiPermissions Then
	Session.Property("DB_MART_CONN_OK") = "YES"
	tempCounter = tempCounter + TeleoptiPermissions
Else
	Session.Property("DB_MART_CONN_OK") = "NO"
End if

'agg
if CheckPermission (connectString,permissionQuery,DB_CCCAGG) = TeleoptiPermissions Then
	Session.Property("DB_AGG_CONN_OK") = "YES"
	tempCounter = tempCounter + TeleoptiPermissions
Else
	Session.Property("DB_AGG_CONN_OK") = "NO"
End if

'All
If tempCounter = AllDatabasesOK Then
	Session.Property("DB_ALL_CONN_OK") = "YES"
Else
	Session.Property("DB_ALL_CONN_OK") = "NO"
End If

'functions
Public Function CheckPermission (strCon, strQuery, strDatabase) 
    'init as "don't have permissions"
	CheckPermission = 0
	
	On Error Resume Next
	Set objConnection = CreateObject("ADODB.Connection") 
	Set objRecordSet = CreateObject("ADODB.Recordset") 
	objConnection.Open strCon & ";Database=" & strDatabase

	objRecordSet.Open strQuery,objConnection
	If Err.Number = 0 Then
		objRecordSet.MoveFirst
		CheckPermission=objRecordset(0)
	End If
	objRecordSet.Close 
	objConnection.Close
	
	On Error Goto 0
	Set objConnection = Nothing 
	Set objRecordSet = Nothing 
End Function
