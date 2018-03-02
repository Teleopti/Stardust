----------------  
--Name: Bockemon
--Date: 2018-02-28
--Desc: New table for audit trial of person access
----------------------------------------------------  
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auditing].[PersonAccess]') AND type in (N'U'))
   DROP TABLE [Auditing].[PersonAccess]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Auditing].[PersonAccess](
	[Id] [uniqueidentifier] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	[ActionPerformedBy] [uniqueidentifier] NOT NULL,
	[Action] nvarchar(255) NOT NULL,
	[ActionResult] nvarchar(128) NOT NULL,
	[Data] nvarchar(MAX) NOT NULL,
	[ActionPerformedOn] [uniqueidentifier] NOT NULL,
	[Correlation] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_PersonAccess] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Auditing].[PersonAccess] ADD  CONSTRAINT [DF_PersonAcess_id]  DEFAULT (newsequentialid()) FOR [id]
GO

ALTER TABLE [Auditing].[PersonAccess] ADD  CONSTRAINT [DF_PersonAccess_TimeStamp]  DEFAULT (getdate()) FOR [TimeStamp]
GO

ALTER TABLE [Auditing].[PersonAccess] ADD  CONSTRAINT [DF_PersonAccess_Correlation]  DEFAULT (newid()) FOR [Correlation]
GO

ALTER TABLE [Auditing].[PersonAccess]  WITH CHECK ADD  CONSTRAINT [FK_PA_ActionPerformedBy_Person_Id] FOREIGN KEY([ActionPerformedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [Auditing].[PersonAccess] CHECK CONSTRAINT [FK_PA_ActionPerformedBy_Person_Id]
GO

ALTER TABLE [Auditing].[PersonAccess]  WITH CHECK ADD  CONSTRAINT [FK_PA_ActionPerformedOn_Person_Id] FOREIGN KEY([ActionPerformedOn])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [Auditing].[PersonAccess] CHECK CONSTRAINT [FK_PA_ActionPerformedOn_Person_Id]
GO