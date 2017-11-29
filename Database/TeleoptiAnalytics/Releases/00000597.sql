-- For PBI #46841
-- Still in development, not released; if it already exists, we should drop it.
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExternalPerformanceData]') AND type in (N'U'))
   DROP TABLE [dbo].[ExternalPerformanceData]
GO