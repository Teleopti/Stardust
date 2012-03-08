option explicit
on error goto 0
Dim ArgObj, var1, var2, strComputer, objIIS
Set ArgObj = WScript.Arguments

var1 = ArgObj(0) 'SitePath
var2 = ArgObj(1) 'Version

strComputer = "LocalHost"
set objIIS=getobject("IIS://localhost/w3svc/1/root/" + var1)
Call SetASPDotNetVersion(objIIS,var2)

'****************************************************************************************** 
' Name: SetASPDotNetVersion 
' Description: Set the script mappings for the specified ASP.NET version 
' Inputs: objIIS, strNewVersion 
'******************************************************************************************
Sub SetASPDotNetVersion(objIIS, strNewVersion) 
 Dim i, ScriptMaps, arrVersions(2), thisVersion, thisScriptMap 
 Dim strSearchText, strReplaceText 
 
	Select Case Trim(LCase(strNewVersion)) 
		Case "v2.0" 
		strReplaceText = "v2.0.50727" 
		Case "v4.0" 
		strReplaceText = "v4.0.30319" 
		Case Else 
		wscript.echo "WARNING: Non-supported ASP.NET version specified!" 
		Exit Sub 
	End Select 
 
 ScriptMaps = objIIS.ScriptMaps 
 arrVersions(0) = "v2.0.50727" 
 arrVersions(1) = "v4.0.30319" 
 'Loop through all three potential old values 
 For Each thisVersion in arrVersions 
  'Loop through all the mappings 
  For thisScriptMap = LBound(ScriptMaps) to UBound(ScriptMaps) 
   'Replace the old with the new  
   ScriptMaps(thisScriptMap) = Replace(ScriptMaps(thisScriptMap), thisVersion, strReplaceText) 
  Next 
 Next  
 
 objIIS.ScriptMaps = ScriptMaps 
 objIIS.SetInfo 
 wscript.echo "<-------Set ASP.NET version to " & strNewVersion & " successfully.------->" 
End Sub 
