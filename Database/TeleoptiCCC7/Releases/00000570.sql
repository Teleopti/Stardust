ALTER TABLE [dbo].[Activity] ADD [IsOutboundActivity] bit NULL
GO

UPDATE [dbo].[Activity]
SET [IsOutboundActivity] = 0
GO

ALTER TABLE [dbo].[Activity] ALTER COLUMN [IsOutboundActivity] bit NOT NULL