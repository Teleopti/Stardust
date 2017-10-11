
DROP TABLE [dbo].[RtaTracer]

GO

CREATE TABLE [dbo].[RtaTracer] (
    [Time] [datetime] NULL,
	[MessageType] [nvarchar] (4000) NULL,
    [Message] [nvarchar] (max) NULL
)
