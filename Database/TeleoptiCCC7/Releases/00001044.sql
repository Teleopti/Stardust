DROP TABLE [Auditing].[StaffingAudit]
GO

CREATE TABLE [Auditing].[StaffingAudit](
            [Id] [uniqueidentifier] NOT NULL,
            [TimeStamp] [datetime] NOT NULL,
            [ActionPerformedById] [uniqueidentifier] NOT NULL,
            [ActionPerformedBy] [nvarchar](300),
            [Action] [nvarchar](255) NOT NULL,
            [Area] [nvarchar](255) NOT NULL,
            [ImportFileName] [nvarchar](500),
            [BpoName] [nvarchar](100),
            [ClearPeriodStart] [datetime],
            [ClearPeriodEnd] [datetime],
CONSTRAINT [PK_StaffingAudit] PRIMARY KEY CLUSTERED 
(
            [Id] ASC
)
)
GO

