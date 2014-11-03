IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriodWithEndDate]'))
DROP VIEW [dbo].[PersonPeriodWithEndDate]
GO

CREATE VIEW [dbo].[PersonPeriodWithEndDate]
AS

select pp.*,
	isnull(
	(
		select case when isnull(p.terminaldate,'20591231 00:00') < min(dateadd(day,-1,isnull(pp2.StartDate,'20591231 00:00'))) then isnull(p.terminaldate,'20591231 00:00')
					else min(dateadd(day,-1,isnull(pp2.StartDate,'20591231 00:00'))) end
		from personperiod pp2
		where pp2.parent = pp.parent
		and pp2.StartDate > pp.StartDate
	)
		,isnull(p.terminaldate,'20591231 00:00')) as EndDate
from personperiod pp
inner join person p on pp.parent = p.id and p.IsDeleted = 0
where pp.StartDate <= isnull(p.terminaldate,'20591231 00:00')
GO