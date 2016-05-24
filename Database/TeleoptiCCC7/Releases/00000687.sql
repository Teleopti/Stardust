IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AgentState]') AND type in (N'U'))
DROP TABLE [dbo].[AgentState]
GO

CREATE TABLE [dbo].[AgentState](
	[PersonId] [uniqueidentifier] NOT NULL,
	[BatchId] [datetime] NULL,
	[SourceId] [nvarchar](50) NULL,
	[PlatformTypeId] [uniqueidentifier] NULL,
	[BusinessUnitId] [uniqueidentifier] NULL,
	[SiteId] [uniqueidentifier] NULL,
	[TeamId] [uniqueidentifier] NULL,
	[ReceivedTime] [datetime] NULL,
	[StateCode] [nvarchar](25) NULL,
	[StateGroupId] [uniqueidentifier] NULL,
	[StateStartTime] [datetime] NULL,
	[ActivityId] [uniqueidentifier] NULL,
	[NextActivityId] [uniqueidentifier] NULL,
	[NextActivityStartTime] [datetime] NULL,
	[RuleId] [uniqueidentifier] NULL,
	[RuleStartTime] [datetime] NULL,
	[StaffingEffect] [float] NULL,
	[Adherence] [int] NULL,
	[AlarmStartTime] [datetime] NULL,
	CONSTRAINT [PK_AgentState] PRIMARY KEY CLUSTERED (
		[PersonId] ASC
	)
)
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[AgentState]') AND type in (N'U'))
DROP TABLE [ReadModel].[AgentState]
GO

CREATE TABLE [ReadModel].[AgentState](
	[PersonId] [uniqueidentifier] NOT NULL,
	[BusinessUnitId] [uniqueidentifier] NULL,
	[SiteId] [uniqueidentifier] NULL,
	[TeamId] [uniqueidentifier] NULL,
	[ReceivedTime] [datetime] NULL,
	[Activity] [nvarchar](50) NULL,
	[NextActivity] [nvarchar](50) NULL,
	[NextActivityStartTime] [datetime] NULL,
	[StateCode] [nvarchar](25) NULL,
	[StateName] [nvarchar](50) NULL,
	[StateStartTime] [datetime] NULL,
	[RuleName] [nvarchar](50) NULL,
	[RuleStartTime] [datetime] NULL,
	[RuleColor] [int] NULL,
	[StaffingEffect] [float] NULL,
	[IsRuleAlarm] [bit] NULL,
	[AlarmStartTime] [datetime] NULL,
	[AlarmColor] [int] NULL,
	CONSTRAINT [PK_AgentState] PRIMARY KEY CLUSTERED (
		[PersonId] ASC
	)
)
GO

CREATE NONCLUSTERED INDEX [IX_AgentState_BusinessUnitId] ON [ReadModel].[AgentState] (
	[BusinessUnitId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_AgentState_SiteId] ON [ReadModel].[AgentState] (
	[SiteId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_AgentState_TeamId] ON [ReadModel].[AgentState] (
	[TeamId] ASC
)
GO
