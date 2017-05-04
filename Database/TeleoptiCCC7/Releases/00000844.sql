ALTER TABLE stardust.jobqueue
ADD LockTimeStamp datetime;
GO

exec ('update Stardust.JobQueue set lockTimestamp = DATEADD(minute,10, GETDATE()) where tagged = 0')
GO

alter table stardust.jobqueue
drop column tagged
Go