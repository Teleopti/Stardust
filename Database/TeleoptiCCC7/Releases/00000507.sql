----------------  
--Name: Mingdi
--Date: 2014-12-25
--Desc: Add new column for table ShiftExchangeOffer to reference request
---------------- 

drop table dbo.ShiftExchangeOffer


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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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


