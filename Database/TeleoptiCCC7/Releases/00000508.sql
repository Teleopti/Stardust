----------------  
--Name: Chundan Xu, Zhiping Lan
--Date: 2015-1-5
--Desc: Add new table for gamification setting in PBI 31318
---------------- 
IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[GamificationSetting]'))
   DROP TABLE [dbo].[GamificationSetting]
GO
CREATE TABLE [dbo].[GamificationSetting](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[GamificationSettingRuleSet] [int] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[AnsweredCallsBadgeEnabled] [bit] NOT NULL,
	[AnsweredCallsThreshold] [int] NOT NULL,
	[AnsweredCallsBronzeThreshold] [int] NOT NULL,
	[AnsweredCallsSilverThreshold] [int] NOT NULL,
	[AnsweredCallsGoldThreshold] [int] NOT NULL,
	[AHTBadgeEnabled] [bit] NOT NULL,
	[AHTThreshold] [bigint] NOT NULL,
	[AHTBronzeThreshold] [bigint] NOT NULL,
	[AHTSilverThreshold] [bigint] NOT NULL,
	[AHTGoldThreshold] [bigint] NOT NULL,
	[AdherenceBadgeEnabled] [bit] NOT NULL,
	[AdherenceThreshold] [int] NOT NULL,
	[AdherenceBronzeThreshold] [int] NOT NULL,
	[AdherenceSilverThreshold] [int] NOT NULL,
	[AdherenceGoldThreshold] [int] NOT NULL,
	[SilverToBronzeBadgeRate] [int] NOT NULL,
	[GoldToSilverBadgeRate] [int] NOT NULL,
 CONSTRAINT [PK_GamificationSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[GamificationSetting]  WITH CHECK ADD  CONSTRAINT [FK_GamificationSetting_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);
Go

ALTER TABLE [dbo].[GamificationSetting] CHECK CONSTRAINT [FK_GamificationSetting_BusinessUnit]
GO

ALTER TABLE [dbo].[GamificationSetting]  WITH CHECK ADD  CONSTRAINT [FK_GamificationSetting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[GamificationSetting] CHECK CONSTRAINT [FK_GamificationSetting_Person_UpdatedBy]
GO

