CREATE TABLE [dbo].[InsightsReport](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Version] [int] NOT NULL,
	[IsBuildIn] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_InsightsReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [dbo].[InsightsReport] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO

ALTER TABLE [dbo].[InsightsReport]  WITH CHECK ADD  CONSTRAINT [FK_InsightsReport_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[InsightsReport] CHECK CONSTRAINT [FK_InsightsReport_Person_CreatedBy]
GO

ALTER TABLE [dbo].[InsightsReport]  WITH CHECK ADD  CONSTRAINT [FK_InsightsReport_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[InsightsReport] CHECK CONSTRAINT [FK_InsightsReport_Person_UpdatedBy]
GO
