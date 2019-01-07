----------------  
--Name: Mingdi
--Date: 2019-01-03
--Desc: Add new table for store connection between Site and BankHolidayCanlendar
----------------  
CREATE TABLE [dbo].[SiteBankHolidayCalendar](
	[Id] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Site] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_SiteBankHolidayCalendar] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)) 
GO

ALTER TABLE [dbo].[SiteBankHolidayCalendar]  WITH CHECK ADD  CONSTRAINT [FK_SiteBankHolidayCalendar_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[SiteBankHolidayCalendar] CHECK CONSTRAINT [FK_SiteBankHolidayCalendar_BusinessUnit]
GO

ALTER TABLE [dbo].[SiteBankHolidayCalendar]  WITH CHECK ADD  CONSTRAINT [FK_SiteBankHolidayCalendar_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[SiteBankHolidayCalendar] CHECK CONSTRAINT [FK_SiteBankHolidayCalendar_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[SiteBankHolidayCalendar]  WITH CHECK ADD  CONSTRAINT [FK_SiteBankHolidayCalendar_Site] FOREIGN KEY([Site])
REFERENCES [dbo].[Site] ([Id])
GO

ALTER TABLE [dbo].[SiteBankHolidayCalendar] CHECK CONSTRAINT [FK_SiteBankHolidayCalendar_Site]
GO

CREATE TABLE [dbo].[BankHolidayCalendarsForSite](
	[Parent] [uniqueidentifier] NOT NULL,
	[BankHolidayCalendar] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_BankHolidayCalendarsForSite] PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[BankHolidayCalendar] ASC
)) 
GO


ALTER TABLE [dbo].[BankHolidayCalendarsForSite]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayCalendarsForSite_SiteBankHolidayCalendar] FOREIGN KEY([Parent])
REFERENCES [dbo].[SiteBankHolidayCalendar] ([Id])
GO
ALTER TABLE [dbo].[BankHolidayCalendarsForSite] CHECK CONSTRAINT [FK_BankHolidayCalendarsForSite_SiteBankHolidayCalendar]
GO

ALTER TABLE [dbo].[BankHolidayCalendarsForSite]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayCalendarsForSite_BankHolidayCalendar] FOREIGN KEY([BankHolidayCalendar])
REFERENCES [dbo].[BankHolidayCalendar] ([Id])
GO
ALTER TABLE [dbo].[BankHolidayCalendarsForSite] CHECK CONSTRAINT [FK_BankHolidayCalendarsForSite_BankHolidayCalendar]
GO