
CREATE SCHEMA [rta] AUTHORIZATION [dbo]
GO

CREATE TABLE [rta].[ApprovedPeriods] (
	[PersonId] [uniqueidentifier] NOT NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CIX_PersonId] ON [rta].[ApprovedPeriods]
(	
	[PersonId] ASC
) ON [PRIMARY]
GO
