ALTER TABLE [dbo].[RtaState] DROP CONSTRAINT [UQ_StateCode_BusinessUnit]

ALTER TABLE dbo.RtaState
ALTER COLUMN Name nvarchar(300)
ALTER TABLE dbo.RtaState
ALTER COLUMN StateCode nvarchar(300)

ALTER TABLE [dbo].[RtaState] ADD  CONSTRAINT [UQ_StateCode_BusinessUnit] UNIQUE NONCLUSTERED 
(
	[StateCode] ASC,
	[BusinessUnit] ASC
)


DROP INDEX [IDX_RuleMappings] ON [ReadModel].[RuleMappings] WITH ( ONLINE = OFF )

ALTER TABLE ReadMOdel.RuleMappings
ALTER COLUMN StateCode nvarchar(300)

CREATE CLUSTERED INDEX [IDX_RuleMappings] ON [ReadModel].[RuleMappings]
(
	[ActivityId] ASC,
	[StateCode] ASC
)