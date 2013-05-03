--#21539
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_Team_Id_Parent')
CREATE NONCLUSTERED INDEX IX_PersonPeriod_Team_Id_Parent
ON [dbo].[PersonPeriod] ([Team])
INCLUDE ([Id],[Parent])
GO

-----------------  
--Name: TamasB
--Date: 2012-12-17
--Desc: #bugfix 21764 - Fix invalid text
----------------  
UPDATE [dbo].PersonRequest
SET DenyReason = 'RequestDenyReasonSupervisor'
WHERE DenyReason = 'xxRequestDenyReasonSupervisor'
GO

----------------  
--Name: David Jonsson
--Date: 2013-01-08
--Desc: Bug #21786. Remove dubplicates in PersonSkill
----------------  
--remove duplicates, but keep one.
--note: We don't consider "Active" it will be random
WITH Dubplicates AS
(
	select Id, Parent,Skill,ROW_NUMBER() OVER(PARTITION BY Parent,Skill ORDER BY Id) as rownumber
	from PersonSkill
) 
DELETE ps
FROM PersonSkill ps
INNER JOIN Dubplicates d
ON ps.Id = d.Id
WHERE d.rownumber >1;
GO

--add constraint to block new duplicates
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonSkill]') AND name = N'UC_Parent_Skill')
ALTER TABLE PersonSkill ADD CONSTRAINT UC_Parent_Skill UNIQUE (Parent,Skill)
GO

----------------  
--Name: David Jonsson
--Date: 2013-05-04
--Desc: Bug #23349 Speed up rotations
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonRotation]') AND name = N'IX_PersonRotation_Person_IsDeleted_BusinessUnit_StartDate')
CREATE NONCLUSTERED INDEX [IX_PersonRotation_Person_IsDeleted_BusinessUnit_StartDate]
ON [dbo].[PersonRotation] ([Person],[IsDeleted],[BusinessUnit],[StartDate])
GO
