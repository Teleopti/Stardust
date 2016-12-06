CREATE TABLE [msg].[MailboxNotification](
	[MailboxId] [uniqueidentifier] NOT NULL,
	[DataSource] [nvarchar](255) NULL,
	[BusinessUnitId] [nvarchar](255) NULL,
	[DomainType] [nvarchar](255) NULL,
	[DomainQualifiedType] [nvarchar](255) NULL,
	[DomainId] [nvarchar](255) NULL,
	[ModuleId] [nvarchar](255) NULL,
	[DomainReferenceId] [nvarchar](255) NULL,
	[EndDate] [nvarchar](255) NULL,
	[StartDate] [nvarchar](255) NULL,
	[DomainUpdateType] [int] NULL,
	[BinaryData] [nvarchar](max) NULL,
	[TrackId] [nvarchar](255) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [msg].[MailboxNotification]  WITH CHECK ADD  CONSTRAINT [FK_MailboxNotification_Mailbox] FOREIGN KEY([MailboxId])
REFERENCES [msg].[Mailbox] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [msg].[MailboxNotification] CHECK CONSTRAINT [FK_MailboxNotification_Mailbox]
GO
