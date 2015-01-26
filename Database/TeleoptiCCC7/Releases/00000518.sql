----------------  
--Name: Chundan Xu, Zhiping Lan
--Date: 2015-1-16
--Desc: create teamGamificationSetting table 
--to save team with applied gamification setting
---------------- 
CREATE TABLE [dbo].[TeamGamificationSetting](
	[Id] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Team] [uniqueidentifier] NOT NULL,
	[GamificationSetting] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_TeamGamificationSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TeamGamificationSetting]  WITH CHECK ADD  CONSTRAINT [FK_TeamGamificationSetting_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);
Go

ALTER TABLE [dbo].[TeamGamificationSetting] CHECK CONSTRAINT [FK_TeamGamificationSetting_BusinessUnit]
GO

ALTER TABLE [dbo].[TeamGamificationSetting]  WITH CHECK ADD  CONSTRAINT [FK_TeamGamificationSetting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[TeamGamificationSetting] CHECK CONSTRAINT [FK_TeamGamificationSetting_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[TeamGamificationSetting]  WITH CHECK ADD  CONSTRAINT [FK_TeamGamificationSetting_Team] FOREIGN KEY([Team])
REFERENCES [dbo].[Team] ([Id])
GO

ALTER TABLE [dbo].[TeamGamificationSetting] CHECK CONSTRAINT [FK_TeamGamificationSetting_Team]
GO

ALTER TABLE [dbo].[TeamGamificationSetting]  WITH CHECK ADD  CONSTRAINT [FK_TeamGamificationSetting_GamificationSetting] FOREIGN KEY([GamificationSetting])
REFERENCES [dbo].[GamificationSetting] ([Id])
GO

ALTER TABLE [dbo].[TeamGamificationSetting] CHECK CONSTRAINT [FK_TeamGamificationSetting_GamificationSetting]
GO
