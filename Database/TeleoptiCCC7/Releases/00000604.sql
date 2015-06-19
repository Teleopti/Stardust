
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

PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[Weekday] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO


ALTER TABLE [dbo].[OutboundCampaignWorkingHours]  WITH CHECK ADD  CONSTRAINT [FK_OutboundCampaignWorkingHours_OutboundCampaign] FOREIGN KEY([Parent])
REFERENCES [dbo].[OutboundCampaign] ([Id])
GO

ALTER TABLE [dbo].[OutboundCampaignWorkingHours] CHECK CONSTRAINT [FK_OutboundCampaignWorkingHours_OutboundCampaign]
GO