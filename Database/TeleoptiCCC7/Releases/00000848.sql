WITH cte as(
	SELECT Parent, StartDate, ROW_NUMBER() OVER (PARTITION BY Parent, StartDate ORDER BY Parent) RN
	FROM   PersonPeriod)
DELETE FROM cte WHERE RN>1
GO
	
ALTER TABLE [dbo].[PersonPeriod] ADD CONSTRAINT UQ_PersonPeriod UNIQUE
(
	[Parent] ASC,
	[StartDate] ASC
)
GO
