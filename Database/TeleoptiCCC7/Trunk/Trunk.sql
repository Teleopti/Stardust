----------------  
--Name: Robin Karlsson
--Date: 2013-02-13
--Desc: Bug #22194. Constraint violation error in PersonSkill
----------------  
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonSkill]') AND name = N'UC_Parent_Skill')
	EXEC('ALTER TABLE [dbo].[PersonSkill] DROP CONSTRAINT [UC_Parent_Skill]')
GO