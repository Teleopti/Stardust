-----------------  
--Name: TamasB
--Date: 2012-12-17
--Desc: #bugfix 21764 - Fix invalid text
----------------  
UPDATE [dbo].PersonRequest
SET DenyReason = 'RequestDenyReasonSupervisor'
WHERE DenyReason = 'xxRequestDenyReasonSupervisor'
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

DECLARE @PKName nvarchar(1000)
SELECT @PKName= '[dbo].[StudentAvailabilityRestriction].['+name+']' FROM sys.indexes
WHERE OBJECT_NAME(object_id) = 'StudentAvailabilityRestriction'
AND index_id = 1
AND is_primary_key = 1
SELECT @PKName
EXEC sp_rename @PKName, N'PK_StudentAvailabilityRestriction', N'INDEX'

--Alter Indexes
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


