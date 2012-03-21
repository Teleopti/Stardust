option explicit
On Error Goto 0

Dim SearchHttp, SearchHttps, MyPos, http, https,msgBoxOK,strURL

'Read from windows installer property.
strURL = Session.Property("AGENT_SERVICE")

'init
SearchHttp	= "http://"
SearchHttps	= "https://"
Session.Property("SDK_CHECK_URL") = "x"

http = CBool(Instr(strURL, SearchHttp))
https = CBool(Instr(strURL, SearchHttps))

'http or https?
if http or https Then
	Session.Property("SDK_CHECK_URL") = "Valid"
else
	msgBoxOK=msgbox("Validate your URL, it does not seem begin with http!",16,"URL must start with http:// or https://")
End If

'https
if https Then
	Session.Property("SDK_SSL") = "True"
	msgBoxOK=MsgBox("A SSL certificate should have been purchased by the customer from certification authority (CA). The customer is also responsible for implementing the certificate at IIS server." & chr(13) & chr(13) & "Please make sure this has been done before you continue with SSL!",48,"Please make a SSL cert is implemented on IIS server")
End If

'Check if the URL is valid
If Not URLExists(strURL) Then
	msgBoxOK=MsgBox("I can't verify the default website specified, please make sure your have the correct hostname and that no firewalls blocks the call." & chr(13) & chr(13) & "The URL might still be valid in case you are doing an offline install",48,"Cant verify default website")
End If


''===========functions==============
Function URLExists(strURL)
	dim objHTTP,strResult
	on error resume next
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
	on error goto 0
end function

function ContainsNotAvailableText(strSource)
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

