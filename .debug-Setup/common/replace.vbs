Dim FileName, Find, ReplaceWith, FileContents, dFileContents
Find         = WScript.Arguments(0)
ReplaceWith  = WScript.Arguments(1)
FileName     = WScript.Arguments(2)

'handle double quote on arg in vbscript, use single quote as input arg
Find=Replace(Find, "'", chr(34))
ReplaceWith=Replace(ReplaceWith, "'", chr(34))


'Read source text file
FileContents = GetFile(FileName)

'replace all string In the source file
dFileContents = replace(FileContents, Find, ReplaceWith, 1, -1, 1)

'Compare source And result
if dFileContents <> FileContents Then
  'write result If different
  WriteFile FileName, dFileContents

  Wscript.Echo "Replace done."
  If Len(ReplaceWith) <> Len(Find) Then 'Can we count n of replacements?
    Wscript.Echo _
    ( (Len(dFileContents) - Len(FileContents)) / (Len(ReplaceWith)-Len(Find)) ) & _
    " replacements."
  End If
Else
  Wscript.Echo "Searched string Not In the source file"
End If

'Read text file
function GetFile(FileName)
  If FileName<>"" Then
    Dim FileStream
    Set FileStream = CreateObject("ADODB.Stream")
    With FileStream
        .Open
        .CharSet = "utf-8"
        .LoadFromFile(FileName)
    End With
    GetFile = FileStream.ReadText()
  End If
End Function

'Write string As a text file.
function WriteFile(FileName, Contents)
  Dim OutStream
  Set OutStream = CreateObject("ADODB.Stream")
  With OutStream
      .Open
      .CharSet = "utf-8"
      .WriteText Contents
      .SaveToFile FileName, 2
  End With
  Set OutStream = Nothing
End Function

