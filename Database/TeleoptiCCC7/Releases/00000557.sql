----------------  
--Name: Mingdi Jianguang Yanyi
--Date: 2015-03-30
--Desc: Create tables for outbound campaigns storage
---------------- 


CREATE TABLE dbo.OutboundCampaign
(
	Id uniqueidentifier NOT NULL,
	Name nvarchar(250) NOT NULL,
	Skill uniqueidentifier NOT NULL,
	CallListLen int NOT NULL,
	TargetRate int NOT NULL,
	ConnectRate int NOT NULL,
	RightPartyConnectRate int NOT NULL,
	ConnectAverageHandlingTime int NOT NULL,
	RightPartyAverageHandlingTime int NOT NULL,
	UnproductiveTime int NOT NULL DEFAULT 0,
	StartDateTime datetime NULL,
	EndDateTime datetime NULL,
	CampaignStatus int NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL,
	UpdatedBy uniqueidentifier NOT NULL,
	UpdatedOn datetime NOT NULL,
	IsDeleted bit NOT NULL DEFAULT 0
)
GO

ALTER TABLE dbo.OutboundCampaign ADD CONSTRAINT
	PK_OutbouondCampaign PRIMARY KEY CLUSTERED 
	(
	Id
	)
GO

ALTER TABLE [dbo].[OutboundCampaign]  WITH CHECK ADD  CONSTRAINT [FK_OutboundCampaign_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id]);

ALTER TABLE [dbo].[OutboundCampaign] CHECK CONSTRAINT [FK_OutboundCampaign_Skill];
GO

ALTER TABLE [dbo].[OutboundCampaign]  WITH CHECK ADD  CONSTRAINT [FK_OutboundCampaign_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id]);

ALTER TABLE [dbo].[OutboundCampaign] CHECK CONSTRAINT [FK_OutboundCampaign_UpdatedBy];
GO

ALTER TABLE [dbo].[OutboundCampaign]  WITH CHECK ADD  CONSTRAINT [FK_OutboundCampaign_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);

ALTER TABLE [dbo].[OutboundCampaign] CHECK CONSTRAINT [FK_OutboundCampaign_BusinessUnit];
GO



CREATE TABLE dbo.OutboundCampaignWorkingPeriod
(
	Id uniqueidentifier NOT NULL,
	OutboundCampaign uniqueidentifier NOT NULL,
	StartTime time NOT NULL,
	EndTime time NOT NULL	
)
GO

ALTER TABLE dbo.OutboundCampaignWorkingPeriod ADD CONSTRAINT
	PK_CampaignWorkingHourPeriod PRIMARY KEY CLUSTERED 
	(
	Id
	)
GO

ALTER TABLE [dbo].[OutboundCampaignWorkingPeriod]  WITH CHECK ADD  CONSTRAINT [FK_OutboundCampaignWorking_OutboundCampaignWorkingPeriod] FOREIGN KEY([OutboundCampaign])
REFERENCES [dbo].[OutboundCampaign] ([Id])
GO

ALTER TABLE [dbo].[OutboundCampaignWorkingPeriod] CHECK CONSTRAINT [FK_OutboundCampaignWorking_OutboundCampaignWorkingPeriod]
GO



CREATE TABLE dbo.OutboundCampaignWorkingPeriodAssignment
(
	Id uniqueidentifier NOT NULL,
	OutboundCampaignWorkingPeriod uniqueidentifier NOT NULL,
	WeekdayIndex smallint
)
GO

ALTER TABLE dbo.OutboundCampaignWorkingPeriodAssignment ADD CONSTRAINT
	PK_OutboundCampaignWorkingPeriodAssignment PRIMARY KEY CLUSTERED 
	(
	Id
	)
GO

ALTER TABLE [dbo].[OutboundCampaignWorkingPeriodAssignment]  WITH CHECK ADD  CONSTRAINT [FK_OutboundCampaignWorkingPeriod_OutboundCampaignWorkingPeriodAssignment] FOREIGN KEY([OutboundCampaignWorkingPeriod])
REFERENCES [dbo].[OutboundCampaignWorkingPeriod] ([Id])
GO

ALTER TABLE [dbo].[OutboundCampaignWorkingPeriodAssignment] CHECK CONSTRAINT [FK_OutboundCampaignWorkingPeriod_OutboundCampaignWorkingPeriodAssignment]
GO
