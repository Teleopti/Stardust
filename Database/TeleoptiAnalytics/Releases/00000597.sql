-- For PBI #46841
CREATE TABLE [dbo].[ExternalPerformanceData](
	[ExternalPerformanceId] [int] NOT NULL,
	[DateFrom] [smalldatetime] NOT NULL,
	[PersonId] [int] NOT NULL,
	[OriginalPersonId] [nvarchar](100) NOT NULL,
	[Score] [real] NULL
 CONSTRAINT [PK_ExternalPerformanceData] PRIMARY KEY CLUSTERED 
(
	[ExternalPerformanceid] ASC,
	[DateFrom] ASC,
	[PersonId] ASC
)) ON [PRIMARY]
GO
