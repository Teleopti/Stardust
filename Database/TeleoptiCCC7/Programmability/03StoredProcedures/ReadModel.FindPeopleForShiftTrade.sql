
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
INTO #fromWorkflowControlSetSkills
    FROM WorkflowControlSetSkills
    where WorkflowControlSet = @workflowControlSetId 

-- ALL my skills so I can use that to see that I have all the skills that THEY have that I must have
SELECT ps.Skill 
into #fromPersonSkill
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
                         INNER JOIN #fromWorkflowControlSetSkills m on m.Skill = ps.Skill
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
SELECT p.Id,p.WorkflowControlSet into #persons
FROM Person p 
WHERE 1=0;

IF (@numMatch = 0) 
BEGIN
       Insert
        into #persons
        SELECT p.id,p.WorkflowControlSet
        FROM Person(nolock) p 
        INNER JOIN PersonPeriod(nolock) pp ON p.Id = pp.Parent
        AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
        AND pp.Team in(SELECT * FROM @GroupIds)
        LEFT JOIN PersonSkill ps ON ps.Parent = pp.Id AND ps.Active = 1
        WHERE (p.TerminalDate >= '2016-09-25' OR p.TerminalDate IS NULL)
        AND p.WorkflowControlSet IS NOT NULL
        AND p.id <> @fromPersonId
		AND p.IsDeleted = 0
        GROUP BY p.id,p.WorkflowControlSet
END
ELSE
BEGIN
        select Parent into #personSkill2 from PersonSkill where Skill in (SELECT skill FROM #personSkill) and Active = 1
 -- all the person that have the skills that are must to make trade
        Insert
        into #persons
        SELECT p.id,p.WorkflowControlSet
        FROM Person(nolock) p 
        INNER JOIN PersonPeriod(nolock) pp ON p.Id = pp.Parent
        AND pp.Team in(SELECT * FROM @GroupIds)
        AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
        INNER JOIN #personSkill2 ps ON ps.Parent = pp.Id
        WHERE (p.TerminalDate >= '2016-09-25' OR p.TerminalDate IS NULL)
        AND p.WorkflowControlSet IS NOT NULL
        AND p.id <> @fromPersonId
		AND p.IsDeleted = 0
        GROUP BY p.id,p.WorkflowControlSet
        HAVING count(*) = @numMatch       
END
           
DECLARE @toPersonId uniqueidentifier, @toWorkflowControlSetId uniqueidentifier                  
DECLARE persons_cursor CURSOR
FOR SELECT id,WorkflowControlSet FROM #persons

OPEN persons_cursor
FETCH NEXT FROM persons_cursor
INTO @toPersonId,@toWorkflowControlSetId

WHILE @@FETCH_STATUS = 0
BEGIN
	--ToPerson Skills
	SELECT ps.Skill 
	INTO #toPersonSkill
    FROM PersonSkill ps 
    INNER JOIN PersonPeriod pp ON pp.Id = ps.Parent
    AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
    INNER JOIN Person p ON pp.Parent = p.Id
    WHERE p.id = @toPersonId
    AND ps.Active = 1

	--Different skills between FromPerson And ToPerson
	SELECT isnull(tps.Skill, fps.Skill) Skill
	INTO #differentSkill
	FROM #toPersonSkill tps
	FULL JOIN #fromPersonSkill fps on (tps.Skill = fps.Skill)
	WHERE (tps.Skill is null or fps.Skill is null)

	IF EXISTS (SELECT 1 FROM #differentSkill)
	BEGIN
		SELECT t.* INTO #skillsInWCS FROM
		(
			--check matching skills in ToWCS
			SELECT Skill 
			FROM WorkflowControlSetSkills wcsSkill
			WHERE WorkflowControlSet = @toWorkflowControlSetId 
			AND   wcsSkill.Skill IN (SELECT Skill FROM #differentSkill)
			UNION ALL
			--check matching skills in FromWCS
			SELECT Skill
			FROM #fromWorkflowControlSetSkills m
			WHERE m.Skill IN (SELECT Skill FROM #differentSkill)
		) t
		IF EXISTS (SELECT 1 FROM #skillsInWCS)
		BEGIN
			DELETE FROM #persons WHERE CURRENT OF persons_cursor; 
		END
		DROP TABLE #skillsInWCS
	END

	DROP TABLE #toPersonSkill
	DROP TABLE #differentSkill

	FETCH NEXT FROM persons_cursor
	INTO @toPersonId,@toWorkflowControlSetId
END

CLOSE persons_cursor 
DEALLOCATE persons_cursor
                    
SELECT DISTINCT
gr.PersonId as PersonId,
gr.TeamId as TeamId,
gr.SiteId as SiteId,
gr.BusinessUnitId as BusinessUnitId
FROM ReadModel.GroupingReadOnly gr
INNER JOIN Person p ON p.id = gr.PersonId
WHERE gr.PersonId IN(select id from #persons)
AND @scheduleDate BETWEEN gr.StartDate and isnull(gr.EndDate,'2059-12-31')
AND (gr.LeavingDate >= @scheduleDate OR gr.LeavingDate IS NULL)
AND ((@namesearch is null or @namesearch = '')
            OR (@noSpaceInName = 1 AND @firstNameFirst = 0 AND ((p.LastName + p.FirstName) like @namesearch))
            OR (@noSpaceInName = 1 AND @firstNameFirst = 1 AND ((p.FirstName + p.LastName) like @namesearch)) 
            OR (@noSpaceInName = 0 AND @firstNameFirst = 0 AND ((p.LastName + ' ' + p.FirstName) like @namesearch))
            OR (@noSpaceInName = 0 AND @firstNameFirst = 1 AND ((p.FirstName + ' ' + p.LastName) like @namesearch)))

END
