CREATE TABLE [dbo].[BankHolidayCalendar](
            [Id] [uniqueidentifier] NOT NULL,
			[Name] [nvarchar](250) NOT NULL,
			[BusinessUnit] [uniqueidentifier] NOT NULL,
			[UpdatedBy] [uniqueidentifier] NOT NULL,
			[UpdatedOn] [datetime] NOT NULL,
			[IsDeleted] [bit] NOT NULL,
CONSTRAINT [PK_BankHolidayCalendar] PRIMARY KEY CLUSTERED 
(
            [Id] ASC
)
)
GO

ALTER TABLE [dbo].[BankHolidayCalendar]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayCalendar_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayCalendar] CHECK CONSTRAINT [FK_BankHolidayCalendar_BusinessUnit]
GO

ALTER TABLE [dbo].[BankHolidayCalendar]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayCalendar_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayCalendar] CHECK CONSTRAINT [FK_BankHolidayCalendar_UpdatedBy]
GO

CREATE TABLE [dbo].[BankHolidayDates](
	[Parent] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
 CONSTRAINT [PK_BankHolidayDates] PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


ALTER TABLE [dbo].[BankHolidayDates]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayDates_BankHolidayCalendar] FOREIGN KEY([Parent])
REFERENCES [dbo].[BankHolidayCalendar] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayDates] CHECK CONSTRAINT [FK_BankHolidayDates_BankHolidayCalendar]
GO