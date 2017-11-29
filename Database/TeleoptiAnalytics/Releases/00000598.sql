CREATE TABLE [msg].[ASMScheduleChangeTime](
	[PersonId] [uniqueidentifier] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	 CONSTRAINT [PK_ASMScheduleChangeTime] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC
)
)
