$port = 9000
for($i=0
     $i -le 6
     $i++){
		$global:value = (netsh http show urlacl url=http://+:$port/)
		if(!($global:value  -like '*http://+:' + $port + '/*'))
		{
			$global:value = (netsh http add urlacl url=http://+:$port/ user=Everyone listen=yes)
			if($global:value  -like '*Error: 5*')
			{
				Write-host 'You must run this as administrator to add permissions to listen on ports'
				start-sleep -seconds 5
				Exit   
			}			
		}
		$port++
     }

$port = 9050
for($i=0
     $i -le 5
     $i++){
		$global:value = (netsh http show urlacl url=http://+:$port/)
		if(!($global:value  -like '*http://+:' + $port + '/*'))
		{
			$global:value = (netsh http add urlacl url=http://+:$port/ user=Everyone listen=yes)
			if($global:value  -like '*Error: 5*')
			{
				Write-host 'You must run this as administrator to add permissions to listen on ports'
				start-sleep -seconds 5
				Exit   
			}			
		}
		$port++
     }
	 
$port = 9100
if(!($global:value  -like '*http://+:' + $port + '/*'))
{
	$global:value = (netsh http add urlacl url=http://+:$port/ user=Everyone listen=yes)
	if($global:value  -like '*Error: 5*')
	{
		Write-host 'You must run this as administrator to add permissions to listen on ports'
		start-sleep -seconds 5
		Exit   
	}			
}
