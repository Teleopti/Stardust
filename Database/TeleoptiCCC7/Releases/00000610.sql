ALTER TABLE dbo.PersonPeriod ADD EndDate DateTime NULL
GO

UPDATE dbo.PersonPeriod SET EndDate = isnull(
	(
		select case when isnull(p.terminaldate,'20591231 00:00') < min(dateadd(day,-1,isnull(pp2.StartDate,'20591231 00:00'))) then isnull(p.terminaldate,'20591231 00:00')
					else min(dateadd(day,-1,isnull(pp2.StartDate,'20591231 00:00'))) end
		from dbo.personperiod pp2
		where pp2.parent = pp.parent
		and pp2.StartDate > pp.StartDate
	)
		,isnull(p.terminaldate,'20591231 00:00')) FROM dbo.PersonPeriod pp 
inner join dbo.person p on pp.parent = p.id
where pp.StartDate <= isnull(p.terminaldate,'20591231 00:00')
GO
