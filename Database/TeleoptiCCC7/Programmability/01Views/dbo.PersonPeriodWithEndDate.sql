IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriodWithEndDate]'))
DROP VIEW [dbo].[PersonPeriodWithEndDate]
GO

CREATE VIEW [dbo].[PersonPeriodWithEndDate]
AS

select pp.*,
	(
		select isnull(min(dateadd(day,-1,pp2.StartDate)),'20591231 00:00')
		from personperiod pp2
		where pp2.parent = pp.parent
		and pp2.StartDate > pp.StartDate
	) as EndDate
from personperiod pp
inner join person p on pp.parent = p.id and p.IsDeleted = 0
GO