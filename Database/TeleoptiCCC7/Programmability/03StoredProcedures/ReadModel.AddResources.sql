IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[AddResources]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[AddResources]
GO

CREATE PROCEDURE ReadModel.AddResources
	@Activity uniqueidentifier,
	@Skills nvarchar(1500),
	@ActivityRequiresSeat bit,
	@PeriodStart datetime,
	@PeriodEnd datetime,
	@Resources float,
	@Heads float
AS
BEGIN
	DECLARE @ActivitySkillCombination int
	DECLARE @PeriodResourceId bigint

	--insert ReadModel.ActivitySkillCombination
	SELECT @ActivitySkillCombination=Id
	FROM ReadModel.ActivitySkillCombination
	WHERE Activity=@Activity
	AND Skills=@Skills

	IF (@ActivitySkillCombination IS NULL)
		begin
			INSERT INTO ReadModel.ActivitySkillCombination (Activity,Skills,ActivityRequiresSeat) VALUES (@Activity,@Skills,@ActivityRequiresSeat)
			SELECT @ActivitySkillCombination=SCOPE_IDENTITY()
		end
	--ELSE?

	--insert/update ReadModel.ScheduledResources
	SELECT @PeriodResourceId = Id
	FROM ReadModel.ScheduledResources
	WHERE ActivitySkillCombinationId = @ActivitySkillCombination
	AND PeriodStart = @PeriodStart

	IF (@PeriodResourceId IS NULL)
		begin
			INSERT INTO ReadModel.ScheduledResources (ActivitySkillCombinationId,Resources,Heads,PeriodStart,PeriodEnd) VALUES (@ActivitySkillCombination,@Resources,@Heads,@PeriodStart,@PeriodEnd)
			SELECT @PeriodResourceId=SCOPE_IDENTITY()
		end
	ELSE
		begin
			UPDATE ReadModel.ScheduledResources
			SET
				Resources	= Resources+@Resources,
				Heads		= Heads+1
			WHERE ActivitySkillCombinationId = @ActivitySkillCombination
			AND PeriodStart = @PeriodStart
		end
		
	SELECT @PeriodResourceId
END
GO
