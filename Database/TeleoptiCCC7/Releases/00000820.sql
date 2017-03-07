DELETE [ReadModel].[RuleMappings]
UPDATE [ReadModel].KeyValueStore SET Value = 'True' WHERE [Key] = 'RuleMappingsInvalido'
GO
ALTER TABLE [ReadModel].[RuleMappings] DROP COLUMN PlatformTypeId
GO
DROP INDEX [IDX_RuleMappings] ON [ReadModel].[RuleMappings]
GO
ALTER TABLE [ReadModel].[RuleMappings] ALTER COLUMN StateCode varchar(300)
GO
CREATE CLUSTERED INDEX [IDX_RuleMappings] ON [ReadModel].[RuleMappings]
(
	[ActivityId] ASC,
	[StateCode] ASC
)
GO

IF EXISTS(select * FROM sys.views where name = 'v_RtaMapping')
	DROP VIEW [dbo].[v_RtaMapping]
ALTER TABLE [dbo].[RtaState] DROP CONSTRAINT [UQ_StateCode_PlatFormTypeId_BusinessUnit]
GO
ALTER TABLE dbo.[RtaState] ALTER COLUMN StateCode varchar(300)
GO
ALTER TABLE dbo.[RtaState] ALTER COLUMN Name varchar(300)
GO
DELETE dbo.[RtaState]
WHERE
PlatformTypeId <> '00000000-0000-0000-0000-000000000000' AND
StateCode = 'CCC Logged out'
GO
UPDATE dbo.[RtaState] 
SET
StateCode = StateCode + ' (' + convert(varchar(max), PlatformTypeId) + ')',
Name = Name + ' (' + convert(varchar(max), PlatformTypeId) + ')'
WHERE 
PlatformTypeId <> '00000000-0000-0000-0000-000000000000' AND
StateCode <> 'CCC Logged out'
GO
ALTER TABLE dbo.[RtaState] DROP COLUMN PlatformTypeId
GO
ALTER TABLE [dbo].[RtaState] ADD CONSTRAINT [UQ_StateCode_PlatFormTypeId_BusinessUnit] UNIQUE NONCLUSTERED 
(
	[StateCode] ASC,
	[BusinessUnit] ASC
)
GO

ALTER TABLE dbo.AgentState DROP COLUMN PlatformTypeId
GO

ALTER TABLE ReadModel.AgentState DROP COLUMN StateCode
GO