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
		from PersonAssignment pa
		inner join Scenario s on s.Id = pa.Scenario and s.DefaultScenario = 1
		--where pa.Minimum > dateadd(day,-1*@DaysBack,GETDATE())
		union
		select pabs.UpdatedOn as UpdatedOn, pabs.UpdatedBy as UpdatedBy
		from PersonAbsence pabs
		inner join Scenario s on s.Id = pabs.Scenario and s.DefaultScenario = 1
		--where pabs.Minimum > dateadd(day,-1*@DaysBack,GETDATE())
		union
		select pd.UpdatedOn as UpdatedOn, pd.UpdatedBy as UpdatedBy
		from PersonDayOff pd
		with (tablock, updlock)		inner join Scenario s on s.Id = pd.Scenario and s.DefaultScenario = 1
		--where pd.Anchor > dateadd(day,-1*@DaysBack,GETDATE())
	) as a
	group by a.UpdatedOn, a.UpdatedBy
	order by a.UpdatedOn, a.UpdatedBy

--now we can hook all existing data to a version by joining the revision table
--personassignment
	insert into Auditing.PersonAssignment_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[Version]
		,[Minimum]
		,[Maximum]
		,[Person]
		,[Scenario]
		,[BusinessUnit])
	 select p.Id
		,ar.Id
		,0
		,p.Version
		,[Minimum]
		,[Maximum]
		,[Person]
		,[Scenario]
		,p.BusinessUnit
	from dbo.PersonAssignment p
	inner join Auditing.Revision ar on p.UpdatedBy = ar.ModifiedBy and p.UpdatedOn = ar.ModifiedAt
	inner join Scenario s on s.Id = p.Scenario and s.DefaultScenario = 1
	
		--mainshift
	insert into Auditing.MainShift_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[ShiftCategory])
	 select ms.Id
		,rev
		,0
		,ms.ShiftCategory
	 from dbo.MainShift ms
	 inner join Auditing.PersonAssignment_AUD pa_aud
	 on ms.Id = pa_aud.Id
	 

	 --mainshiftactivitylayer
	 insert into Auditing.MainShiftActivityLayer_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[Minimum]
		,[Maximum]
		,[OrderIndex]
		,[Payload]
		,[Parent])
	 select al.Id
		,rev
		,0
		,al.Minimum
		,al.Maximum
		,al.OrderIndex
		,al.Payload
		,al.Parent
	 from dbo.MainShiftActivityLayer al
	 inner join Auditing.MainShift_AUD ms_aud
	 on al.Parent = ms_aud.Id

	--overtimeshift
	insert into Auditing.OvertimeShift_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[OrderIndex]
		,[Parent])
	 select os.Id,
		rev,
		0
		,os.OrderIndex
		,os.Parent
	 from dbo.OvertimeShift os
	 inner join Auditing.PersonAssignment_AUD pa_aud
	 on os.Parent = pa_aud.Id
	
	--overtimeshift activity layer
	insert into Auditing.OvertimeShiftActivityLayer_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[Minimum]
		,[Maximum]
		,[OrderIndex]
		,[Payload]
		,[DefinitionSet]
		,[Parent])
	 select al.Id,
		rev,
		0,
		al.Minimum,
		al.Maximum,
		al.OrderIndex,
		al.Payload,
		al.DefinitionSet,
		al.Parent
	 from dbo.OvertimeShiftActivityLayer al
	 inner join Auditing.OvertimeShift_AUD os_aud
	 on al.Parent = os_aud.Id
	 
	 --personalshift
	 insert into Auditing.PersonalShift_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[OrderIndex]
		,[Parent])
	 select ps.Id,
		rev,
		0,
		ps.OrderIndex,
		ps.Parent
	 from dbo.PersonalShift ps
	 inner join Auditing.PersonAssignment_AUD pa_aud
	 on ps.Parent = pa_aud.Id
	 
	 --personalshiftactivitylayer
	 insert into Auditing.PersonalShiftActivityLayer_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[Minimum]
		,[Maximum]
		,[OrderIndex]
		,[Payload]
		,[Parent])
	 select al.Id,
		rev,
		0,
		al.Minimum,
		al.Maximum,
		al.OrderIndex,
		al.payLoad,
		al.Parent
	 from dbo.PersonalShiftActivityLayer al
	 inner join Auditing.PersonalShift_AUD ps_aud
	 on al.Parent = ps_aud.Id
	 
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
		,[Payload]
		,[BusinessUnit])
	 select p.Id,
		ar.Id,
		0,
		p.Version,
		LastChange,
		Minimum,
		Maximum,
		Person,
		Scenario,
		Payload,
		p.BusinessUnit
	 from dbo.PersonAbsence p
	inner join Auditing.Revision ar on p.UpdatedBy = ar.ModifiedBy and p.UpdatedOn = ar.ModifiedAt
	inner join Scenario s on s.Id = p.Scenario and s.DefaultScenario = 1

	--persondayoff
	insert into Auditing.PersonDayOff_AUD ([Id]
		,[REV]
		,[REVTYPE]
		,[Version]
		,[Anchor]
		,[TargetLength]
		,[Flexibility]
		,[DisplayColor]
		,[PayrollCode]
		,[Name]
		,[ShortName]
		,[Person]
		,[Scenario]
		,[BusinessUnit])
	 select p.Id,
		ar.Id,
		0,
		p.Version,
		Anchor,
		TargetLength,
		Flexibility,
		DisplayColor,
		PayrollCode,
		p.Name,
		p.ShortName,
		Person,
		Scenario,
		p.BusinessUnit
	 from dbo.PersonDayOff p
	inner join Auditing.Revision ar on p.UpdatedBy = ar.ModifiedBy and p.UpdatedOn = ar.ModifiedAt
	inner join Scenario s on s.Id = p.Scenario and s.DefaultScenario = 1

END
GO