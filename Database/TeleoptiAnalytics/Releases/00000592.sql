
DROP TABLE [dbo].[RtaTracer]

GO

CREATE TABLE [dbo].[RtaTracer] (
    Id int NOT NULL IDENTITY(1,1),
	[Time] [datetime] NULL,
	[MessageType] [nvarchar] (max) NULL,
    [Message] [nvarchar] (max) NULL,
	CONSTRAINT [PK_RtaTracer] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
