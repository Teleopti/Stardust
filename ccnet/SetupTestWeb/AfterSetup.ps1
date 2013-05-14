param($PhysicalPath,$DefaultSite,$SiteName,$AppName,$AppPoolName)
import-module webadministration
import-module .\MachineKey.psm1.ps1

#Step 5: Set validation and decryption keys
Set-MachineKey ("IIS:\sites\{0}\{1}" -f $DefaultSite, $AppName)