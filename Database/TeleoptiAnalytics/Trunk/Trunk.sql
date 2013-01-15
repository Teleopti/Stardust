/****** Object:  Table [RTA].[ActualAgentState]    Script Date: 01/15/2013 13:24:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [RTA].[ActualAgentState](
	[PersonId] [uniqueidentifier] NOT NULL,
	[StateCode] [nvarchar](500) NOT NULL,
	[PlatformTypeId] [uniqueidentifier] NOT NULL,
	[State] [nvarchar](500) NOT NULL,
	[StateId] [uniqueidentifier] NOT NULL,
	[Scheduled] [nvarchar](500) NOT NULL,
	[ScheduledId] [uniqueidentifier] NOT NULL,
	[StateStart] [datetime] NOT NULL,
	[ScheduledNext] [nvarchar](500) NOT NULL,
	[ScheduledNextId] [uniqueidentifier] NOT NULL,
	[NextStart] [datetime] NOT NULL,
	[AlarmName] [nvarchar](500) NOT NULL,
	[AlarmId] [uniqueidentifier] NOT NULL,
	[Color] [int] NOT NULL,
	[AlarmStart] [datetime] NOT NULL,
	[StaffingEffect] [float] NOT NULL,
 CONSTRAINT [PK_ActualAgentState] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO