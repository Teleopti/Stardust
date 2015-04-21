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

-- USE [Infratest_CCC7]
-- GO

-- /****** Object:  Table [dbo].[Activity]    Script Date: 04/21/2015 14:13:48 ******/
-- SET ANSI_NULLS ON
-- GO

-- SET QUOTED_IDENTIFIER ON
-- GO

-- CREATE TABLE [dbo].[Activity](
	-- [Id] [uniqueidentifier] NOT NULL,
	-- [Version] [int] NOT NULL,
	-- [UpdatedBy] [uniqueidentifier] NOT NULL,
	-- [UpdatedOn] [datetime] NOT NULL,
	-- [Name] [nvarchar](50) NOT NULL,
	-- [ShortName] [nvarchar](25) NULL,
	-- [DisplayColor] [int] NOT NULL,
	-- [InContractTime] [bit] NOT NULL,
	-- [InReadyTime] [bit] NOT NULL,
	-- [RequiresSkill] [bit] NOT NULL,
	-- [BusinessUnit] [uniqueidentifier] NOT NULL,
	-- [IsDeleted] [bit] NOT NULL,
	-- [InPaidTime] [bit] NOT NULL,
	-- [InWorkTime] [bit] NOT NULL,
	-- [ReportLevelDetail] [int] NOT NULL,
	-- [IsMaster] [nvarchar](1) NOT NULL,
	-- [PayrollCode] [nvarchar](20) NULL,
	-- [RequiresSeat] [bit] NOT NULL,
	-- [AllowOverwrite] [bit] NOT NULL,
 -- CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED 
-- (
	-- [Id] ASC
-- )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
-- ) ON [PRIMARY]

-- GO

-- ALTER TABLE [dbo].[Activity]  WITH CHECK ADD  CONSTRAINT [FK_Activity_BusinessUnit] FOREIGN KEY([BusinessUnit])
-- REFERENCES [dbo].[BusinessUnit] ([Id])
-- GO

-- ALTER TABLE [dbo].[Activity] CHECK CONSTRAINT [FK_Activity_BusinessUnit]
-- GO

-- ALTER TABLE [dbo].[Activity]  WITH CHECK ADD  CONSTRAINT [FK_Activity_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
-- REFERENCES [dbo].[Person] ([Id])
-- GO

-- ALTER TABLE [dbo].[Activity] CHECK CONSTRAINT [FK_Activity_Person_UpdatedBy]
-- GO

-- ALTER TABLE [dbo].[Activity] ADD  CONSTRAINT [DF_Activity_IsMaster]  DEFAULT ((0)) FOR [IsMaster]
-- GO

-- ALTER TABLE [dbo].[Activity] ADD  CONSTRAINT [DF_Activity_RequiresSeat]  DEFAULT ((0)) FOR [RequiresSeat]
-- GO

-- ALTER TABLE [dbo].[Activity] ADD  CONSTRAINT [DF_Activity_AllowOverwrite]  DEFAULT ((1)) FOR [AllowOverwrite]
-- GO

