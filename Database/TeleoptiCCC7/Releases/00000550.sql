
DROP TABLE [dbo].[Seat]
DROP TABLE [dbo].[SeatMapLocation]
DROP TABLE [dbo].[SeatMap]

GO

CREATE TABLE [dbo].[SeatMapLocation](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[ParentLocation] [uniqueidentifier] NULL,
	[SeatMapJsonData] [nvarchar](max) NOT NULL,
	
CONSTRAINT [PK_SeatMapLocation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Seat](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Priority] [int] NOT NULL

CONSTRAINT [PK_Seat] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO


ALTER TABLE [dbo].[SeatMapLocation] WITH CHECK ADD  CONSTRAINT [FK_SeatMapLocation_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);
GO
ALTER TABLE [dbo].[SeatMapLocation] CHECK CONSTRAINT [FK_SeatMapLocation_BusinessUnit]
GO

ALTER TABLE [dbo].[Seat] WITH NOCHECK ADD  CONSTRAINT [FK_Seat_SeatMapLocation] FOREIGN KEY([Parent])
REFERENCES [dbo].[SeatMapLocation] ([Id]);
GO
ALTER TABLE [dbo].[Seat] CHECK CONSTRAINT [FK_Seat_SeatMapLocation]
GO


