
ALTER TABLE dbo.OutboundCampaign
Add BelongsToStartDate smalldatetime
GO
ALTER TABLE dbo.OutboundCampaign
Add BelongsToEndDate smalldatetime
GO


UPDATE dbo.OutboundCampaign
SET BelongsToStartDate='1900-1-1'
GO
UPDATE dbo.OutboundCampaign
SET BelongsToEndDate='2050-1-1'
GO

ALTER TABLE dbo.OutboundCampaign
AlTER COLUMN BelongsToStartDate smalldatetime not null 
ALTER TABLE dbo.OutboundCampaign
AlTER COLUMN BelongsToEndDate smalldatetime not null
GO