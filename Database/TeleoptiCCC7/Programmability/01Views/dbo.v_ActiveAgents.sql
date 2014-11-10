IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_ActiveAgents]'))
DROP VIEW [dbo].[v_ActiveAgents]
GO
--select * from [dbo].[v_ActiveAgents]
CREATE View [dbo].[v_ActiveAgents]
WITH ENCRYPTION
AS
 
select count(p.Id) as ActiveAgents
from dbo.Person p
inner join (
	select distinct
		pbu.Parent,
		pbu.BusinessUnit
	from dbo.v_PersonPeriodTeamSiteBu pbu) BU_Person --Distinct list of person and BU they belong to
	on BU_Person.Parent = p.Id
inner join dbo.BusinessUnit bu
	on BU_Person.BusinessUnit = bu.Id
where  p.IsDeleted = 0 --Only use active agents
and bu.IsDeleted = 0 --Only use active BUs
and (p.TerminalDate is null 
	or p.TerminalDate >= DATEADD(D, 0, DATEDIFF(D, 0, GETDATE())))
and (exists
		(
		select personperi1_.Id
        from   PersonPeriod personperi1_
        where  personperi1_.Parent = p.Id
		and personperi1_.StartDate < DATEADD(D, 0, DATEDIFF(D, 0, GETDATE()))
		)
	)
and (exists
		(
			select
				personassi2_.Id,
				scenario3_.Id
			from   PersonAssignment personassi2_
			inner join Scenario scenario3_
				on personassi2_.Scenario = scenario3_.Id
			where  exists
				(
				select 1
                from   Person p
                where  personassi2_.Person = p.Id
                and p.IsDeleted = 0
				)
			and personassi2_.Person = p.Id
			and scenario3_.DefaultScenario = 1
		)
	)

GO
