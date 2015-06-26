
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE dbo.OutboundCampaignWorkingPeriodAssignment
DROP CONSTRAINT FK_OutboundCampaignWorkingPeriodAssignment_OutboundCampaignWorkingPeriod

DROP TABLE dbo.OutboundCampaignWorkingPeriodAssignment
GO

ALTER TABLE dbo.OutboundCampaignWorkingPeriod
DROP CONSTRAINT FK_OutboundCampaignWorkingPeriod_OutboundCampaign

DROP TABLE dbo.OutboundCampaignWorkingPeriod
GO




