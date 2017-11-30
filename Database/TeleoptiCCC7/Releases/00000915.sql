DROP TABLE [dbo].[ASMScheduleChangeTime]
CREATE TABLE [ReadModel].[ASMScheduleChangeTime](
	[PersonId] [uniqueidentifier] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	 CONSTRAINT [PK_ASMScheduleChangeTime] PRIMARY KEY CLUSTERED 
	(
		[PersonId] ASC
	)
)
GO