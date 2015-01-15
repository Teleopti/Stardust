----------------  
--Name: Chundan Xu, Zhiping Lan
--Date: 2015-1-14
--Desc: remove Version column
---------------- 
IF EXISTS(SELECT * FROM sys.columns 
   WHERE [name] = N'Version' AND [object_id] = OBJECT_ID(N'[dbo].[GamificationSetting]'))
BEGIN
	ALTER TABLE [dbo].[GamificationSetting]
	DROP COLUMN [Version]
END
GO

----------------  
--Name: Team Real Time
--Desc: Read model column names and new column
----------------  

DROP TABLE [ReadModel].[SiteAdherence]
GO

CREATE TABLE [ReadModel].[SiteOutOfAdherence](
	[SiteId] [uniqueidentifier] NULL,
	[BusinessUnitId] [uniqueidentifier] NULL,
	[Count] [int] NULL,
	[PersonIds] [varchar](MAX) NULL
)
GO
