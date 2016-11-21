begin
	declare @minutes int
	select @minutes =value from 	dbo.RequestStrategySettings
	where setting = 'AbsenceFarFutureTime'
	if @minutes = 60
		update dbo.RequestStrategySettings
		set value = 15
		where Setting = 'AbsenceFarFutureTime' 

	select @minutes =value from 	dbo.RequestStrategySettings
	where setting = 'AbsenceNearFutureTime'
	if @minutes = 20
		update dbo.RequestStrategySettings
		set value = 5
		where Setting = 'AbsenceNearFutureTime' 
end;
