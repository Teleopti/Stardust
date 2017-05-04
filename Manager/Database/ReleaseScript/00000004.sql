USE [$(DBNAME)]
GO

ALTER TABLE stardust.jobqueue
ADD lockTimestamp datetime;

exec ('update Stardust.JobQueue set lockTimestamp = DATEADD(minute,10, GETDATE()) where tagged = 0')

alter table stardust.jobqueue
drop column tagged