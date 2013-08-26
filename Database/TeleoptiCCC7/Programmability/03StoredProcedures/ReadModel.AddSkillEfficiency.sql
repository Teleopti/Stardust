IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[AddSkillEfficiency]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[AddSkillEfficiency]
GO

CREATE PROCEDURE [ReadModel].[AddSkillEfficiency]
	@ResourceId bigint,
	@SkillId uniqueidentifier,
	@Efficiency float
AS
BEGIN
	IF (NOT EXISTS (SELECT 1 FROM ReadModel.PeriodSkillEfficiencies WHERE ParentId=@ResourceId AND SkillId=@SkillId))
		begin
			INSERT INTO ReadModel.PeriodSkillEfficiencies (ParentId,SkillId,Amount) VALUES (@ResourceId,@SkillId,@Efficiency)
		end
	ELSE
		begin
			UPDATE ReadModel.PeriodSkillEfficiencies SET Amount=Amount+@Efficiency WHERE ParentId=@ResourceId AND SkillId=@SkillId
		end
END

GO
