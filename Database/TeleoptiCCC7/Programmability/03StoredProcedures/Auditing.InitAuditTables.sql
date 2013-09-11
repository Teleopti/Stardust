IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auditing].[InitAuditTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE  [Auditing].[InitAuditTables] 
GO

CREATE PROCEDURE [Auditing].[InitAuditTables] 
--@DaysBack int = 99 --Should ideally come from the Options page!
AS
/*
exec [Auditing].[InitAuditTables] 
*/
BEGIN
set nocount on

--remove all old revisions (cascading deletes to audit tables)
delete from Auditing.Revision
with (tablock, updlock) --just to be sure there will be table lock


--Insert revisions first, one per user and time (not 100% correct but anyway)
insert into Auditing.Revision (ModifiedAt, ModifiedBy)
	select a.UpdatedOn, a.UpdatedBy
	from (
		select pa.UpdatedOn as UpdatedOn, pa.UpdatedBy as UpdatedBy
		from PersonAssignment pa with (tablock, updlock)
		inner join Scenario s 
			on s.Id = pa.Scenario and s.DefaultScenario = 1
		
		union
		
		select pabs.UpdatedOn as UpdatedOn, pabs.UpdatedBy as UpdatedBy
		from PersonAbsence pabs with (tablock, updlock)
		inner join Scenario s 
			on s.Id = pabs.Scenario and s.DefaultScenario = 1
	) as a
	group by a.UpdatedOn, a.UpdatedBy
	order by a.UpdatedOn, a.UpdatedBy

--now we can hook all existing data to a version by joining the revision table
--personassignment
	insert into Auditing.PersonAssignment_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[Version]
		,[Person]
		,[Scenario]
		,[ShiftCategory]
		,[DayOffTemplate]
		,[Date])
	 select p.Id
		,ar.Id
		,0
		,p.Version
		,[Person]
		,[Scenario]
		,p.ShiftCategory
		,p.DayOffTemplate
		,p.[Date]
	from dbo.PersonAssignment p
	inner join Auditing.Revision ar on p.UpdatedBy = ar.ModifiedBy and p.UpdatedOn = ar.ModifiedAt
	inner join Scenario s on s.Id = p.Scenario and s.DefaultScenario = 1
	
	--ShiftLayer
	insert into Auditing.ShiftLayer_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[Minimum]
		,[Maximum]
		,[OrderIndex]
		,[Payload]
		,[DefinitionSet]
		,[LayerType]
		,[Parent])
	 select al.Id,
		rev,
		0,
		al.Minimum,
		al.Maximum,
		al.OrderIndex,
		al.Payload,
		al.DefinitionSet,
		al.LayerType,
		al.Parent
	 from dbo.ShiftLayer al
	 inner join Auditing.PersonAssignment_AUD pa_aud
	 on al.Parent = pa_aud.Id
	 
	 --personabsence
	 insert into Auditing.PersonAbsence_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[Version]
		,[LastChange]
		,[Minimum]
		,[Maximum]
		,[Person]
		,[Scenario]
		,[Payload])
	 select p.Id,
		ar.Id,
		0,
		p.Version,
		LastChange,
		Minimum,
		Maximum,
		Person,
		Scenario,
		Payload
	 from dbo.PersonAbsence p
	inner join Auditing.Revision ar on p.UpdatedBy = ar.ModifiedBy and p.UpdatedOn = ar.ModifiedAt
	inner join Scenario s on s.Id = p.Scenario and s.DefaultScenario = 1

END
GO

