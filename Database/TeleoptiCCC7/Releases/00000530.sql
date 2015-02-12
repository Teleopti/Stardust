----------------  
--Name: CS
--Date: 2015-02-12
--Desc: create seniority work day ranks table 
---------------- 
CREATE TABLE [dbo].[SeniorityWorkDayRanks](
	[Id] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Monday] [int] NOT NULL,
	[Tuesday] [int] NOT NULL,
	[Wednesday] [int] NOT NULL,
	[Thursday] [int] NOT NULL,
	[Friday] [int] NOT NULL,
	[Saturday] [int] NOT NULL,
	[Sunday] [int] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_SeniorityWorkDayRanks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SeniorityWorkDayRanks]  WITH CHECK ADD  CONSTRAINT [FK_SeniorityWorkDayRanks_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[SeniorityWorkDayRanks] CHECK CONSTRAINT [FK_SeniorityWorkDayRanks_BusinessUnit]
GO

ALTER TABLE [dbo].[SeniorityWorkDayRanks]  WITH CHECK ADD  CONSTRAINT [FK_SeniorityWorkDayRanks_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[SeniorityWorkDayRanks] CHECK CONSTRAINT [FK_SeniorityWorkDayRanks_Person_UpdatedBy]
GO




