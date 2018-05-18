
    [CmdletBinding()]
    param (
        [Parameter(Position=0,mandatory=$true)] 
        [ValidateScript({Test-Path $_})]    
        [String]$FilePath,
        [Parameter(Position=1,mandatory=$true)]
        $ProductVersion)

Write-Host ""
Write-Host "This Version number will be set: $ProductVersion" -ForegroundColor Cyan
Write-Host ""

Try {

# Add Required WiX Type Libraries
Add-Type -Path "C:\Program Files (x86)\WiX Toolset v4.0\bin\WixToolset.Dtf.WindowsInstaller.dll";
 
# Open the MSI Database
$OpenMSIDB = New-Object WixToolset.Dtf.WindowsInstaller.Database("$FilePath", [WixToolset.Dtf.WindowsInstaller.DatabaseOpenMode]::Direct);
 
#Create a Select Query against ProductVersion property
$SQLQuery = "SELECT * FROM Property WHERE Property= 'ProductVersion'"
 
#Create and Execute a View object
[WixToolset.Dtf.WindowsInstaller.View]$oView = $OpenMSIDB.OpenView($SQLQuery)
$oView.Execute()
 
#Fetch Result
$oRecord = $oView.Fetch()
$sProductVersion = $oRecord.GetString(2)
 
#Display ProductVersion Field
Write-Host "This is the old Version number:"
"ProductVersion = $($sProductVersion)"
 
#Set new ProductVersion nr
$oRecord.SetString("Value","$ProductVersion")
$oView.Modify([WixToolset.Dtf.WindowsInstaller.ViewModifyMode]::Update,$oRecord)

#Close the Database.
$oView.Close();
$OpenMSIDB.Dispose();

}
Catch {

 }

