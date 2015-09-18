IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateGroupingReadModel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdateGroupingReadModel]
GO

-- =============================================
-- Author:		RobinK
-- Create date: 2011-09-26
-- Description:	Updates the read model for groupings
-- Change:		AF 2011-10-27 Error because string or binary data would be truncated
--				AF 2011-11-22 Had to format the sql to read it :-)
--				DJ 2011-12-08 adding view for PersonPeriod including EndDate instead on row based dateadd()
--				AF 2012-01-18 Display agents on deleted sites, teams, contracts, part time percentages, contract schedules, shift bags, BUT not skills. Do not want to see deleted skills at all.
--				Ola 2012-05-16 Added parameter to just update changed persons
--				AF 20130806 Execute as owner, otherwise we are not allowed to do truncate table. 
-- =============================================
-- exec ReadModel.UpdateGroupingReadModel 'B0C67CB1-1C4F-4047-8DC1-9EF500DC79A6, 2AE730A0-5AF7-49B7-9498-9EF500DC79A6'
CREATE PROCEDURE ReadModel.UpdateGroupingReadModel
@persons nvarchar(max)
WITH EXECUTE AS OWNER
AS
BEGIN
	 SET NOCOUNT ON;

CREATE TABLE #ids(person uniqueidentifier)

IF @persons = '00000000-0000-0000-0000-000000000000'  --"EveryBody"
--Flush and re-load everybody
BEGIN
	TRUNCATE TABLE [ReadModel].[GroupingReadOnly]
	INSERT INTO #ids SELECT Id FROM Person WHERE IsDeleted = 0
END
ELSE
--Flush and re-load only PersonIds in string
BEGIN
	INSERT INTO #ids SELECT * FROM SplitStringString(@persons) 
	DELETE FROM [ReadModel].[GroupingReadOnly] WHERE PersonId in(SELECT * FROM #ids);
	
	--We found one case where duplicates where present. Let's remove them first.
	WITH cte as(
	  SELECT Person,ROW_NUMBER() OVER (PARTITION BY Person ORDER BY Person) RN
	  FROM   #ids)
	DELETE FROM cte WHERE RN>1
END 

    declare @mainId uniqueidentifier
	set @mainId='6CE00B41-0722-4B36-91DD-0A3B63C545CF'
	
	declare @contractId uniqueidentifier
	set @contractId='0CE00B41-0722-4B36-91DD-0A3B63C545CF'

	declare @contractScheduleId uniqueidentifier
	set @contractScheduleId='1CE00B41-0722-4B36-91DD-0A3B63C545CF'

	declare @partTimePercentageId uniqueidentifier
	set @partTimePercentageId='2CE00B41-0722-4B36-91DD-0A3B63C545CF'

	declare @ruleSetBagId uniqueidentifier
	set @ruleSetBagId='3CE00B41-0722-4B36-91DD-0A3B63C545CF'

	declare @skillId uniqueidentifier
	set @skillId='4CE00B41-0722-4B36-91DD-0A3B63C545CF'

	--declare @noteId uniqueidentifier
	--set @noteId='5CE00B41-0722-4B36-91DD-0A3B63C545CF'
	
	--Insert people from business hierarchy
	--elapsed time = 242 ms.
	INSERT INTO ReadModel.groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) 
	select	p.id,
			isnull(pp.startdate,'1900-01-01') as startdate,
			t.id teamid,
			s.id siteid,
			s.businessunit as businessunitid,
			t.id as groupid,s.name +'/'+t.name as groupname, --50+50+1 chars can never exceed 101. Column now has 200 so no issue.
			p.firstname,
			p.lastname,
			@mainId as pageid,
			'xxMain' as pagename,
			p.employmentnumber,
			pp.EndDate,
			p.terminaldate as leavingdate
	from site s 
	inner join team t on t.site=s.id
	inner join PersonPeriod pp on pp.team=t.id 
	inner join person p on p.id=pp.parent and p.isdeleted=0 
	inner join #ids i on i.person = p.Id
	
	--Insert people from contract
	INSERT INTO ReadModel.groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate)
	select p.id,isnull(pp.startdate,'1900-01-01') as startdate,
	t.id teamid,
	t.site siteid,
	c.businessunit as businessunitid,
	c.id as groupid,
	c.name as groupname,
	p.firstname,
	p.lastname,
	@contractId as pageid,
	'xxContract' as pagename,
	p.employmentnumber,
	pp.EndDate,
	p.terminaldate as leavingdate 
	from team t 
	inner join PersonPeriod pp on pp.team=t.id 
	inner join contract c on pp.contract=c.id 
	inner join person p on p.id=pp.parent and p.isdeleted=0 
	inner join #ids i on i.person = p.Id

	--Insert people from part time percentage
	INSERT INTO ReadModel.groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) 
	select p.id,isnull(pp.startdate,'1900-01-01') as startdate,
	t.id teamid,
	t.site siteid,
	c.businessunit as businessunitid,
	c.id as groupid,
	c.name as groupname,
	p.firstname,
	p.lastname,
	@partTimePercentageId as pageid,
	'xxPartTimePercentage' as pagename,
	p.employmentnumber,
	pp.EndDate,
	p.terminaldate as leavingdate 
	from team t 
	inner join PersonPeriod pp on pp.team=t.id 
	inner join parttimepercentage c on pp.parttimepercentage=c.id 
	inner join person p on p.id=pp.parent and p.isdeleted=0 
	inner join #ids i on i.person = p.Id
	
	--Insert people from contract schedule
	INSERT INTO ReadModel.groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) 
	select p.id,isnull(pp.startdate,'1900-01-01') as startdate,
	t.id teamid,
	t.site siteid,
	c.businessunit as businessunitid,
	c.id as groupid,
	c.name as groupname,
	p.firstname,p.lastname,
	@contractScheduleId as pageid,
	'xxContractSchedule' as pagename,
	p.employmentnumber,
	pp.EndDate,
	p.terminaldate as leavingdate 
	from team t 
	inner join PersonPeriod pp on pp.team=t.id 
	inner join contractschedule c on pp.contractschedule=c.id 
	inner join person p on p.id=pp.parent and p.isdeleted=0 
	inner join #ids i on i.person = p.Id
	
	--Insert people from rule set bag
	INSERT INTO ReadModel.groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) 
	select p.id,isnull(pp.startdate,'1900-01-01') as startdate,
	t.id teamid,
	t.site siteid,
	c.businessunit as businessunitid,
	c.id as groupid,
	c.name as groupname,
	p.firstname,
	p.lastname,
	@ruleSetBagId as pageid,
	'xxRuleSetBag' as pagename,
	p.employmentnumber,
	pp.EndDate,
	p.terminaldate as leavingdate 
	from team t 
	inner join PersonPeriod pp on pp.team=t.id 
	inner join rulesetbag c on pp.rulesetbag=c.id 
	inner join person p on p.id=pp.parent and p.isdeleted=0 
	inner join #ids i on i.person = p.Id
	
	--Insert people from skill
	INSERT INTO ReadModel.groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) 
	select
		p.id,
		isnull(pp.startdate,'1900-01-01') as startdate,
		t.id teamid,
		t.site siteid,
		skill.businessunit as businessunitid,
		skill.id as groupid,
		skill.name as groupname,
		p.firstname,p.lastname,
		@skillId as pageid,
		'xxSkill' as pagename,
		p.employmentnumber,
		pp.EndDate,
		p.terminaldate as leavingdate 
	from team t 
	inner join PersonPeriod pp
		on	pp.team=t.id
	inner join personskill c
		on	pp.id=c.parent
		and c.active=1 
	inner join skill
		on	skill.id=c.skill and skill.isdeleted=0
	inner join person p on p.id=pp.parent and p.isdeleted=0 
	inner join #ids i on i.person = p.Id

	CREATE TABLE #groupsForSecondCTE
	(
		groupid uniqueidentifier not null,
		queryid uniqueidentifier not null,
		name nvarchar(200) null,
		pagename nvarchar(50) null,
		pageid uniqueidentifier not null,
		businessunit uniqueidentifier not null,
		parent uniqueidentifier null
	);

	CREATE TABLE #tempresult
	(
		groupid uniqueidentifier not null,
		queryid uniqueidentifier not null,
		name nvarchar(200) null,
		pagename nvarchar(50) null,
		pageid uniqueidentifier not null,
		businessunit uniqueidentifier not null,
		parent uniqueidentifier null
	);

	--Flatten out the hierarchy of custom groups
	WITH grouping1 (GroupId,queryid,name,pagename,pageid,businessunit,parent)
	AS
	(
	SELECT
		pgb.id,
		pgb.id,
		CAST(pgb.name AS NVARCHAR(200)),
		gp.name pagename,
		gp.id pageid,
		gp.businessunit,
		CAST(NULL AS UNIQUEIDENTIFIER)
	FROM persongroupbase pgb
	INNER JOIN rootpersongroup rpg
		ON rpg.persongroupbase=pgb.id
	INNER JOIN grouppage gp
		ON gp.id=rpg.parent
	WHERE gp.isdeleted=0
	
	UNION ALL
	
	SELECT
		pgb.id,
		pgb.id,
		CAST(g1.name+'\'+pgb.name AS NVARCHAR(200)),
		g1.pagename,
		g1.pageid,
		g1.businessunit,
		cpg.parent
	FROM persongroupbase pgb
	INNER JOIN childpersongroup cpg
		ON cpg.persongroupbase=pgb.id
	INNER JOIN grouping1 g1
		ON g1.groupid=cpg.parent
	)
	INSERT INTO #groupsForSecondCTE SELECT * FROM grouping1;
	INSERT INTO #tempresult SELECT * FROM #groupsForSecondCTE;

	--Enable people to pop up both in MainGroup\SubGroup1 and MainGroup
	WITH grouping2 (groupId,queryid,name,pagename,pageid,businessunit,parent)
	AS
	(
	SELECT t2.groupid,t1.groupid,t2.name,t1.pagename,t1.pageid,t1.businessunit,t1.parent FROM #groupsForSecondCTE t1 INNER JOIN #groupsForSecondCTE t2 ON t2.queryid=t1.parent WHERE t1.parent IS NOT NULL
	UNION ALL
	SELECT p.groupid,c.groupid,p.name,c.pagename,c.pageid,c.businessunit,c.parent FROM #groupsForSecondCTE c INNER JOIN grouping2 p ON p.queryid=c.parent
	)

	INSERT INTO #tempresult SELECT * FROM grouping2

	INSERT INTO ReadModel.groupingreadonly (pageid,pagename,businessunitid,groupid,groupname,firstname,lastname,employmentnumber,personid,teamid,siteid,startdate,enddate,leavingdate)
	SELECT pageid,pagename,businessunit,groupid,tr.name groupname,p.firstname,p.lastname,p.employmentnumber,p.id personid,pp.team teamid,t.site siteid,isnull(pp.startdate,'1900-01-01') startdate,pp.EndDate as enddate,p.terminaldate leavingdate
	FROM #tempresult tr
	INNER JOIN persongroup pg
		ON pg.persongroup=tr.queryid
	INNER JOIN person p
		ON p.id=pg.person AND p.isdeleted=0
	INNER JOIN #ids i
		ON i.person = p.Id
	LEFT JOIN PersonPeriod pp
		ON pp.parent=p.id
	LEFT JOIN team t
		ON t.id=pp.team
END
GO

--=================
--Finally, when DBManager applies this SP also execute the SP
--=================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateGroupingReadModel]') AND type in (N'P', N'PC')) 
EXEC [ReadModel].[UpdateGroupingReadModel] '00000000-0000-0000-0000-000000000000'
GO