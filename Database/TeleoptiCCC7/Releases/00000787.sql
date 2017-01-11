----------------  
--Name: Robin Karlsson
--Date: 2017-01-11
--Desc: Adding unique constraint for PersonSkill to avoid duplicate entries
---------------- 
ALTER TABLE [dbo].[PersonSkill] ADD CONSTRAINT UQ_PersonSkill_Parent_Skill UNIQUE NONCLUSTERED
(
	[Parent] ASC,
	[Skill] ASC
) ON [PRIMARY]
GO
