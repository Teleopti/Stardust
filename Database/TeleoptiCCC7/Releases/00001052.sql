DROP TABLE [Auditing].[PersonAccess]
GO

CREATE TABLE [Auditing].[PersonAccess](
            [Id] [uniqueidentifier] NOT NULL,
			[TimeStamp] [datetime] NOT NULL,
			[ActionPerformedById] [uniqueidentifier] NOT NULL,
			[ActionPerformedBy] [nvarchar](300),
			[Action] [nvarchar](255) NOT NULL,
			[ActionResult] [nvarchar](128) NOT NULL,
			[Data] [nvarchar](max) NOT NULL,
			[Correlation] [uniqueidentifier] NOT NULL,
            [ActionPerformedOnId] [uniqueidentifier] NOT NULL,
            [ActionPerformedOn] [nvarchar](300),
			[SearchKeys] [nvarchar](max) NOT NULL,
CONSTRAINT [PK_PersonAccess] PRIMARY KEY CLUSTERED 
(
            [Id] ASC
)
)
GO