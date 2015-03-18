
CREATE TABLE [dbo].[SeatBooking](
	[Id] [uniqueidentifier] NOT NULL,
	[Seat] [uniqueidentifier] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	
CONSTRAINT [PK_SeatBooking] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SeatBooking] WITH CHECK ADD  
CONSTRAINT [FK_SeatBooking_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);
GO
ALTER TABLE [dbo].[SeatBooking] CHECK CONSTRAINT [FK_SeatBooking_BusinessUnit]
GO

ALTER TABLE [dbo].[SeatBooking] WITH NOCHECK ADD  
CONSTRAINT [FK_SeatBooking_Seat] FOREIGN KEY(Seat)
REFERENCES [dbo].[Seat] ([Id]);
GO
ALTER TABLE [dbo].[Seatbooking] CHECK CONSTRAINT [FK_SeatBooking_Seat]
GO

ALTER TABLE [dbo].[SeatBooking] WITH NOCHECK ADD  
CONSTRAINT [FK_SeatBooking_Person] FOREIGN KEY(Person)
REFERENCES [dbo].[Person] ([Id]);
GO
ALTER TABLE [dbo].[SeatBooking] CHECK CONSTRAINT [FK_SeatBooking_Person]
GO
