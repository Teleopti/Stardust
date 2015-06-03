DROP TABLE [msg].[Notification]
DROP TABLE [msg].[Mailbox]

CREATE TABLE [msg].[Mailbox](
	[Id] [uniqueidentifier] NOT NULL,
	[Route] [varchar](400) NOT NULL,
 CONSTRAINT [PK_Mailbox] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
CREATE CLUSTERED INDEX [CIX_msg_Mailbox] ON [msg].[Mailbox]
(
	[Route] ASC
)

CREATE TABLE [msg].[Notification](
	[Id] [uniqueidentifier] NOT NULL,
	[MailboxParent] [uniqueidentifier] NOT NULL,
	[DataSource] [nvarchar](50) NULL,
	[BusinessUnitId] [uniqueidentifier] NULL,
	[DomainType] [varchar](100) NULL,
	[DomainQualifiedType] [varchar](100) NULL,
	[DomainId] [uniqueidentifier] NULL,
	[ModuleId] [uniqueidentifier] NULL,
	[DomainReferenceId] [uniqueidentifier] NULL,
	[DomainReferenceType] [varchar](100) NULL,
	[EndDate] datetime NULL,
	[StartDate] datetime NULL,
	[DomainUpdateType] int NULL,
	[BinaryData] [nvarchar](max) NULL
CONSTRAINT [PK_Notification] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
CREATE CLUSTERED INDEX [CIX_msg_Notification] ON [msg].[Notification]
(
	[MailboxParent] ASC
)

ALTER TABLE [msg].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_Mailbox] FOREIGN KEY([MailboxParent])
REFERENCES [msg].[Mailbox] ([Id])
ON DELETE CASCADE
ALTER TABLE [msg].[Notification] CHECK CONSTRAINT [FK_Notification_Mailbox]

