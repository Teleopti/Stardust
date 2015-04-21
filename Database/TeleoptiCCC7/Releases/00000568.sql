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
DELETE [RtaState] 
FROM [RtaState]
LEFT OUTER JOIN (
	SELECT 
		MIN(CAST(Id AS BINARY(16))) AS Id, 
		StateCode, 
		PlatformTypeId, 
		BusinessUnit
	FROM [RtaState]
	GROUP BY
		StateCode, 
		PlatformTypeId, 
		BusinessUnit
) AS KeepRows ON
	KeepRows.Id = [RtaState].Id
WHERE
	KeepRows.Id IS NULL
GO

ALTER TABLE [RtaState] 
ADD CONSTRAINT UQ_StateCode_PlatFormTypeId_BusinessUnit 
UNIQUE (StateCode, PlatformTypeId, BusinessUnit)
GO
