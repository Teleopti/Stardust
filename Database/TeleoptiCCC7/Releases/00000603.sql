
CREATE TABLE [dbo].[SeatPlan](
	[Id] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Status] [int] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	
CONSTRAINT [PK_SeatPlan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SeatPlan] WITH CHECK ADD  
CONSTRAINT [FK_SeatPlan_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id]);
GO
ALTER TABLE [dbo].[SeatPlan] CHECK CONSTRAINT [FK_SeatPlan_BusinessUnit]
GO