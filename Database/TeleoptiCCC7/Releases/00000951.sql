ALTER TABLE [dbo].[ExternalPerformanceData]
DROP COLUMN  [Score]
GO
ALTER TABLE [dbo].[ExternalPerformanceData] ADD [Score] DECIMAL (7,4) NOT NULL
GO

ALTER TABLE [dbo].[BadgeSetting]
DROP COLUMN  [Threshold]
GO
ALTER TABLE [dbo].[BadgeSetting] ADD [Threshold] DECIMAL (7,4) NOT NULL
GO

ALTER TABLE [dbo].[BadgeSetting]
DROP COLUMN  [BronzeThreshold]
GO
ALTER TABLE [dbo].[BadgeSetting] ADD [BronzeThreshold] DECIMAL (7,4) NOT NULL
GO

ALTER TABLE [dbo].[BadgeSetting]
DROP COLUMN  [SilverThreshold]
GO
ALTER TABLE [dbo].[BadgeSetting] ADD [SilverThreshold] DECIMAL (7,4) NOT NULL
GO

ALTER TABLE [dbo].[BadgeSetting]
DROP COLUMN  [GoldThreshold]
GO
ALTER TABLE [dbo].[BadgeSetting] ADD [GoldThreshold] DECIMAL (7,4) NOT NULL
GO
