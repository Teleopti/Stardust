CREATE TABLE [dbo].[OutboundCampaignActualBacklog](
	[Campaign] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[ActualBacklog] [bigint] NOT NULL)
GO

ALTER TABLE [dbo].[OutboundCampaignActualBacklog] ADD CONSTRAINT [PK_OutboundCampaignActualBacklog] PRIMARY KEY CLUSTERED
(
	[Campaign] ASC,
	[Date] ASC
)

GO

ALTER TABLE [dbo].[OutboundCampaignActualBacklog] ADD CONSTRAINT [FK_OutboundCampaignActualBacklog_OutboundCampaign] FOREIGN KEY
(
	[Campaign]
) REFERENCES [dbo].[OutboundCampaign]
(
	[Id]
)