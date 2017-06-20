delete from dbo.RequestStrategySettings where Setting = 'UpdateResourceReadModelIntervalMinutes'
delete from dbo.RequestStrategySettings where Setting = 'BulkRequestTimeoutMinutes'
delete from dbo.RequestStrategySettings where Setting = 'AbsenceNearFuture'
delete from dbo.RequestStrategySettings where Setting = 'AbsenceFarFutureTime'
delete from dbo.RequestStrategySettings where Setting = 'AbsenceNearFutureTime'


insert into dbo.RequestStrategySettings values ('AbsenceRequestBulkFrequencyMinutes', 10) 

