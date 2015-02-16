-- =============================================
-- Author: Asad MIrza
-- Description:	Added state column 
-- =============================================

DELETE [ReadModel].[SiteOutOfAdherence]
GO

ALTER TABLE [ReadModel].[SiteOutOfAdherence] DROP COLUMN PersonIds
GO

ALTER TABLE [ReadModel].[SiteOutOfAdherence] ADD [State] nvarchar(MAX) NULL
GO
