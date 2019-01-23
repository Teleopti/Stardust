IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankHolidayDate]') AND type in (N'U'))
   DROP TABLE [dbo].[BankHolidayDate]
GO

CREATE TABLE [dbo].[BankHolidayDate](
	[Id] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Description] [nvarchar](250) NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[Calendar] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_BankHolidayDate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [UQ_Date_Calendar] UNIQUE NONCLUSTERED 
(
	[Date] ASC,
	[Calendar] ASC
)
) 
GO

ALTER TABLE [dbo].[BankHolidayDate]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayDate_BankHolidayCalendar] FOREIGN KEY([Calendar])
REFERENCES [dbo].[BankHolidayCalendar] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayDate] CHECK CONSTRAINT [FK_BankHolidayDate_BankHolidayCalendar]
GO

ALTER TABLE [dbo].[BankHolidayDate]  WITH CHECK ADD  CONSTRAINT [FK_BankHolidayDate_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[BankHolidayDate] CHECK CONSTRAINT [FK_BankHolidayDate_UpdatedBy]
GO


