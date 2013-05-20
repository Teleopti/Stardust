
----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE dbo.PersonAssignment
ADD TheDate datetime
GO

update dbo.PersonAssignment
set TheDate ='1800-1-1'
GO

ALTER TABLE dbo.PersonAssignment
ALTER COLUMN TheDate datetime not null
GO

----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment audit table. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE Auditing.PersonAssignment_AUD
ADD TheDate datetime
GO

update Auditing.PersonAssignment_AUD
set TheDate ='1800-1-1'
GO


----------------  
--Name: Kunning Mao
--Date: 2013-05-20
--Desc: Empty read models in order to re-fill it with compressed shifts
----------------  
TRUNCATE TABLE [ReadModel].[PersonScheduleDay]
GO

ALTER TABLE [ReadModel].[PersonScheduleDay] ALTER COLUMN [Shift] nvarchar(2000)
GO