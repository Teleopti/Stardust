Param(
  [string]$path,
  [string]$subscription, 			#this the name from your .publishsettings file
  [string]$service,					#this is the name of the cloud service
  [string]$package,					#Azure package file
  [string]$configuration,			#Azure config file
  [string]$slot,					#production or staging
  [string]$publishSettingsFile,		#publishsettings file
  [string]$deploymentLabel			#version to be deployed
  )

$storageAccount = "teleopticcc7"	
$timeStampFormat = "g"
 
Write-Output "Slot: $slot"
Write-Output "Subscription: $subscription"
Write-Output "Service: $service"
Write-Output "Storage Account: $storageAccount"
Write-Output "Package: $path$package"
Write-Output "Configuration: $path$configuration"
Write-Output "PublishSettingsFile: $publishSettingsFile"
 
Write-Output "Running Azure Imports"
Import-Module "C:\Program Files (x86)\Microsoft SDKs\Windows Azure\PowerShell\Azure\Azure.psd1"
Import-AzurePublishSettingsFile $publishSettingsFile
Set-AzureSubscription -CurrentStorageAccount $storageAccount -SubscriptionName $subscription
Set-AzureService -ServiceName $service -Label $deploymentLabel
 
function Publish(){
 $deployment = Get-AzureDeployment -ServiceName $service -Slot $slot -ErrorVariable a -ErrorAction silentlycontinue 
 
 if ($a[0] -ne $null) {
    Write-Output "$(Get-Date -f $timeStampFormat) - No deployment is detected. Creating a new deployment. "
 }
 
 if ($deployment.Name -ne $null) {
    #Update deployment inplace (usually faster, cheaper, won't destroy VIP)
    Write-Output "$(Get-Date -f $timeStampFormat) - Deployment exists in $servicename.  Upgrading deployment."
    UpgradeDeployment
 } else {
    CreateNewDeployment
 }
}
 
function CreateNewDeployment()
{
    write-progress -id 3 -activity "Creating New Deployment" -Status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: In progress"
 
    $opstat = New-AzureDeployment -Slot $slot -Package $package -Configuration $configuration -label $deploymentLabel -ServiceName $service
 
    $completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $completeDeploymentID = $completeDeployment.deploymentid
 
    write-progress -id 3 -activity "Creating New Deployment" -completed -Status "Complete"
    Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: Complete, Deployment ID: $completeDeploymentID"
}
 
function UpgradeDeployment()
{
    write-progress -id 3 -activity "Upgrading Deployment" -Status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Upgrading Deployment: In progress"
 
    # perform Update-Deployment
    $setdeployment = Set-AzureDeployment -Upgrade -Slot $slot -Package $package -Configuration $configuration -label $deploymentLabel -ServiceName $service -Force
 
    $completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $completeDeploymentID = $completeDeployment.deploymentid
 
    write-progress -id 3 -activity "Upgrading Deployment" -completed -Status "Complete"
    Write-Output "$(Get-Date -f $timeStampFormat) - Upgrading Deployment: Complete, Deployment ID: $completeDeploymentID"
}

Write-Output "Create Azure Deployment"
Publish

# Wait for input before closing.
# Write-Host "Press any key to continue..."
# $x = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp")