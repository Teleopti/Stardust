IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateGroupingReadModelGroupPage]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdateGroupingReadModelGroupPage]
GO

-- =============================================
-- Author:		<Asad Mirza>
-- Create date: <2012-05-18>
-- Description:	<This procedure will recreate the custom and default hierarchy >
-- =============================================

CREATE PROCEDURE ReadModel.UpdateGroupingReadModelGroupPage
@ids nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	CREATE TABLE #Gids(page uniqueidentifier)
    INSERT INTO #Gids SELECT * FROM SplitStringString(@ids) 
	
	DELETE FROM [ReadModel].[GroupingReadOnly] WHERE PageId  in(SELECT * FROM #Gids)


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
	and gp.Id in (SELECT * FROM #Gids)
	
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
	LEFT JOIN PersonPeriod pp
		ON pp.parent=p.id
	LEFT JOIN team t
		ON t.id=pp.team
END
GO
--=============
--initial load
--=============
declare @AllGroupings nvarchar(max)
SET @AllGroupings =
	(
	select Cast(Id as char(36))+','
	from dbo.grouppage 
	for xml path('')
	)
exec [ReadModel].[UpdateGroupingReadModelGroupPage] @ids=@AllGroupings
GO