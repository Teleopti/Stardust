
/*
Make sure that all persons in the database has write protection info. This is created automatically in the domain.
*/

INSERT INTO [dbo].[PersonWriteProtectionInfo] (Id,CreatedBy,UpdatedBy,CreatedOn,UpdatedOn,PersonWriteProtectedDate) SELECT p.Id,p.CreatedBy,p.UpdatedBy,p.CreatedOn,p.UpdatedOn,null FROM [dbo].[Person] p WHERE p.Id NOT IN (SELECT id FROM [dbo].[PersonWriteProtectionInfo])
GO

-- =============================================
-- Author:		Ola
-- Create date: 2012-02-28
-- Description:	New LicenseStatus table
-- =============================================
CREATE TABLE [dbo].[LicenseStatus](
	[Id] [uniqueidentifier] NOT NULL,
	[XmlString] [nvarchar](4000) NOT NULL,
	CONSTRAINT PK_LicenseStatus PRIMARY KEY CLUSTERED (Id))
GO




-- =============================================
-- Author:		Talha
-- Create date: 2012-3-05
-- Description:	save forecasts file into database
-- =============================================
CREATE TABLE [dbo].[ForecastFile](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[FileName] [nvarchar](255) NULL,
	[FileContent] [varbinary](MAX) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ForecastFile]  WITH CHECK ADD  CONSTRAINT [FK_ForecastFile_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
ALTER TABLE [dbo].[ForecastFile] CHECK CONSTRAINT [FK_ForecastFile_BusinessUnit]
ALTER TABLE [dbo].[ForecastFile]  WITH CHECK ADD  CONSTRAINT [FK_ForecastFile_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[ForecastFile] CHECK CONSTRAINT [FK_ForecastFile_Person_CreatedBy]
ALTER TABLE [dbo].[ForecastFile]  WITH CHECK ADD  CONSTRAINT [FK_ForecastFile_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[ForecastFile] CHECK CONSTRAINT [FK_ForecastFile_Person_UpdatedBy]
GO


