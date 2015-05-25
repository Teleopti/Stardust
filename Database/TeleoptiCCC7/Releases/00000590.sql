CREATE TABLE [dbo].[OutboundCampaignManualProductionTime](
	[Campaign] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[PlannedTime] [bigint] NOT NULL)
GO

ALTER TABLE [dbo].[OutboundCampaignManualProductionTime] ADD CONSTRAINT [PK_OutboundCampaignManualProductionTime] PRIMARY KEY CLUSTERED
(
	[Campaign] ASC,
	[Date] ASC
)

GO

ALTER TABLE [dbo].[OutboundCampaignManualProductionTime] ADD CONSTRAINT [FK_OutboundCampaignManualProductionTime_OutboundCampaign] FOREIGN KEY
(
	[Campaign]
) REFERENCES [dbo].[OutboundCampaign]
(
	[Id]
)
