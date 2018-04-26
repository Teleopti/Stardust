-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-23
-- Desc: Fixing missing clustered indexes. VSTS #74982
-----------------------------------------------------------

ALTER TABLE dbo.OutboundCampaignWorkingHours
	DROP CONSTRAINT PK_OutboundCampaignWorkingHours
GO
ALTER TABLE dbo.OutboundCampaignWorkingHours ADD CONSTRAINT
	PK_OutboundCampaignWorkingHours PRIMARY KEY CLUSTERED 
	(
		Parent,
		Weekday
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

GO
ALTER TABLE dbo.OutboundCampaignWorkingHours SET (LOCK_ESCALATION = TABLE)
GO
