
ALTER TABLE msg.MailboxNotification 
ADD [Id] [int] IDENTITY(1,1) NOT NULL 
CONSTRAINT [PK_MailboxNotification] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
GO
