----------------  
--Name: Mingdi
--Date: 2014-12-25
--Desc: Add new column for table ShiftExchangeOffer to reference request
---------------- 

IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[ShiftExchangeOffer]'))
   DROP TABLE [dbo].[ShiftExchangeOffer]
GO

CREATE TABLE [dbo].[ShiftExchangeOffer](
	[Request] [uniqueidentifier] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Checksum] [bigint] NOT NULL,
	[MyShiftStartDateTime] [datetime] NULL,
	[MyShiftEndDateTime] [datetime] NULL,
	[ValidTo] [datetime] NOT NULL,
	[ShiftWithinStartDateTime] [datetime] NULL,
	[ShiftWithinEndDateTime] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_ShiftExchangeOffer] PRIMARY KEY CLUSTERED 
(
	[Request] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ShiftExchangeOffer]  WITH CHECK ADD  CONSTRAINT [FK_ShiftExchangeOffer_Request] FOREIGN KEY([Request])
REFERENCES [dbo].[Request] ([Id])
GO

ALTER TABLE [dbo].[ShiftExchangeOffer] CHECK CONSTRAINT [FK_ShiftExchangeOffer_Request]
GO

ALTER TABLE [dbo].[ShiftExchangeOffer]  WITH CHECK ADD  CONSTRAINT [FK_ShiftExchangeOffer_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ShiftExchangeOffer] CHECK CONSTRAINT [FK_ShiftExchangeOffer_Person]
GO


