CREATE TABLE [dbo].[OvertimeRequestOpenPeriodSkillType](
	[Id] [uniqueidentifier] NOT NULL ,
	[Parent] [uniqueidentifier] NOT NULL,
	[SkillType] [uniqueidentifier] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [PK_OvertimeRequestOpenPeriodSkillType] UNIQUE NONCLUSTERED 
(
	[Parent] ASC,
	[SkillType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


ALTER TABLE  [dbo].[OvertimeRequestOpenPeriodSkillType]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_OvertimeRequestOpenPeriod] FOREIGN KEY([Parent])
REFERENCES [dbo].[OvertimeRequestOpenPeriod] ([Id])
GO

ALTER TABLE  [dbo].[OvertimeRequestOpenPeriodSkillType]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_SkillType] FOREIGN KEY([SkillType])
REFERENCES [dbo].[SkillType] ([Id])
GO 