CREATE TABLE [dbo].[Mailbox](
	[Id] [uniqueidentifier] NOT NULL,
	[Route] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Mailbox] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Notification](
	[Parent] [uniqueidentifier] NOT NULL,
	[Message] [varchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_Mailbox] FOREIGN KEY([Parent])
REFERENCES [dbo].[Mailbox] ([Id])

ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Notification_Mailbox]


