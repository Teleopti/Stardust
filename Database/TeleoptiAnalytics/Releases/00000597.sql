CREATE TABLE [msg].[PersonScheduleChangeMessage](
	[Id] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[PersonId] [uniqueidentifier] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	 CONSTRAINT [PK_PersonScheduleChangeMessage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [MSG]

CREATE NONCLUSTERED INDEX [IX_PersonScheduleChangeMessage_PersonId] ON [msg].[PersonScheduleChangeMessage] 
(
	[PersonId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]
GO