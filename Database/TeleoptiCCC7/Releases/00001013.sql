Create SCHEMA Toggle
GO
Create TABLE [Toggle].[Override](
	[Toggle] [nvarchar](255) NOT NULL,
	[Enabled] [bit] NOT NULL,
	CONSTRAINT [PK_Toggle] PRIMARY KEY CLUSTERED
(
	[Toggle] ASC
)
)ON [PRIMARY]
GO