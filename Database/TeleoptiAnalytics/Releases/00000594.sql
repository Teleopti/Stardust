
DROP TABLE [RtaTracer].[Logs]
GO


CREATE TABLE [RtaTracer].[Logs] (
    Id int NOT NULL IDENTITY(1,1),
	[Time] [datetime] NULL,
	[Tenant] [nvarchar] (255) NULL,
	[MessageType] [nvarchar] (500) NULL,
    [Message] [nvarchar] (max) NULL,
	CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)

CREATE INDEX IDX_Tenant
ON [RtaTracer].[Logs] (Tenant);

CREATE INDEX IDX_MessageType
ON [RtaTracer].[Logs] (MessageType);
