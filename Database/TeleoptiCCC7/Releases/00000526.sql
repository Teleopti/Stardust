-- =============================================
-- Author:		Chundan Xu
-- Create date: 2015-02-09
-- Description:	Bug #32258.
-- =============================================
ALTER TABLE [dbo].[GamificationSetting]
ALTER COLUMN [AdherenceThreshold] decimal(7,4)
GO

ALTER TABLE [dbo].[GamificationSetting]
ALTER COLUMN [AdherenceBronzeThreshold] decimal(7,4)
GO

ALTER TABLE [dbo].[GamificationSetting]
ALTER COLUMN [AdherenceSilverThreshold] decimal(7,4)
GO

ALTER TABLE [dbo].[GamificationSetting]
ALTER COLUMN [AdherenceGoldThreshold] decimal(7,4)
GO