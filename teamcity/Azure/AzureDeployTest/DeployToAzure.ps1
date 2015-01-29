Param(
  [string]$path,
  [string]$subscription, 			#this the name from your .publishsettings file
  [string]$service,					#this is the name of the cloud service
  [string]$package,					#Azure package file
  [string]$configuration,			#Azure config file
  [string]$slot,					#production or staging
  [string]$publishSettingsFile		#publishsettings file
  )

$storageAccount = "teleopticcc7"	#Our standard Storage account
$timeStampFormat = "g"
$deploymentLabel = $package
$deploymentLabel = $deploymentLabel.Replace("Azure-", "")    
$deploymentLabel = $deploymentLabel.Replace(".cspkg", "")  

Write-Output ""
Write-Output "Starting to upload new Azure Deployment"
Write-Output ""
Write-Output "Subscription: $subscription"
Write-Output "Cloud Service: $service"
Write-Output "Deployment Label: $deploymentLabel"
Write-Output ""

# Wait for input before closing.
Write-Host "Press any key to continue..."
$x = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp")
 
Import-AzurePublishSettingsFile $publishSettingsFile
 
function Publish(){
	$deployment = Get-AzureDeployment -ServiceName $service -Slot $slot -ErrorVariable a -ErrorAction silentlycontinue 
	 
	if ($a[0] -ne $null) 
	{
		Write-Output "$(Get-Date -f $timeStampFormat) - No deployment is detected. Creating a new deployment. "
		CreateNewDeployment
	}
	else
	{
		Write-Output "$(Get-Date -f $timeStampFormat) - Removing Deployment: In progress"
		Remove-AzureDeployment -ServiceName $service -Slot $slot -Force
		Write-Output "$(Get-Date -f $timeStampFormat) - Removing Deployment: Done"
		
		CreateNewDeployment
	}
 
}
 
function CreateNewDeployment()
{
	Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: In progress"
 
	$opstat = New-AzureDeployment -Slot $slot -Package $package -Configuration $configuration -label $deploymentLabel -ServiceName $service
 
	$completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
	$completeDeploymentID = $completeDeployment.deploymentid
 
	Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: Complete, Deployment ID: $completeDeploymentID"
	Write-Output "$(Get-Date -f $timeStampFormat) - Verify further progress in Azure Portal, https://manage.windowsazure.com"
}

Publish

# Wait for input before closing.
Write-Host "Press any key to continue..."
$x = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp")