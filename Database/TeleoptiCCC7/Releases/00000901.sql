CREATE TABLE [dbo].[ExternalBadgeSetting](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[QualityId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Enabled] [bit] NOT NULL,
	[LargerIsBetter] [bit] NOT NULL,
	[UnitType] [smallint] NOT NULL,
	[Threshold] [int] NULL,
	[BronzeThreshold] [int] NULL,
	[SilverThreshold] [int] NULL,
	[GoldThreshold] [int] NULL,
CONSTRAINT [PK_ExternalBadgeSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExternalBadgeSetting] WITH NOCHECK ADD CONSTRAINT [FK_ExternalBadgeSetting_GamificationSetting] FOREIGN KEY([Parent])
REFERENCES [dbo].[GamificationSetting] ([Id])
GO

ALTER TABLE [dbo].[ExternalBadgeSetting] CHECK CONSTRAINT [FK_ExternalBadgeSetting_GamificationSetting]
GO

ALTER TABLE [dbo].[ExternalBadgeSetting] ADD UNIQUE NONCLUSTERED 
(
	[Parent] ASC,
	[QualityId] ASC
)
GO

ALTER TABLE [dbo].[ExternalBadgeSetting] ADD UNIQUE NONCLUSTERED 
(
	[Parent] ASC,
	[Name] ASC
)
GO
