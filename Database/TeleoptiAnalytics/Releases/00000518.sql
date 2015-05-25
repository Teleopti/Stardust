CREATE TABLE [dbo].[Mailbox](
	[Id] [uniqueidentifier] NOT NULL,
	[Route] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Mailbox] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

CREATE TABLE [dbo].[Notification](
	[Parent] [uniqueidentifier] NOT NULL,
	[Message] [varchar](max) NOT NULL
)

ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_Mailbox] FOREIGN KEY([Parent])
REFERENCES [dbo].[Mailbox] ([Id])

ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Notification_Mailbox]


