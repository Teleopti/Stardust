----------------  
--Name: Xinfeng Li
--Date: 2014-12-24
--Desc: Add new table for agent badges with rank
---------------- 
CREATE TABLE [dbo].[AgentBadgeWithRankTransaction](
	[Id] [uniqueidentifier] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[BadgeType] [int] NOT NULL,
	[BronzeBadgeAmount] [smallint] NULL,
	[SilverBadgeAmount] [smallint] NULL,
	[GoldBadgeAmount] [smallint] NULL,
	[CalculatedDate] [datetime] NOT NULL,
	[Description] [nvarchar](50) NOT NULL,
	[InsertedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_AgentBadgeWithRankTransaction] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[AgentBadgeWithRankTransaction]  WITH CHECK ADD  CONSTRAINT [FK_AgentBadgeWithRankTransaction_Person_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[AgentBadgeWithRankTransaction] CHECK CONSTRAINT [FK_AgentBadgeWithRankTransaction_Person_Person]
GO

CREATE UNIQUE CLUSTERED INDEX [CIX_AgentBadgeWithRankTransaction] ON [dbo].[AgentBadgeWithRankTransaction]
(
	[Person] ASC,
	[BadgeType] ASC,
	[CalculatedDate] ASC
)
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

-- =============================================
-- Author:		Xinfeng Li
-- Create date: 2014-12-24
-- Description:	it will clean the whole table dbo.AgentBadgeWithRankTransaction
-- exec dbo.ResetAgentBadgesWithRank
-- =============================================
CREATE PROCEDURE [dbo].[ResetAgentBadgesWithRank]
WITH EXECUTE AS OWNER
AS
BEGIN
SET NOCOUNT ON;
TRUNCATE TABLE dbo.AgentBadgeWithRankTransaction
END
