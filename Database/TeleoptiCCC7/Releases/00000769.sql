update dbo.RequestStrategySettings
set value = 15
where Setting = 'AbsenceFarFutureTime' and value = 60

update dbo.RequestStrategySettings
set value = 5
where Setting = 'AbsenceNearFutureTime' and value = 20
