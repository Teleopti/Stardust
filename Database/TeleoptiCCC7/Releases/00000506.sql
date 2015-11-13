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

