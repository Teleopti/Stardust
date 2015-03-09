----------------  
--Name: Rob Wright
--Date: 2015-2-27
--Desc: Add new tables for Seat Maps and Locations
---------------- 
CREATE TABLE [dbo].[SeatMap](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[SeatMapJsonData] [nvarchar](max) NOT NULL,
	
CONSTRAINT [PK_SeatMap] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SeatMapLocation](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[ParentLocation] [uniqueidentifier] NULL,
	
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


ALTER TABLE [dbo].[SeatMap] WITH CHECK ADD  CONSTRAINT [FK_SeatMap_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);
GO
ALTER TABLE [dbo].[SeatMap] CHECK CONSTRAINT [FK_SeatMap_BusinessUnit]
GO

ALTER TABLE [dbo].[SeatMapLocation] WITH NOCHECK ADD  CONSTRAINT [FK_SeatMapLocation_SeatMap] FOREIGN KEY([Parent])
REFERENCES [dbo].[SeatMap] ([Id]);
GO
ALTER TABLE [dbo].[SeatMapLocation] CHECK CONSTRAINT [FK_SeatMapLocation_SeatMap]
GO

ALTER TABLE [dbo].[Seat] WITH NOCHECK ADD  CONSTRAINT [FK_Seat_SeatMapLocation] FOREIGN KEY([Parent])
REFERENCES [dbo].[SeatMapLocation] ([Id]);
GO
ALTER TABLE [dbo].[Seat] CHECK CONSTRAINT [FK_Seat_SeatMapLocation]
GO


