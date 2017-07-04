CREATE TABLE [dbo].[QueuedOvertimeRequest](
	[Id] [uniqueidentifier] NOT NULL,
	[PersonRequest] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Sent] [datetime] NULL
 CONSTRAINT [PK_QueuedOvertimeRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO


