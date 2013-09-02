option explicit
on error resume next 'msi cannot handle any error from cscript

dim strDatabase
dim returnValue
dim wshShell
dim objShell
dim objFSO
dim objFile
dim strCommand
dim strPath
dim strFolder

strDatabase = WScript.Arguments.Item(0)

'Display warning message
'returnValue = MsgBox ("A database patch error has occurred for database [" & strDatabase & "]" & VbCrLf & "Please review DbManager log files for more info!",vbOKOnly+vbCritical, "Database patch failed!")

'get current script path
strPath = wscript.ScriptFullName

'get current folder
Set objShell = CreateObject("Wscript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFile = objFSO.GetFile(strPath)
strFolder = objFSO.GetParentFolderName(objFile) 

'start explorer
'strCommand = "explorer.exe /e," & strFolder
'objShell.Run strCommand
dim fileSystem, folder, file, recentFile, sExtension  
Set fileSystem = CreateObject("Scripting.FileSystemObject") 
Set recentFile = Nothing
Set folder = fileSystem.GetFolder(strFolder) 

For Each file in folder.Files
	sExtension=lcase(fileSystem.getextensionname(file))
  If (recentFile is Nothing) Then
	if(sExtension = "log") Then Set recentFile = file	
  ElseIf (file.DateLastModified > recentFile.DateLastModified) Then
  	if(sExtension = "log") Then Set recentFile = file
  End If
Next

If Not recentFile is Nothing Then
	objShell.Run "Notepad.exe " & strFolder & "\" & recentFile.Name
End If

'clean up
Set objShell = Nothing
Set objFSO = Nothing
Set objFile = Nothing
Set fileSystem = Nothing 
Set folder  = Nothing
Set file = Nothing 
Set recentFile = Nothing 
Set sExtension = Nothing