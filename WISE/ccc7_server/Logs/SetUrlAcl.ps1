$numberOfNodes  = Get-WmiObject -Class Win32_ComputerSystem -ComputerName . | Select-Object -Property NumberOfLogicalProcessors
$port = 14100
for($i=1 
    $i -le $numberOfNodes.NumberOfLogicalProcessors
    $i++){
            $global:value = (netsh http show urlacl url=http://+:$port/)
            if(!($global:value  -like '*http://+:' + $port + '/*'))
            {
		        $global:value = (netsh http add urlacl url=http://+:$port/ user=Everyone listen=yes)
	        }
            $port++
       }