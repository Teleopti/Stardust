
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[OutboundCampaign] ALTER COLUMN [StartDate] DATETIME NOT NULL 
GO

ALTER TABLE [dbo].[OutboundCampaign] ALTER COLUMN [EndDate] DATETIME NOT NULL 
GO

ALTER TABLE [dbo].[OutboundCampaign] DROP COLUMN [CampaignStatus]
GO

CREATE TABLE [dbo].[OutboundCampaignWorkingHours](
	[Parent] [uniqueidentifier] NOT NULL,
	[Weekday] [int] NOT NULL,
	[StartTime] [bigint] NOT NULL,
	[EndTime] [bigint] NOT NULL,
	CONSTRAINT [PK_OutboundCampaignWorkingHours] PRIMARY KEY NONCLUSTERED 
	(
		[Parent] ASC,
		[Weekday] ASC
	)
)

GO


ALTER TABLE [dbo].[OutboundCampaignWorkingHours]  WITH CHECK ADD  CONSTRAINT [FK_OutboundCampaignWorkingHours_OutboundCampaign] FOREIGN KEY([Parent])
REFERENCES [dbo].[OutboundCampaign] ([Id])
GO

ALTER TABLE [dbo].[OutboundCampaignWorkingHours] CHECK CONSTRAINT [FK_OutboundCampaignWorkingHours_OutboundCampaign]
GO
