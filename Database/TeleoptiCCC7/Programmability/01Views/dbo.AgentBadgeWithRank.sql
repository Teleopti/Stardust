IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[AgentBadgeWithRank]'))
DROP VIEW [dbo].[AgentBadgeWithRank]
GO

----------------  
--Name: Xinfeng Li
--Date: 2014-12-25
--Desc: Add new view to get total badges with rank
---------------- 
CREATE VIEW [dbo].[AgentBadgeWithRank]
AS
SELECT Person,
       BadgeType,
       sum(BronzeBadgeAmount) AS 'BronzeBadgeAmount',
       sum(SilverBadgeAmount) AS 'SilverBadgeAmount',
       sum(GoldBadgeAmount) AS 'GoldBadgeAmount',
       max(CalculatedDate) AS 'LastCalculatedDate'
  FROM dbo.AgentBadgeWithRankTransaction
GROUP BY person, BadgeType
GO