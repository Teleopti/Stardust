-- For PBI #46841
CREATE TABLE [dbo].[ExternalPerformance](
    [Id] UniqueIdentifier NOT NULL,
    [ExternalId] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Datatype] smallint NOT NULL
 CONSTRAINT [PK_ExternalPerformanceInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExternalPerformance] ADD UNIQUE NONCLUSTERED 
(
	[ExternalId] ASC
)
GO

ALTER TABLE [dbo].[ExternalPerformance] ADD UNIQUE NONCLUSTERED 
(
	[Name] ASC
)
GO
