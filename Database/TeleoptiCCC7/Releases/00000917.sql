----------------  
--Name: Dragonfly
--Date: 2017-12-07
--Desc: Recreate table ExternalPerformanceData to readModel
----------------  


DROP TABLE [Dbo].[ExternalPerformanceData]

CREATE TABLE [ReadModel].[ExternalPerformanceData](
	[ExternalPerformance] [uniqueidentifier] NULL,
	[DateFrom] [smalldatetime] NULL,
	[Person] [uniqueidentifier] NULL,
	[OriginalPersonId] [nvarchar](130) NULL,
	[Score] [int] NULL
) ON [PRIMARY]

GO
