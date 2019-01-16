IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankHolidayCalendarsForSite]') AND type in (N'U'))
   DROP TABLE [dbo].[BankHolidayCalendarsForSite]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SiteBankHolidayCalendar]') AND type in (N'U'))
   DROP TABLE [dbo].[SiteBankHolidayCalendar]
GO
