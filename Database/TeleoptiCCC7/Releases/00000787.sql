----------------  
--Name: Robin Karlsson
--Date: 2017-01-11
--Desc: Adding unique constraint for PersonSkill to avoid duplicate entries
---------------- 
WITH cte as(
  SELECT ROW_NUMBER() OVER (PARTITION BY Parent, Skill
                            ORDER BY Id) RN
  FROM   dbo.PersonSkill
  )
DELETE FROM cte WHERE RN>1
GO

ALTER TABLE [dbo].[PersonSkill] ADD CONSTRAINT UQ_PersonSkill_Parent_Skill UNIQUE NONCLUSTERED
(
	[Parent] ASC,
	[Skill] ASC
) ON [PRIMARY]
GO
