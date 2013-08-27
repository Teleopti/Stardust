IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[RemoveResources]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[RemoveResources]
GO

CREATE PROCEDURE ReadModel.RemoveResources
	@Activity uniqueidentifier,
	@Skills nvarchar(1500),
	@PeriodStart datetime,
	@PeriodEnd datetime,
	@Resources float,
	@Heads float
AS
BEGIN
	DECLARE @ActivitySkillCombination int
	
	SELECT @ActivitySkillCombination=Id FROM ReadModel.ActivitySkillCombination WHERE Activity=@Activity AND Skills=@Skills
	IF (@ActivitySkillCombination IS NOT NULL)
		begin
			DECLARE @PeriodResourceId bigint
			SELECT @PeriodResourceId = Id FROM ReadModel.ScheduledResources WHERE ActivitySkillCombinationId = @ActivitySkillCombination AND PeriodStart = @PeriodStart
			IF (@PeriodResourceId IS NOT NULL)
				begin
					UPDATE ReadModel.ScheduledResources SET Resources=Resources-@Resources,Heads=Heads-1 WHERE Id=@PeriodResourceId
				end
		end
	
END
GO
