----------------  
--Name: Mingdi
--Date: 2018-05-08
--Desc: add one more colum for the clustered index
---------------- 

DROP INDEX [CIX_AgentBadgeWithRankTransaction] ON dbo.AgentBadgeWithRankTransaction
GO

CREATE UNIQUE CLUSTERED INDEX [CLIX_AgentBadgeWithRankTransaction] ON [dbo].[AgentBadgeWithRankTransaction]
(
	[Person] ASC,
	[BadgeType] ASC,
	[CalculatedDate] ASC,
	[IsExternal] ASC
)
GO


DROP INDEX [CIX_AgentBadgeTransaction] ON dbo.AgentBadgeTransaction
GO

CREATE UNIQUE CLUSTERED INDEX [CLIX_AgentBadgeTransaction] ON [dbo].[AgentBadgeTransaction]
(
	[Person] ASC,
	[BadgeType] ASC,
	[CalculatedDate] ASC,
	[IsExternal] ASC
)
GO