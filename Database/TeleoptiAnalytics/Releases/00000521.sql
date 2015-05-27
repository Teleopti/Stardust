DROP TABLE [msg].[Notification]
DROP TABLE [msg].[Mailbox]

CREATE TABLE [msg].[Mailbox](
	[Id] [uniqueidentifier] NOT NULL,
	[Route] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Mailbox] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
CREATE TABLE [msg].[Notification](
	[Id] [uniqueidentifier] NOT NULL,
	[MailboxParent] [uniqueidentifier] NOT NULL,
	[DataSource] [nvarchar](max) NULL,
	[BusinessUnitId] [nvarchar](36) NULL,
	[DomainType] [nvarchar](max) NULL,
	[DomainQualifiedType] [nvarchar](max) NULL,
	[DomainId] [nvarchar](36) NULL,
	[ModuleId] [nvarchar](36) NULL,
	[DomainReferenceId] [nvarchar](36) NULL,
	[DomainReferenceType] [nvarchar](max) NULL,
	[EndDate] [nvarchar](max) NULL,
	[StartDate] [nvarchar](max) NULL,
	[DomainUpdateType] int NULL,
	[BinaryData] [nvarchar](max) NULL
CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

ALTER TABLE [msg].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_Mailbox] FOREIGN KEY([MailboxParent])
REFERENCES [msg].[Mailbox] ([Id])
ALTER TABLE [msg].[Notification] CHECK CONSTRAINT [FK_Notification_Mailbox]

