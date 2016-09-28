
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
        FROM Person p 
        INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
        AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
        AND pp.Team in(SELECT * FROM @GroupIds)
        LEFT JOIN PersonSkill ps ON ps.Parent = pp.Id AND ps.Active = 1
        WHERE (p.TerminalDate >= '2016-09-25' OR p.TerminalDate IS NULL)
        AND p.WorkflowControlSet IS NOT NULL
        AND p.id <> @fromPersonId
        GROUP BY p.id
END
ELSE
BEGIN

 -- all the person that have the skills that are must to make trade
        Insert
        into #persons
        SELECT p.id
        FROM Person p 
        INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
        AND pp.Team in(SELECT * FROM @GroupIds)
        AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
        INNER JOIN PersonSkill ps ON ps.Parent = pp.Id AND ps.Active = 1
        WHERE Skill in(SELECT * FROM #personSkill)
        AND (p.TerminalDate >= '2016-09-25' OR p.TerminalDate IS NULL)
        AND p.WorkflowControlSet IS NOT NULL
        AND p.id <> @fromPersonId
        GROUP BY p.id
        HAVING count(*) = @numMatch       
END
                             
 --of that persons that have another WCS and have must have skills configured
 --if I don't have one or more of those skills the Skill column will become null
SELECT p.id, aps.Skill
INTO #othersMustMatch
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN WorkflowControlSet wcs ON p.WorkflowControlSet = wcs.id
INNER JOIN WorkflowControlSetSkills ws on ws.WorkflowControlSet =wcs.Id
AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
INNER JOIN PersonSkill ps ON ps.Parent = pp.Id AND ps.Active = 1 AND ws.Skill = ps.Skill
LEFT JOIN #allPersonSkill aps ON aps.Skill = ps.Skill
WHERE p.Id in(select * from #persons)
                                                  
--and then we remove the persons that had such a skill that i had not
DELETE P
FROM #persons p
INNER JOIN #othersMustMatch o ON o.Id = p.Id
WHERE o.Skill IS NULL
                                                  
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

            ORDER BY PersonId

drop table #matching
drop table #personSkill
drop table #persons
drop table #allPersonSkill
drop table #othersMustMatch


END
