ALTER TABLE [dbo].[ShiftLayer]
  ADD [Meeting] [uniqueidentifier];


CREATE TABLE [dbo].[ExternalMeeting](
	[Id] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](max) NULL,
	[Agenda] [nvarchar](max) NULL,
	CONSTRAINT [PK_ExternalMeeting] PRIMARY KEY CLUSTERED 
	(
		Id ASC
	)
)  ON [PRIMARY]
GO


ALTER TABLE [dbo].[ShiftLayer]  WITH CHECK ADD CONSTRAINT [FK_MeetingShiftLayer_ExternalMeeting] FOREIGN KEY([Meeting])
REFERENCES [dbo].[ExternalMeeting] ([Id])
GO

ALTER TABLE [dbo].[ShiftLayer] CHECK CONSTRAINT [FK_MeetingShiftLayer_ExternalMeeting]
GO