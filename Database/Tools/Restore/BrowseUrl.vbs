option explicit
on error resume next

'main
dim sdkUrl
dim args
dim httpStatus
const intHttpStatusOK=200

Set args = WScript.Arguments
sdkUrl = args.Item(0)

httpStatus=httpStatusGet(sdkUrl)
WScript.Quit httpStatus

'=====functions======
function httpStatusGet(url)
	dim objHttp,count

	WScript.Echo url
	set objHttp = CreateObject("microsoft.xmlhttp")
	For count = 1 to 3
		objHttp.open "GET",url,false
		objHttp.send
		WScript.Echo "Request status is: " & objHttp.status
		httpStatusGet = objHttp.status
		if httpStatusGet = intHttpStatusOK then
			Exit For
		End If
		WScript.Echo "Trying again in 15 seconds... "
		WScript.Sleep 15000
	Next
End function
