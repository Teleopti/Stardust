--Name: Xinfeng
--Date: 2014-09-05  
--Desc: Add new table for agent badges transaction and change AgentBadge table to view
--------------------------------------------------------------------------------------------------------------------
CREATE TABLE [dbo].[AgentBadgeTransaction](
	[Id] uniqueidentifier NOT NULL,
    [Person] uniqueidentifier NOT NULL,
    [BadgeType] int NOT NULL, --AnsweredCallsBadge=0,AverageHandlingTimeBadge=1,AdherenceBadge=2
    [Amount] smallint,
    [CalculatedDate] datetime NOT NULL,
    [Description] nvarchar(50) NOT NULL,
    [InsertedOn] datetime NOT NULL
)
GO

ALTER TABLE [dbo].[AgentBadgeTransaction] ADD  CONSTRAINT [PK_AgentBadgeTransaction] PRIMARY KEY NONCLUSTERED 
(
    [Id] ASC
)
GO

ALTER TABLE [dbo].[AgentBadgeTransaction] WITH CHECK ADD CONSTRAINT [FK_AgentBadgeTransaction_Person_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[AgentBadgeTransaction] CHECK CONSTRAINT [FK_AgentBadgeTransaction_Person_Person]
GO

CREATE UNIQUE CLUSTERED INDEX [CIX_AgentBadgeTransaction] ON [dbo].[AgentBadgeTransaction]
(
    [Person] ASC,
    [BadgeType] ASC,
    [CalculatedDate] ASC
)
GO

IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[AgentBadge]'))
DROP TABLE [dbo].[AgentBadge]
GO

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[AgentBadge]'))
DROP VIEW [dbo].[AgentBadge]
GO

CREATE VIEW [dbo].[AgentBadge]
WITH SCHEMABINDING
AS
SELECT Person,
       BadgeType,
       sum(Amount) AS 'TotalAmount',
       max(CalculatedDate) AS 'LastCalculatedDate'
  FROM dbo.AgentBadgeTransaction
GROUP BY person, BadgeType
GO
