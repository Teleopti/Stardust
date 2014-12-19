ProjectFile = WScript.Arguments(0)
Set WFWI = CreateObject("WFWI.Document")
WScript.Echo "Open file: " & ProjectFile
WFWI.Open ProjectFile
WFWI.SetProperty "ProductVersion", WScript.Arguments(1)
WScript.Echo "Saving file ... "
WFWI.Save ProjectFile
WScript.Echo "Saving file. Done!"
WScript.Sleep 5000

On Error Resume Next
Do
	if IsWriteAccessible(ProjectFile) then Exit Do
	WScript.Echo ProjectFile & "  is locked! Try kill ..."
	Dim oShell : Set oShell = CreateObject("WScript.Shell")
	oShell.Run "taskkill /im WfWI.exe", , True
	WScript.Sleep 1000
Loop
On Error Goto 0
WScript.Echo "The file is now unlocked"

Function IsWriteAccessible(sFilePath)
    ' Strategy: Attempt to open the specified file in 'append' mode.
    ' Does not appear to change the 'modified' date on the file.
    ' Works with binary files as well as text files.

    ' Only 'ForAppending' is needed here. Define these constants
    ' outside of this function if you need them elsewhere in
    ' your source file.
    Const ForReading = 1, ForWriting = 2, ForAppending = 8

    IsWriteAccessible = False

    Dim oFso : Set oFso = CreateObject("Scripting.FileSystemObject")

    On Error Resume Next

    Dim nErr : nErr = 0
    Dim sDesc : sDesc = ""
    Dim oFile : Set oFile = oFso.OpenTextFile(sFilePath, ForAppending)
    If Err.Number = 0 Then
        oFile.Close
        If Err Then
            nErr = Err.Number
            sDesc = Err.Description
        Else
            IsWriteAccessible = True
        End if
    Else
        Select Case Err.Number
            Case 70
                ' Permission denied because:
                ' - file is open by another process
                ' - read-only bit is set on file, *or*
                ' - NTFS Access Control List settings (ACLs) on file
                '   prevents access

            Case Else
                ' 52 - Bad file name or number
                ' 53 - File not found
                ' 76 - Path not found

                nErr = Err.Number
                sDesc = Err.Description
        End Select
    End If    'Set oFso = Nothing

    On Error GoTo 0

    If nErr Then
        Err.Raise nErr, , sDesc
    End If
End Function