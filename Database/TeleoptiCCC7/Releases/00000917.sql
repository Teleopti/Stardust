----------------  
--Name: Dragonfly
--Date: 2017-12-07
--Desc: Recreate table ExternalPerformanceData to readModel
----------------  


DROP TABLE [Dbo].[ExternalPerformanceData]

CREATE TABLE [ReadModel].[ExternalPerformanceData](
    [ExternalPerformance] [uniqueidentifier] NOT NULL,
    [DateFrom] [smalldatetime] NOT NULL,
    [Person] [uniqueidentifier] NOT NULL,
    [OriginalPersonId] [nvarchar](130) NOT NULL,
    [Score] [real] NOT NULL
 CONSTRAINT [PK_ReadModel_ExternalPerformanceData] PRIMARY KEY CLUSTERED
(
    [ExternalPerformance] ASC,
    [DateFrom] ASC,
    [Person] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [ReadModel].[ExternalPerformanceData] WITH CHECK ADD CONSTRAINT [FK_ReadModel_ExternalPerformanceData_ExternalPerformance] FOREIGN KEY([ExternalPerformance])
REFERENCES [dbo].[ExternalPerformance] ([Id])
GO

ALTER TABLE [ReadModel].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ReadModel_ExternalPerformanceData_ExternalPerformance]
GO

ALTER TABLE [ReadModel].[ExternalPerformanceData] WITH CHECK ADD CONSTRAINT [FK_ReadModel_ExternalPerformanceData_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [ReadModel].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ReadModel_ExternalPerformanceData_Person]
GO
