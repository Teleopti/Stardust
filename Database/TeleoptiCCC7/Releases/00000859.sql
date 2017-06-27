CREATE TABLE [dbo].[OvertimeRequest](
	[Request] [uniqueidentifier] NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL,
	[OvertimeType] [int] NOT NULL
 CONSTRAINT [PK_OvertimeRequest] PRIMARY KEY CLUSTERED 
(
	[Request] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[OvertimeRequest]  WITH NOCHECK ADD  CONSTRAINT [FK_OvertimeRequest_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO

ALTER TABLE [dbo].[OvertimeRequest] CHECK CONSTRAINT [FK_OvertimeRequest_Activity]
GO

ALTER TABLE [dbo].[OvertimeRequest]  WITH NOCHECK ADD  CONSTRAINT [FK_OvertimeRequest_Request] FOREIGN KEY([Request])
REFERENCES [dbo].[Request] ([Id])
GO

ALTER TABLE [dbo].[OvertimeRequest] CHECK CONSTRAINT [FK_OvertimeRequest_Request]
GO


