Option Explicit
On Error Resume Next 'With out this, any error will crash MsiExec
Dim intMessage
Dim admConnectString
Dim objConnection,objRecordSet
Dim GroupExist
Dim server
Dim admSqlLogin
Dim admPassword
Dim admAuthModel
Dim debug
Dim dbCounter
Dim db_wingroup

'set to true if you need to debug. Note: Session-Object (a WISE specific object) is not avaiable from cscript host
debug = false

if debug Then
	server		= "."
	admSqlLogin	=""
	admPassword	=""
	admAuthModel="NT"
	db_wingroup = "BUILTIN\Administrators"
Else
	'Get Wise props
	server		= Session.Property("WiseSqlServerName")
	admAuthModel=Session.Property("WiseSqlAuth")
	admSqlLogin	=Session.Property("WiseSqlUser")
	admPassword=Session.Property("WiseSqlPass")
	db_wingroup=Session.Property("DB_WINGROUP")
End If

GroupExist="SET NOCOUNT ON;SELECT count(name) from sys.syslogins where isntgroup = 1 and name = '" & db_wingroup & "'"

'admConnection string to the server
If admAuthModel = "NT" Then
	admConnectString="Provider=sqloledb;Data Source=" & server & ";Integrated Security=SSPI;Initial Catalog=master"
Else
	admConnectString="Provider=sqloledb;Data Source=" & server & ";User Id=" & admSqlLogin & ";Password=" & admPassword & ";Initial Catalog=master"
End If

'Check if wingroup is count = 1
if debug Then
	msgbox CheckSingleValue (admConnectString,GroupExist)
Else
	dbCounter=CheckSingleValue (admConnectString,GroupExist)
	if dbCounter = 1 Then
		Session.Property("DB_WINGROUP_OK") = "YES"
	Else
		Session.Property("DB_WINGROUP_OK") = "NO"
	End If

End If


'functions
Public Function CheckSingleValue (strCon, strQuery) 
    'init as "don't have permissions"
	CheckSingleValue = 0
	
	On Error Resume Next
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
	
	On Error Goto 0
	Set objConnection = Nothing 
	Set objRecordSet = Nothing 
End Function
