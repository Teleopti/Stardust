----------------  
--Name: Dragonfly
--Date: 2017-12-12
--Desc: Use dbo instead of read model for ExternalPerformanceData 
----------------  

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[ExternalPerformanceData]') AND type in (N'U'))
   DROP TABLE [ReadModel].[ExternalPerformanceData]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExternalPerformanceData]') AND type in (N'U'))
   DROP TABLE [dbo].[ExternalPerformanceData]
GO

CREATE TABLE [dbo].[ExternalPerformanceData](
	[Id] [uniqueidentifier] NOT NULL,
	[ExternalPerformance] [uniqueidentifier] NOT NULL,
	[DateFrom] [smalldatetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[OriginalPersonId] [nvarchar](130) NOT NULL,
	[Score] [int] NULL,
 CONSTRAINT [PK_ExternalPerformanceData] PRIMARY KEY CLUSTERED 
(
[Id] ASC
),
UNIQUE NONCLUSTERED 
(
	[ExternalPerformance] ASC,
	[DateFrom] ASC,
	[Person] ASC
)
) ON [PRIMARY]

GO


ALTER TABLE [dbo].[ExternalPerformanceData]  WITH CHECK ADD  CONSTRAINT [FK_ExternalPerformanceData_ExternalPerformance] FOREIGN KEY([ExternalPerformance])
REFERENCES [dbo].[ExternalPerformance] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ExternalPerformanceData_ExternalPerformance]
GO

ALTER TABLE [dbo].[ExternalPerformanceData]  WITH CHECK ADD  CONSTRAINT [FK_ExternalPerformanceData_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ExternalPerformanceData_Person]
GO


