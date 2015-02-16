-- =============================================
-- Author: Xianwei Shen
-- Create date: 2015-02-16
-- Description:	remove unused column
-- =============================================
DELETE [ReadModel].[AdherencePercentage]
GO

ALTER TABLE [ReadModel].[AdherencePercentage] DROP COLUMN [ShiftEndTime]
GO