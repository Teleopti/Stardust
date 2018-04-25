  ----------------  
--Name: Mingdi
--Date: 2018-04-25
--Desc: enlarge the value for imported number and thresholds
---------------- 

ALTER TABLE [dbo].[ExternalPerformanceData]
DROP COLUMN  [Score]
GO
ALTER TABLE [dbo].[ExternalPerformanceData] ADD [Score] DECIMAL (10,4) NOT NULL
GO

ALTER TABLE [dbo].[BadgeSetting]
DROP COLUMN  [Threshold]
GO
ALTER TABLE [dbo].[BadgeSetting] ADD [Threshold] DECIMAL (10,4) NOT NULL
GO

ALTER TABLE [dbo].[BadgeSetting]
DROP COLUMN  [BronzeThreshold]
GO
ALTER TABLE [dbo].[BadgeSetting] ADD [BronzeThreshold] DECIMAL (10,4) NOT NULL
GO

ALTER TABLE [dbo].[BadgeSetting]
DROP COLUMN  [SilverThreshold]
GO
ALTER TABLE [dbo].[BadgeSetting] ADD [SilverThreshold] DECIMAL (10,4) NOT NULL
GO

ALTER TABLE [dbo].[BadgeSetting]
DROP COLUMN  [GoldThreshold]
GO
ALTER TABLE [dbo].[BadgeSetting] ADD [GoldThreshold] DECIMAL (10,4) NOT NULL
GO
