IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExternalPerformanceData]') AND type in (N'U'))
   DROP TABLE [dbo].[ExternalPerformanceData]
GO


IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExternalPerformance]') AND type in (N'U'))
   DROP TABLE [dbo].[ExternalPerformance]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BadgeSetting]') AND type in (N'U'))
   DROP TABLE [dbo].[BadgeSetting]
GO

-----------------------------------------------------------------------------------------------------
--CREATE ExternalPerformance TABLE-------
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

ALTER TABLE [dbo].[ExternalPerformance] ADD CONSTRAINT [UNIQUE_BU_EXTERNALID] UNIQUE NONCLUSTERED 
(
    [BusinessUnit] ASC,
    [ExternalId] ASC
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


-----------------------------------------------------------------------------------------------------
--CREATE ExternalPerformanceData TABLE-------

CREATE TABLE [dbo].[ExternalPerformanceData](
	[Id] uniqueidentifier NOT NULL,
    [ExternalPerformance] [uniqueidentifier] NOT NULL,
    [DateFrom] [smalldatetime] NOT NULL,
    [PersonId] [uniqueidentifier] NOT NULL,
    [OriginalPersonId] [nvarchar](200) NOT NULL,
    [Score] FLOAT NOT NULL
	CONSTRAINT [PK_ExternalPerformanceData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExternalPerformanceData] ADD CONSTRAINT [UQ_ExternalPerformanceData] UNIQUE NONCLUSTERED 
(
	[ExternalPerformance] ASC,
	[DateFrom] ASC,
	[PersonId] ASC
)

ALTER TABLE [dbo].[ExternalPerformanceData] WITH CHECK ADD CONSTRAINT [FK_ExternalPerformanceData_ExternalPerformance] FOREIGN KEY([ExternalPerformance])
REFERENCES [dbo].[ExternalPerformance] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ExternalPerformanceData_ExternalPerformance]
GO

ALTER TABLE [dbo].[ExternalPerformanceData] WITH CHECK ADD CONSTRAINT [FK_ExternalPerformanceData_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ExternalPerformanceData] CHECK CONSTRAINT [FK_ExternalPerformanceData_Person]
GO

-----------------------------------------------------------------------------------------------------
--CREATE BadgeSetting TABLE-------

CREATE TABLE [dbo].[BadgeSetting](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[QualityId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Enabled] [bit] NOT NULL,
	[LargerIsBetter] [bit] NOT NULL,
	[DataType] [smallint] NOT NULL,
	[Threshold] FLOAT NULL,
	[BronzeThreshold] FLOAT NULL,
	[SilverThreshold] FLOAT NULL,
	[GoldThreshold] FLOAT NULL,
CONSTRAINT [PK_BadgeSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BadgeSetting] WITH NOCHECK ADD CONSTRAINT [FK_BadgeSetting_GamificationSetting] FOREIGN KEY([Parent])
REFERENCES [dbo].[GamificationSetting] ([Id])
GO

ALTER TABLE [dbo].[BadgeSetting] CHECK CONSTRAINT [FK_BadgeSetting_GamificationSetting]
GO

ALTER TABLE [dbo].[BadgeSetting] ADD CONSTRAINT[UNIQUE_PARENT_QUALITYID] UNIQUE NONCLUSTERED 
(
	[Parent] ASC,
	[QualityId] ASC
)
GO