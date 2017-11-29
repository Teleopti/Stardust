-- For PBI #46841
-- Still in development, not released; if it already exists, we can drop it directly
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExternalPerformance]') AND type in (N'U'))
   DROP TABLE [dbo].[ExternalPerformance]
GO

CREATE TABLE [dbo].[ExternalPerformance](
    [Id] uniqueidentifier NOT NULL,
    [BusinessUnit] [uniqueidentifier] NOT NULL,
    [ExternalId] [int] NOT NULL,
    [Name] [nvarchar](200) NOT NULL,
    [Datatype] smallint NOT NULL,
    [UpdatedBy] uniqueidentifier NOT NULL,
    [UpdatedOn] DATETIME NOT NULL,
    [IsDeleted] INT NOT NULL DEFAULT 0
 CONSTRAINT [PK_ExternalPerformanceInfo] PRIMARY KEY CLUSTERED
(
    [Id] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExternalPerformance] ADD UNIQUE NONCLUSTERED
(
    [BusinessUnit] ASC,
    [ExternalId] ASC
)
GO

ALTER TABLE [dbo].[ExternalPerformance] ADD UNIQUE NONCLUSTERED
(
    [BusinessUnit] ASC,
    [Name] ASC
)
GO

ALTER TABLE [dbo].[ExternalPerformance] WITH CHECK ADD CONSTRAINT [FK_ExternalPerformance_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformance] CHECK CONSTRAINT [FK_ExternalPerformance_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[ExternalPerformance] WITH CHECK ADD CONSTRAINT [FK_ExternalPerformance_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformance] CHECK CONSTRAINT [FK_ExternalPerformance_BusinessUnit]
GO


CREATE TABLE [dbo].[ExternalPerformanceData](
    [ExternalPerformance] [uniqueidentifier] NOT NULL,
    [DateFrom] [smalldatetime] NOT NULL,
    [Person] [uniqueidentifier] NOT NULL,
    [OriginalPersonId] [nvarchar](200) NOT NULL,
    [Score] [real] NULL
 CONSTRAINT [PK_ExternalPerformanceData] PRIMARY KEY CLUSTERED
(
    [ExternalPerformance] ASC,
    [DateFrom] ASC,
    [Person] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExternalPerformanceData] WITH CHECK ADD CONSTRAINT [FK_ExternalPerformanceData_ExternalPerformance] FOREIGN KEY([ExternalPerformance])
REFERENCES [dbo].[ExternalPerformance] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ExternalPerformanceData_ExternalPerformance]
GO

ALTER TABLE [dbo].[ExternalPerformanceData] WITH CHECK ADD CONSTRAINT [FK_ExternalPerformanceData_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ExternalPerformanceData_Person]
GO
