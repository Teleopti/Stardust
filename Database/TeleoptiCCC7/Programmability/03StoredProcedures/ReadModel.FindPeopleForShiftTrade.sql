
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[FindPeopleForShiftTrade]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[FindPeopleForShiftTrade]
GO
/****** Object:  StoredProcedure [ReadModel].[LoadPersonForScheduleSearch]    Script Date: 25/09/2016 11:02:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [ReadModel].[FindPeopleForShiftTrade]
	@scheduleDate smalldatetime,
	@groupIdList varchar(max),
	@businessUnitId uniqueidentifier,
	@name nvarchar(max),
	@noSpaceInName bit,
	@firstNameFirst bit,
	@workflowControlSetId uniqueidentifier,
	@fromPersonId uniqueidentifier,
	@shiftStartUTC smalldatetime,
	@shiftEndUTC smalldatetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @namesearch nvarchar(max);
	set @namesearch = '%' + @name + '%';

	-- Get all skills that the persons I can change with must have
select Skill 
INTO #matching 
    FROM WorkflowControlSetSkills
    where WorkflowControlSet = @workflowControlSetId 

-- ALL my skills so I can use that to see that I have all the skills that THEY have that I must have
SELECT ps.Skill 
into #allPersonSkill
                         FROM PersonSkill ps 
                         INNER JOIN PersonPeriod pp ON pp.Id = ps.Parent
                         AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
                         INNER JOIN Person p ON pp.Parent = p.Id
                         WHERE p.id = @fromPersonId
                         AND ps.Active = 1

-- here is all Skills that I have that are configured that the other person must have to make a trade possible
SELECT ps.Skill 
into #personSkill
                         FROM PersonSkill ps 
                         INNER JOIN PersonPeriod pp ON pp.Id = ps.Parent
                         AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
                         INNER JOIN Person p ON pp.Parent = p.Id
                         INNER JOIN #matching m on m.Skill = ps.Skill
                         WHERE p.id = @fromPersonId
                         AND ps.Active = 1

DECLARE @GroupIds table
(
    GroupId uniqueidentifier
)

INSERT INTO @GroupIds
	SELECT * FROM dbo.SplitStringString(@groupIdList)

--the number of skills that must match
DECLARE @numMatch int = (SELECT COUNT(*) FROM #personSkill)

--  bypass syntax checking so we can fill #persons in else clause below --
SELECT p.Id into #persons
FROM Person p 
WHERE 1=0;

IF (@numMatch = 0) 
BEGIN
       Insert
        into #persons
        SELECT p.id
        FROM Person(nolock) p 
        INNER JOIN PersonPeriod(nolock) pp ON p.Id = pp.Parent
        AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
        AND pp.Team in(SELECT * FROM @GroupIds)
        LEFT JOIN PersonSkill ps ON ps.Parent = pp.Id AND ps.Active = 1
        WHERE (p.TerminalDate >= '2016-09-25' OR p.TerminalDate IS NULL)
        AND p.WorkflowControlSet IS NOT NULL
        AND p.id <> @fromPersonId
		AND p.IsDeleted = 0
        GROUP BY p.id
END
ELSE
BEGIN
        select Parent into #personSkill2 from PersonSkill where Skill in (SELECT skill FROM #personSkill) and Active = 1
 -- all the person that have the skills that are must to make trade
        Insert
        into #persons
        SELECT p.id
        FROM Person(nolock) p 
        INNER JOIN PersonPeriod(nolock) pp ON p.Id = pp.Parent
        AND pp.Team in(SELECT * FROM @GroupIds)
        AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
        INNER JOIN #personSkill2 ps ON ps.Parent = pp.Id
        WHERE (p.TerminalDate >= '2016-09-25' OR p.TerminalDate IS NULL)
        AND p.WorkflowControlSet IS NOT NULL
        AND p.id <> @fromPersonId
		AND p.IsDeleted = 0
        GROUP BY p.id
        HAVING count(*) = @numMatch       
END
                             
 --of that persons that have another WCS and have must have skills configured
 --if I don't have one or more of those skills the Skill column will be null
 --if other people donn't have the skills in WCS the OtherSkill column will be null 
SELECT p.id, aps.Skill,ps.Skill as OtherSkill
INTO #othersMustMatch
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN WorkflowControlSet wcs ON p.WorkflowControlSet = wcs.id
INNER JOIN WorkflowControlSetSkills ws on ws.WorkflowControlSet =wcs.Id
AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
LEFT JOIN PersonSkill ps ON ps.Parent = pp.Id AND ps.Active = 1 AND ws.Skill = ps.Skill
LEFT JOIN #allPersonSkill aps ON aps.Skill = ps.Skill
WHERE p.Id in(select * from #persons)

--and then we remove the persons that had such a skill that i had not
--and remove persons that not match their WCS
DELETE P
FROM #persons p
INNER JOIN #othersMustMatch o ON o.Id = p.Id
WHERE o.Skill IS NULL or o.OtherSkill is null

-- remove the possibility to trade with an agent when my workflowControlSet has a must have skill
-- that the other agent has, but I do not.

SELECT p.id
INTO #othersHaveMustHaveSkillFromMyWCS
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
INNER JOIN PersonSkill ps ON ps.Parent = pp.Id AND ps.Active = 1 
--INNER JOIN #matching ON #matching.Skill = ps.Skill
WHERE p.Id in(select * from #persons)
AND ps.Skill in ( select Skill from #matching)
AND ps.SKill not in (select Skill from #allPersonSkill)
										          
DELETE P 
FROM #persons p
WHERE p.id in (select id from #othersHaveMustHaveSkillFromMyWCS)
                                                  
SELECT DISTINCT
gr.PersonId as PersonId,
gr.TeamId as TeamId,
gr.SiteId as SiteId,
gr.BusinessUnitId as BusinessUnitId
FROM ReadModel.GroupingReadOnly gr
INNER JOIN Person p ON p.id = gr.PersonId
WHERE gr.PersonId IN(select * from #persons)
AND @scheduleDate BETWEEN gr.StartDate and isnull(gr.EndDate,'2059-12-31')
AND (gr.LeavingDate >= @scheduleDate OR gr.LeavingDate IS NULL)
AND ((@namesearch is null or @namesearch = '')
            OR (@noSpaceInName = 1 AND @firstNameFirst = 0 AND ((p.LastName + p.FirstName) like @namesearch))
            OR (@noSpaceInName = 1 AND @firstNameFirst = 1 AND ((p.FirstName + p.LastName) like @namesearch)) 
            OR (@noSpaceInName = 0 AND @firstNameFirst = 0 AND ((p.LastName + ' ' + p.FirstName) like @namesearch))
            OR (@noSpaceInName = 0 AND @firstNameFirst = 1 AND ((p.FirstName + ' ' + p.LastName) like @namesearch)))

END
