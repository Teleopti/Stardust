
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

	DECLARE @GroupIds table
	(
		GroupId uniqueidentifier
	)

	INSERT INTO @GroupIds
	SELECT * FROM dbo.SplitStringString(@groupIdList)


	DECLARE @WorkflowControlSetSkillsFrom table
	(
		SkillId uniqueidentifier
	)

	Insert INTO @WorkflowControlSetSkillsFrom
	Select wcsSkill.SKill from WorkflowControlSetSkills wcsSkill, WorkflowControlSet wcs 
	where wcsSkill.WorkflowControlSet = wcs.Id 
	and wcs.Id = @workflowControlSetId

	DECLARE @AllPersonSkillsFrom table
	(
 		SkillId uniqueidentifier
	)

	Insert Into @AllPersonSkillsFrom
		SELECT ps.Skill
		FROM PersonSkill ps 
		INNER JOIN PersonPeriod pp ON pp.Id = ps.Parent
		AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
		AND ps.Active = 1
		INNER JOIN Person p ON pp.Parent = p.Id
		WHERE p.id = @fromPersonId

	SELECT DISTINCT
		gr.PersonId as PersonId,
		gr.TeamId as TeamId,
		gr.SiteId as SiteId,
		gr.BusinessUnitId as BusinessUnitId
	FROM ReadModel.GroupingReadOnly gr
		INNER JOIN Person p ON gr.PersonId = p.Id
		INNER JOIN @GroupIds gids ON gids.GroupId = gr.GroupId
		INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
		LEFT JOIN PersonSkill ps ON ps.Parent = pp.Id
		INNER JOIN WorkflowControlSet wcs on p.WorkflowControlSet = wcs.Id
	WHERE gr.Businessunitid = @businessUnitId
		AND @scheduleDate BETWEEN gr.StartDate and isnull(gr.EndDate,'2059-12-31')
		AND (gr.LeavingDate >= @scheduleDate OR gr.LeavingDate IS NULL)
		AND p.WorkflowControlSet IS NOT NULL
		--AND PageName = 'xxMain'
		AND ((@namesearch is null or @namesearch = '')
			OR (@noSpaceInName = 1 AND @firstNameFirst = 0 AND ((p.LastName + p.FirstName) like @namesearch))
			OR (@noSpaceInName = 1 AND @firstNameFirst = 1 AND ((p.FirstName + p.LastName) like @namesearch)) 
			OR (@noSpaceInName = 0 AND @firstNameFirst = 0 AND ((p.LastName + ' ' + p.FirstName) like @namesearch))
			OR (@noSpaceInName = 0 AND @firstNameFirst = 1 AND ((p.FirstName + ' ' + p.LastName) like @namesearch)))
		AND  NOT EXISTS (
			Select Id from SKILL where ( 
				Skill.Id in ( 
					Select SkillId From @AllPersonSkillsFrom 
					EXCEPT
					Select ps.Skill
					FROM PersonSkill ps 
					INNER JOIN PersonPeriod pp ON pp.Id = ps.Parent
						AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
					AND pp.Parent = p.Id
					WHERE ps.Active = 1 )
				OR
				Skill.Id in (
					Select ps.Skill
					FROM PersonSkill ps 
					INNER JOIN PersonPeriod pp ON pp.Id = ps.Parent
						AND @scheduleDate BETWEEN pp.StartDate and isnull(pp.EndDate,'2059-12-31')
					AND pp.Parent = p.Id
					WHERE ps.Active = 1
					EXCEPT
					Select SkillId From @AllPersonSkillsFrom 
				)															
			)
			AND
				Skill.Id in (
					SELECT SkillId From @WorkflowControlSetSkillsFrom
					UNION
					Select wcsSkill.SKill from WorkflowControlSetSkills wcsSkill 
					where wcsSkill.WorkflowControlSet = wcs.Id 
				)
		) 
	
	OPTION	(FORCE ORDER)
END
