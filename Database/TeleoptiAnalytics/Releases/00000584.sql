CREATE TABLE [dbo].[hangfire_requeue](
	[id] [uniqueidentifier] NOT NULL,
	[event_name] [nvarchar](200) NULL,
	[handler_name] [nvarchar](200) NULL,
	[handled] [bit] NOT NULL,
	[timestamp] [datetime] NULL,
 CONSTRAINT [PK_hangfire_requeue] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]