----------------  
--Name: Chundan Xu, Zhiping Lan
--Date: 2015-1-5
--Desc: Add new table for gamification setting in PBI 31318
---------------- 
CREATE TABLE [dbo].[DifferentialAgentBadgeSettings](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[BadgeSettingRuleSet] [int] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[AnsweredCallsBadgeEnabled] [bit] NOT NULL,
	[AnsweredCallsThreshold] [int] NOT NULL,
	[AnsweredCallsBronzeThreshold] [int] NOT NULL,
	[AnsweredCallsSilverThreshold] [int] NOT NULL,
	[AnsweredCallsGoldThreshold] [int] NOT NULL,
	[AHTBadgeEnabled] [bit] NOT NULL,
	[AHTThreshold] [int] NOT NULL,
	[AHTBronzeThreshold] [int] NOT NULL,
	[AHTSilverThreshold] [int] NOT NULL,
	[AHTGoldThreshold] [int] NOT NULL,
	[AdherenceBadgeEnabled] [bit] NOT NULL,
	[AdherenceThreshold] [int] NOT NULL,
	[AdherenceBronzeThreshold] [int] NOT NULL,
	[AdherenceSilverThreshold] [int] NOT NULL,
	[AdherenceGoldThreshold] [int] NOT NULL,
	[SilverBadgeDaysThreshold] [int] NOT NULL,
	[GoldBadgeDaysThreshold] [int] NOT NULL,
 CONSTRAINT [PK_DifferentialAgentBadgeSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[DifferentialAgentBadgeSettings]  WITH CHECK ADD  CONSTRAINT [FK_DifferentialAgentBadgeSettings_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);
Go

ALTER TABLE [dbo].[DifferentialAgentBadgeSettings] CHECK CONSTRAINT [FK_DifferentialAgentBadgeSettings_BusinessUnit]
GO

ALTER TABLE [dbo].[DifferentialAgentBadgeSettings]  WITH CHECK ADD  CONSTRAINT [FK_DifferentialAgentBadgeSettings_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[DifferentialAgentBadgeSettings] CHECK CONSTRAINT [FK_DifferentialAgentBadgeSettings_Person_UpdatedBy]
GO

