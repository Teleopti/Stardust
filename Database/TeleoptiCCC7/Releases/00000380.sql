----------------  
--Name: Robin K
--Date: 2012-12-07
--Desc: Adding read model table for schedules.
-----------------  
CREATE TABLE [ReadModel].[PersonScheduleDay](
	[Id] [uniqueidentifier] NOT NULL,
	[PersonId] [uniqueidentifier] NOT NULL,
	[TeamId] [uniqueidentifier] NOT NULL,
	[BelongsToDate] [smalldatetime] NOT NULL,
	[ShiftStart] [datetime] NULL,
	[ShiftEnd] [datetime] NULL,
	[Shift] [nvarchar](4000) NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[BusinessUnitId] [uniqueidentifier] NOT NULL,
	[InsertedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_PersonScheduleDay] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [ReadModel].[PersonScheduleDay] ADD  CONSTRAINT [DF_PersonScheduleDay_InsertedOn]  DEFAULT (getutcdate()) FOR [InsertedOn]
GO

----------------  
--Name: Robin Karlsson
--Date: 2013-02-13
--Desc: Bug #22194. Constraint violation error in PersonSkill
----------------  
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonSkill]') AND name = N'UC_Parent_Skill')
	EXEC('ALTER TABLE [dbo].[PersonSkill] DROP CONSTRAINT [UC_Parent_Skill]')
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (380,'7.3.380') 
