ALTER TABLE [dbo].[RtaState] ADD [BusinessUnit] [uniqueidentifier] NULL
GO

UPDATE [RtaState]
SET [BusinessUnit] = [RtaStateGroup].[BusinessUnit]
FROM [RtaState]
JOIN [RtaStateGroup] ON [RtaStateGroup].[Id] = [RtaState].[Parent]
GO

ALTER TABLE [RtaState] ALTER COLUMN [BusinessUnit] [uniqueidentifier] NOT NULL
GO

ALTER TABLE [RtaState]  WITH CHECK ADD  CONSTRAINT [FK_RtaState_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [BusinessUnit] ([Id])
GO

-- delete duplicates of state codes
WITH Duplicates
AS (
	SELECT s.Id, s.name, s.StateCode, s.PlatformTypeId,s.BusinessUnit, 
	ROW_NUMBER() OVER 
	(PARTITION BY s.StateCode, s.PlatformTypeId, s.BusinessUnit 
	ORDER BY
	g.IsDeleted ASC,
	g.UpdatedOn DESC) 
	AS DuplicateCount FROM RtaState s INNER JOIN RtaStateGroup g ON s.Parent = g.Id)
delete rs
from rtastate rs inner join Duplicates d on d.id = rs.id
where d.DuplicateCount >1;
GO


ALTER TABLE [RtaState] 
ADD CONSTRAINT UQ_StateCode_PlatFormTypeId_BusinessUnit 
UNIQUE (StateCode, PlatformTypeId, BusinessUnit)
GO
