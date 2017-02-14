----------------  
--Name: Jonas & Bharath
--Date: 2017-02-14
--Desc: Removing unique constraint for PersonSkill
---------------- 
ALTER TABLE [dbo].[PersonSkill] DROP CONSTRAINT [UQ_PersonSkill_Parent_Skill]
GO


