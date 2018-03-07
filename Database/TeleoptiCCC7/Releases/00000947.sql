DROP TABLE [rta].[Events]
GO

CREATE TABLE [rta].[Events] (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [uniqueidentifier] NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[Type] nvarchar(max) NULL,
	[Event] nvarchar(max) NULL,
	CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
)
GO
