Option Explicit

Dim objWMI, objItem, colItems
Dim strComputer, VerOS, VerBig, Ver9x, Version9x, OS, OSystem

On Error Resume Next

strComputer = "."
Set objWMI = GetObject("winmgmts:\\" & strComputer & "\root\cimv2")
Set colItems = objWMI.ExecQuery("Select * from Win32_OperatingSystem",,48)

For Each objItem in colItems
	VerBig = Left(objItem.Version,1)
Next

WScript.Quit VerBig