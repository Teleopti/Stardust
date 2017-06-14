CREATE TABLE #pp (Id uniqueidentifier)
GO

WITH cte as(
	SELECT Id, Parent, StartDate, ROW_NUMBER() OVER (PARTITION BY Parent, StartDate ORDER BY Parent) RN
	FROM   PersonPeriod)
INSERT INTO #pp SELECT Id FROM cte WHERE RN>1

delete ExternalLogOnCollection
from ExternalLogOnCollection elc
inner join #pp pp on elc.personperiod = pp.Id 

delete PersonSkill
from PersonSkill ps
inner join #pp pp on ps.parent = pp.Id 

delete PersonPeriod
from PersonPeriod
inner join #pp pp on PersonPeriod.Id = pp.Id
GO
	
ALTER TABLE [dbo].[PersonPeriod] ADD CONSTRAINT UQ_PersonPeriod UNIQUE
(
	[Parent] ASC,
	[StartDate] ASC
)
GO
