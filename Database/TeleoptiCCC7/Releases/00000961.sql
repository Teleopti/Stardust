INSERT INTO  [dbo].[OvertimeRequestOpenPeriodSkillType]
(Id, Parent, SkillType)
SELECT NEWID(), Id, SkillType FROM [dbo].[OvertimeRequestOpenPeriod] WHERE SkillType IS NOT NULL

GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod] drop constraint FK_OvertimeRequestOpenPeriod_SkillType

GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod] DROP COLUMN SkillType

GO