IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[AgentBadge]'))
DROP VIEW [dbo].[AgentBadge]
GO

CREATE VIEW [dbo].[AgentBadge]
AS
SELECT Person,
       BadgeType,
       sum(Amount) AS 'TotalAmount',
       max(CalculatedDate) AS 'LastCalculatedDate'
  FROM dbo.AgentBadgeTransaction
GROUP BY person, BadgeType
GO
