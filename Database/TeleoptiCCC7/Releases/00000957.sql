----------------  
-- Name: Bockemon
-- Date: 2018-03-06
-- Desc: New table for tenant audit trial. Removing unnecessary default constraints from PersonAccess table. 
----------------------------------------------------  
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Tenant].[Audit]') AND type in (N'U'))
   DROP TABLE [Tenant].[Audit]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Tenant].[Audit](
	[Id] [uniqueidentifier] NOT NULL, 					
	[TimeStamp] [datetime] NOT NULL,
	[ActionPerformedBy] [uniqueidentifier] NOT NULL, 	
	[Action] nvarchar(255) NOT NULL,
	[ActionResult] nvarchar(128) NOT NULL,
	[Data] nvarchar(MAX) NOT NULL,
	[ActionPerformedOn] [uniqueidentifier] NOT NULL,	
	[Correlation] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_Audit] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

--
-- Cleanup of unsued default constraints on audit table for PersonAccess
--
ALTER TABLE [Auditing].[PersonAccess] DROP  CONSTRAINT [DF_PersonAcess_id]
GO

ALTER TABLE [Auditing].[PersonAccess] DROP  CONSTRAINT [DF_PersonAccess_TimeStamp]
GO

ALTER TABLE [Auditing].[PersonAccess] DROP CONSTRAINT [DF_PersonAccess_Correlation]
GO