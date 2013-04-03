/****** Object:  Table [mart].[LastUpdatedPerStep]    Script Date: onsdag 2013 04 03 10:50:56 ******/

CREATE TABLE [mart].[LastUpdatedPerStep](
	[StepName] [varchar](500) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NULL,
	[LastChecksum] [bigint] NOT NULL
) ON [PRIMARY]

GO