exec('CREATE SCHEMA [msg] AUTHORIZATION [dbo]')

ALTER SCHEMA msg 
    TRANSFER dbo.Mailbox;

ALTER SCHEMA msg 
    TRANSFER dbo.[Notification];


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
	[Id] [uniqueidentifier] NOT NULL default NEWID(),
	[Parent] [uniqueidentifier] NOT NULL,
	[Message] [varchar](max) NOT NULL

CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

ALTER TABLE [msg].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_Mailbox] FOREIGN KEY([Parent])
REFERENCES [msg].[Mailbox] ([Id])
ALTER TABLE [msg].[Notification] CHECK CONSTRAINT [FK_Notification_Mailbox]

