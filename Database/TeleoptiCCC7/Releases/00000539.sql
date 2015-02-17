-- =============================================
-- Author: Asad Mirza
-- Description:	Added state column 
-- =============================================

DELETE [ReadModel].[TeamOutOfAdherence]
GO

ALTER TABLE [ReadModel].[TeamOutOfAdherence] DROP COLUMN PersonIds
GO

ALTER TABLE [ReadModel].[TeamOutOfAdherence] ADD [State] nvarchar(MAX) NULL
GO
