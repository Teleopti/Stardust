DROP TABLE [msg].[Notification]
DROP TABLE [msg].[Mailbox]

CREATE TABLE [msg].[Mailbox](
	[Id] [uniqueidentifier] NOT NULL,
	[Route] [varchar](400) NULL,
	[Notifications] [varchar](MAX) NULL,
 CONSTRAINT [PK_Mailbox] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
CREATE CLUSTERED INDEX [CIX_msg_Mailbox2] ON [msg].[Mailbox]
(
	[Route] ASC
)
