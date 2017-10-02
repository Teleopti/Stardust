Param (

    $CloudServiceName = 'teleoptirnd',          #In Parameter ex: teleoptirnd
	$AzurePkgSize = "Standard_D1_v2"			# Pkg size to use: Standard_D1_v2 or Standard_D2_v2
    
)

#$Here = $PSScriptRoot

$VersionedName = Get-childitem "$PSScriptRoot\AzureRelease\*$AzurePkgSize.cspkg"
$VersionedName = $VersionedName.Name.ToString()
$AzurePackagePath = "$PSScriptRoot\AzureRelease"

#Fixed Variables
$subscription = 'Teleopti CCC Azure'			                #this the name from your .publishsettings file
$subscriptionID = "9020de4a-13f8-465b-a3ae-995caf390fb8"                        #this is the subscription id of the cloud service
$service	  = "$CloudServiceName"			                                    #this is the name of the cloud service
$package	  = "$AzurePackagePath\$VersionedName"			                	#Azure package file
$configuration	= "$AzurePackagePath\$CloudServiceName"	+ '.cscfg'              #Azure config file
$slot		   = 'production'		                                            #production or staging
$publishSettingsFile  = "$PSScriptRoot\AzureDemo.publishsettings"	        	#publishsettings file

Write-Host '***************************************************************'
Write-Host "CloudServiceName =	$CloudServiceName"
Write-Host "AzurePackagePath =	$AzurePackagePath"
Write-Host "VersionedName	 =	$VersionedName"
Write-Host "PathToHere		 =	$PSScriptRoot"
Write-Host '***************************************************************'


$storageAccount = "teleopticcc7"	#Our standard Storage account
$timeStampFormat = "g"
$deploymentLabel = [io.path]::GetFileNameWithoutExtension($package)
$deploymentLabel = $deploymentLabel.Replace("Azure-", "")  

# Importing & Setting Azure subscription
Write-Output "Importing Azure subscription file"   
Import-AzurePublishSettingsFile $publishSettingsFile
Set-AzureSubscription -CurrentStorageAccount $storageAccount -SubscriptionName $subscription

Write-Output ''
Write-Output "$(Get-Date -f $timeStampFormat) - Azure Cloud Service deploy script started."
Write-Output "$(Get-Date -f $timeStampFormat) - Preparing deployment of version: $deploymentLabel for $service with Subscription ID $subscriptionID."
Write-Output ''
 
function StartInstances()
{
    write-progress -id 4 -activity "Starting Instances" -status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Starting Instances: In progress"

    $deployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $runstatus = $deployment.Status

    if ($runstatus -ne 'Running')
    {
        $run = Set-AzureDeployment -Slot $slot -ServiceName $service -Status Running
    }
    $deployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $oldStatusStr = @("") * $deployment.RoleInstanceList.Count

    while (-not(AllInstancesRunning($deployment.RoleInstanceList)))
    {
        $i = 1
        foreach ($roleInstance in $deployment.RoleInstanceList)
        {
            $instanceName = $roleInstance.InstanceName
            $instanceStatus = $roleInstance.InstanceStatus

            if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
            {
                $oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
                Write-Output "$(Get-Date -f $timeStampFormat) - Starting Instance '$instanceName': $instanceStatus"
            }

            write-progress -id (4 + $i) -activity "Starting Instance '$instanceName'" -status "$instanceStatus"
            $i = $i + 1
        }

        sleep -Seconds 1

        $deployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    }

    $i = 1
    foreach ($roleInstance in $deployment.RoleInstanceList)
    {
        $instanceName = $roleInstance.InstanceName
        $instanceStatus = $roleInstance.InstanceStatus

        if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
        {
            $oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
            Write-Output "$(Get-Date -f $timeStampFormat) - Starting Instance '$instanceName': $instanceStatus"
        }

        $i = $i + 1
    }

    $deployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $opstat = $deployment.Status

    write-progress -id 4 -activity "Starting Instances" -completed -status $opstat
    Write-Output "$(Get-Date -f $timeStampFormat) - Starting Instances: $opstat"
}

function AllInstancesRunning($roleInstanceList)
{
    foreach ($roleInstance in $roleInstanceList)
    {
        if ($roleInstance.InstanceStatus -ne "ReadyRole")
        {
            return $false
        }
    }

    return $true
}

function CreateNewDeployment()
{
    
    Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: In progress"
 
	$opstat = New-AzureDeployment -Slot $slot -Package $package -Configuration $configuration -label $deploymentLabel -ServiceName $service
    
    StartInstances

	$completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
	$completeDeploymentID = $completeDeployment.deploymentid
    $completeDeploymentUrl = $completeDeployment.url.AbsoluteUri
 
	Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: Complete, Deployment ID: $completeDeploymentID"
	Write-Output "$(Get-Date -f $timeStampFormat) - Verify further progress in Azure Portal, https://portal.azure.com"
    
    Write-Output "$(Get-Date -f $timeStampFormat) - Created Cloud Service with URL $completeDeploymentUrl."
    Write-Output "$(Get-Date -f $timeStampFormat) - GoDaddy address to browse website: "
    Write-Output "$(Get-Date -f $timeStampFormat) - Azure Cloud Service deploy script finished."
}

function Publish(){
	$deployment = Get-AzureDeployment -ServiceName $service -Slot $slot -ErrorVariable a -ErrorAction silentlycontinue 
	 
	if ($a[0] -ne $null) 
	{
		Write-Output "$(Get-Date -f $timeStampFormat) - No deployment is detected. Creating a new deployment. "
		CreateNewDeployment
	}
	else
	{
		Write-Progress -id 2 -activity "Deleting Deployment" -Status "In progress"
        Write-Output "$(Get-Date -f $timeStampFormat) - Removing Deployment: In progress"
    
    	$removeDeployment = Remove-AzureDeployment -ServiceName $service -Slot $slot -Force
		
        Write-Progress -id 2 -activity "Deleting Deployment: Complete" -completed -Status $removeDeployment
        Write-Output "$(Get-Date -f $timeStampFormat) - Removing Deployment: Done"
		
		CreateNewDeployment
	}
 
}

Try
{
Publish
}
Catch [Exception]
{
    $ErrorMessage = $_.Exception.Message
	Throw $ErrorMessage
}