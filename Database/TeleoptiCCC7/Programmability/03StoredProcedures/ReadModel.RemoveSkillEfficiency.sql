IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[RemoveSkillEfficiency]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[RemoveSkillEfficiency]
GO

CREATE PROCEDURE [ReadModel].[RemoveSkillEfficiency]
	@ResourceId bigint,
	@SkillId uniqueidentifier,
	@Efficiency float
AS
BEGIN
	IF (EXISTS (SELECT 1 FROM ReadModel.PeriodSkillEfficiencies WHERE ParentId=@ResourceId AND SkillId=@SkillId))
		begin
			UPDATE ReadModel.PeriodSkillEfficiencies SET Amount=Amount-@Efficiency WHERE ParentId=@ResourceId AND SkillId=@SkillId
		end
END

GO


