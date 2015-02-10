ProjectFile = WScript.Arguments(0)
Set WFWI = CreateObject("WFWI.Document")
WScript.Echo "Open file: " & ProjectFile
WFWI.Open ProjectFile
WFWI.SetProperty "ProductVersion", WScript.Arguments(1)
WFWI.SetSilent
WScript.Echo "Saving file ... "
WFWI.Save ProjectFile
WScript.Echo "Saving file. Done!"
Set WFWI = Nothing
