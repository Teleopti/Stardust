GO

/****** Object:  Table [dbo].[OvertimeRequestOpenPeriod]    Script Date: 10/26/2017 15:03:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OvertimeRequestOpenPeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[PeriodType] [nvarchar](255) NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[DaysMinimum] [int] NULL,
	[DaysMaximum] [int] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[AutoGrantType] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod]  WITH NOCHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriod_WorkflowControlSet] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod] CHECK CONSTRAINT [FK_OvertimeRequestOpenPeriod_WorkflowControlSet]
GO