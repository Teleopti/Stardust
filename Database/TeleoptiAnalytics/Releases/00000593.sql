
DROP TABLE [dbo].[RtaTracer]
GO

CREATE SCHEMA [RtaTracer]
GO

CREATE TABLE [RtaTracer].[Logs] (
    Id int NOT NULL IDENTITY(1,1),
	[Time] [datetime] NULL,
	[Tenant] [nvarchar] (max) NULL,
	[MessageType] [nvarchar] (max) NULL,
    [Message] [nvarchar] (max) NULL,
	CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)

CREATE TABLE [RtaTracer].[Tracer] (
	[Tenant] [nvarchar] (255) NOT NULL,
	[UserCode] [nvarchar] (max) NULL,
	CONSTRAINT [PK_Tracer] PRIMARY KEY CLUSTERED 
	(
		[Tenant] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



