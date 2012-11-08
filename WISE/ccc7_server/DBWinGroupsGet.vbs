Option Explicit
'====================================
'main
'====================================
On Error Resume Next 'With out this, any error will crash MsiExec
Dim intMessage
Dim admConnectString
Dim objConnection,objRecordSet
Dim displayGroups
Dim server
Dim admSqlLogin
Dim admPassword
Dim admAuthModel
Dim debug
Dim dbCounter

'set to true if you need to debug. Note: Session-Object (a WISE specific object) is not avaiable from cscript host
debug = false

if debug Then
	server		= "."
	admSqlLogin	=""
	admPassword	=""
	admAuthModel="NT"
Else
	'Get Wise props
	server		= Session.Property("WiseSqlServerName")
	admAuthModel=Session.Property("WiseSqlAuth")
	admSqlLogin	=Session.Property("WiseSqlUser")
	admPassword=Session.Property("WiseSqlPass")
End If

displayGroups="SET NOCOUNT ON;SELECT ROW_NUMBER() OVER (ORDER BY name) AS ComboOrder, name from sys.syslogins where isntgroup = 1 and name <> 'NT SERVICE\MSSQLSERVER' and name <> 'NT SERVICE\SQLSERVERAGENT'"

'admConnection string to the server
If admAuthModel = "NT" Then
	admConnectString="Provider=sqloledb;Data Source=" & server & ";Integrated Security=SSPI;Initial Catalog=master"
Else
	admConnectString="Provider=sqloledb;Data Source=" & server & ";User Id=" & admSqlLogin & ";Password=" & admPassword & ";Initial Catalog=master"
End If

'Loop Windows groups on SQL Server
AvailableGroupsGet admConnectString, displayGroups

'====================================
'subs
'====================================
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
'functions
'====================================
Public Function AvailableGroupsGet(strCon, strQuery) 
	On Error Resume Next
	
	Dim strGroupNames
	Dim field
	Set objConnection = CreateObject("ADODB.Connection")
	if err.number <> 0 Then DisplayCustomError("Set objConnection") End If
	
	Set objRecordSet = CreateObject("ADODB.Recordset")
	if err.number <> 0 Then DisplayCustomError("Set objRecordSet") End If
	
	objConnection.Open strCon
	if err.number <> 0 Then DisplayCustomError("objConnection.Open") End If
	
	objRecordSet.Open strQuery,objConnection
	if err.number <> 0 Then DisplayCustomError("objRecordSet.Open") End If
	
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