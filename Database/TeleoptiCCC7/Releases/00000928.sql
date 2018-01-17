----------------  
--Name: CodeMonkeys
--Date: 2018-01-17
--Desc: Add SkillType to OvertimeRequestOpenPeriod
----------------  

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod]
ADD SkillType uniqueidentifier
GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod]
ADD CONSTRAINT FK_OvertimeRequestOpenPeriod_SkillType FOREIGN KEY(SkillType)
REFERENCES [dbo].[SkillType](Id)