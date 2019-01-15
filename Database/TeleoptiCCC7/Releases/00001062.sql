CREATE TABLE [dbo].[BankHolidayCalendarSite](
	[Id] [uniqueidentifier] NOT NULL,
	[Site] [uniqueidentifier] NOT NULL,
	[Calendar] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_BankHolidayCalendarSite] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [UQ_Site_Calendar_BusinessUnit] UNIQUE NONCLUSTERED 
(
	[Site] ASC,
	[Calendar] ASC,
	[BusinessUnit] Asc
)
) 
GO

ALTER TABLE [dbo].[BankHolidayCalendarSite]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayCalendarSite_BankHolidayCalendar] FOREIGN KEY([Calendar])
REFERENCES [dbo].[BankHolidayCalendar] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayCalendarSite] CHECK CONSTRAINT [FK_BankHolidayCalendarSite_BankHolidayCalendar]
GO

ALTER TABLE [dbo].[BankHolidayCalendarSite]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayCalendarSite_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayCalendarSite] CHECK CONSTRAINT [FK_BankHolidayCalendarSite_BusinessUnit]
GO

ALTER TABLE [dbo].[BankHolidayCalendarSite]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayCalendarSite_Site] FOREIGN KEY([Site])
REFERENCES [dbo].[Site] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayCalendarSite] CHECK CONSTRAINT [FK_BankHolidayCalendarSite_Site]
GO

ALTER TABLE [dbo].[BankHolidayCalendarSite]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayCalendarSite_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayCalendarSite] CHECK CONSTRAINT [FK_BankHolidayCalendarSite_UpdatedBy]
GO


