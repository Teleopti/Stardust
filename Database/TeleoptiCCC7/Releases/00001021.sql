CREATE TABLE [Auditing].[StaffingAudit](
	[Id] [uniqueidentifier] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	[ActionPerformedBy] [uniqueidentifier] NOT NULL,
	[Action] [nvarchar](255) NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
	[Area] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_StaffingAudit] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY] 
GO

ALTER TABLE [Auditing].[StaffingAudit]  WITH CHECK ADD  CONSTRAINT [FK__PER_SA_ActionPerformedBy_Person_Id] FOREIGN KEY([ActionPerformedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [Auditing].[StaffingAudit] CHECK CONSTRAINT [FK__PER_SA_ActionPerformedBy_Person_Id]
GO


