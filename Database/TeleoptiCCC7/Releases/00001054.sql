IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankHolidayDates]') AND type in (N'U'))
   DROP TABLE [dbo].[BankHolidayDates]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankHolidayCalendar]') AND type in (N'U'))
   DROP TABLE [dbo].[BankHolidayCalendar]
GO

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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
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


CREATE TABLE [dbo].[BankHolidayDate](
	[Id] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Description] [nvarchar](250) NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CalendarId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_BankHolidayDate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BankHolidayDate]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayDate_BankHolidayCalendar] FOREIGN KEY([CalendarId])
REFERENCES [dbo].[BankHolidayCalendar] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayDate] CHECK CONSTRAINT [FK_BankHolidayDate_BankHolidayCalendar]
GO

ALTER TABLE [dbo].[BankHolidayDate]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayDate_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayDate] CHECK CONSTRAINT [FK_BankHolidayDate_UpdatedBy]
GO





