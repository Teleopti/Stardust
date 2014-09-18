ProjectFile = WScript.Arguments(0)
Set WFWI = CreateObject("WFWI.Document")
WFWI.Open ProjectFile
WFWI.SetProperty "ProductVersion", WScript.Arguments(1)
WFWI.Save ProjectFile