ALTER TABLE [dbo].[BankHolidayDate] ADD CONSTRAINT UQ_Date_Calendar UNIQUE NONCLUSTERED 
(
	[Date],
	[CalendarId]
)
GO