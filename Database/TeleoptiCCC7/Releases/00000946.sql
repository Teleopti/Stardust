CREATE TABLE [rta].[Events] (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Event] nvarchar(max) NULL,
	CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
)
