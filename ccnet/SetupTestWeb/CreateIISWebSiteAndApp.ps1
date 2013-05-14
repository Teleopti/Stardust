param($PhysicalPath,$DefaultSite,$SiteName,$AppName,$AppPoolName)
import-module webadministration
import-module .\MachineKey.psm1.ps1

#Step 1: Create New Directories
#We use the New-Item cmdlet to create four new file system directories. Execute the following commands (use 'md' instead of New-Item if you don't want to specify the -type parameter):
New-Item $PhysicalPath -type Directory

#Step 2: Copy Content
#Now let's write some simple html content to these directories:
Set-Content $PhysicalPath\Default.htm "This is a dummy Default Page"

#Step 3: Create New Application Pool
#Create the new Application Pool for the new site
New-Item IIS:\AppPools\$AppPoolName
Set-ItemProperty IIS:\AppPools\$AppPoolName -name "ManagedRuntimeVersion" -Value "v4.0"

#Step 4: Create New Sites, Web Applications and Virtual Directories and Assign to Application Pool
#TestApp is assigned to the 'Default Web Site' (port 80)
New-Item IIS:\Sites\$DefaultSite\$AppName -physicalPath $PhysicalPath -type Application
Set-ItemProperty IIS:\sites\$DefaultSite\$AppName -name applicationPool -value $AppPoolName