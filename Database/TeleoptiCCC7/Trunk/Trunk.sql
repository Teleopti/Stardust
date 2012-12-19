-----------------  
--Name: TamasB
--Date: 2012-12-17
--Desc: #bugfix 21764 - Fix invalid text
----------------  
UPDATE [dbo].PersonRequest
SET DenyReason = 'RequestDenyReasonSupervisor'
WHERE DenyReason = 'xxRequestDenyReasonSupervisor'
GO

----------------  
--Name: Robin K
--Date: 2012-12-07
--Desc: Adding read model table for schedules. Not the final model...
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

-----------------  
--Name: Jonas
--Date: 2012-12-19
--Desc: #bugfix 21822 - Changing names for web start menu items
----------------  
update ApplicationFunction
set FunctionDescription = 'xxAnywhere'
where ForeignId = '0080'

update ApplicationFunction
set FunctionDescription = 'xxMobileReports', FunctionCode = 'MobileReports'
where ForeignId = '0074'

----------------  
--Name: David + Kunning
--Date: 2012-12-13
--Desc: Reduce deadlock, by Cascade delete and moved clustered index to parent
----------------  
ALTER TABLE [dbo].[StudentAvailabilityRestriction] DROP CONSTRAINT [PK_StudentAvailabilityRestriction]
GO

ALTER TABLE dbo.StudentAvailabilityRestriction ADD CONSTRAINT
	PK_StudentAvailabilityRestriction PRIMARY KEY NONCLUSTERED 
	(
	Id
	)

CREATE CLUSTERED INDEX [CIX_StudentAvailabilityRestriction_parent] ON [dbo].[StudentAvailabilityRestriction] 
(
	[Parent] ASC
)

ALTER TABLE [dbo].[StudentAvailabilityRestriction] DROP CONSTRAINT [FK_StudentAvailabilityDay_AvailabilityRestriction]
GO

ALTER TABLE [dbo].[StudentAvailabilityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_StudentAvailabilityDay_AvailabilityRestriction] FOREIGN KEY([Parent])
REFERENCES [dbo].[StudentAvailabilityDay] ([Id])
ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[StudentAvailabilityRestriction] CHECK CONSTRAINT [FK_StudentAvailabilityDay_AvailabilityRestriction]
GO


