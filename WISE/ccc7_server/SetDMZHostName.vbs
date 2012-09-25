option explicit
On Error Goto 0

Dim SearchHttp, SearchHttps, MyPos, http, https,msgBoxOK,strURL,dmz_hostname,strIsDmz

'Read from windows installer property.
strURL = Session.Property("DNS_ALIAS")
strIsDmz = Session.Property("IS_DMZ")

'init
SearchHttp	= "http://"
SearchHttps	= "https://"

http = CBool(Instr(strURL, SearchHttp))
https = CBool(Instr(strURL, SearchHttps))

If strIsDmz="YES" Then
	if http Then
		dmz_hostname = Mid(strURL, 8, Len(strURL)-8)
	End If

	if https Then
		dmz_hostname = Mid(strURL, 9, Len(strURL)-9)
	End If
Else
	dmz_hostname="localhost"
End if

Session.Property("DMZ_HOSTNAME") = dmz_hostname