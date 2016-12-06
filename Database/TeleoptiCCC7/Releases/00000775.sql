
CREATE TABLE [dbo].[JobStartTime](
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[StartTime] [datetime] NOT NULL
 CONSTRAINT [PK_JobStartTime] PRIMARY KEY CLUSTERED 
(
	[BusinessUnit] ASC
))

GO