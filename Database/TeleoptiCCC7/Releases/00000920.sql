----------------  
--Name: Dragonfly
--Date: 2017-12-18
--Desc: Use PersonId instead of Person
----------------  

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExternalPerformanceData]') AND type in (N'U'))
   DROP TABLE [dbo].[ExternalPerformanceData]
GO

CREATE TABLE [dbo].[ExternalPerformanceData](
	[Id] [uniqueidentifier] NOT NULL,
	[ExternalPerformance] [uniqueidentifier] NOT NULL,
	[DateFrom] [datetime] NOT NULL,
	[PersonId] [uniqueidentifier] NOT NULL,
	[OriginalPersonId] [nvarchar](130) NOT NULL,
	[Score] [int] NULL,
 CONSTRAINT [PK_ExternalPerformanceData] PRIMARY KEY CLUSTERED 
(
[Id] ASC
),
CONSTRAINT [UQ_ExternalPerformanceData] UNIQUE NONCLUSTERED 
(
	[ExternalPerformance] ASC,
	[DateFrom] ASC,
	[PersonId] ASC
)) ON [PRIMARY]
GO


ALTER TABLE [dbo].[ExternalPerformanceData]  WITH CHECK ADD  CONSTRAINT [FK_ExternalPerformanceData_ExternalPerformance] FOREIGN KEY([ExternalPerformance])
REFERENCES [dbo].[ExternalPerformance] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ExternalPerformanceData_ExternalPerformance]
GO

ALTER TABLE [dbo].[ExternalPerformanceData]  WITH CHECK ADD  CONSTRAINT [FK_ExternalPerformanceData_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ExternalPerformanceData_PersonId]
GO


