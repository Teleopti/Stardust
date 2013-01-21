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
GO

--create temp table
CREATE TABLE [dbo].[temp_StudentAvailabilityRestriction](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NULL,
	[RestrictionIndex] [int] NOT NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL
	)

CREATE CLUSTERED INDEX [CIX_StudentAvailabilityRestriction_parent] ON [dbo].[temp_StudentAvailabilityRestriction] 
(
	[Parent] ASC
)

--get current data
INSERT INTO [dbo].[temp_StudentAvailabilityRestriction]
(Id, Parent, RestrictionIndex, StartTimeMinimum, StartTimeMaximum, EndTimeMinimum, EndTimeMaximum, WorkTimeMinimum, WorkTimeMaximum)
SELECT
Id, Parent, RestrictionIndex, StartTimeMinimum, StartTimeMaximum, EndTimeMinimum, EndTimeMaximum, WorkTimeMinimum, WorkTimeMaximum
FROM [dbo].[StudentAvailabilityRestriction]

--drop old table
DROP TABLE [dbo].[StudentAvailabilityRestriction]

--rename temp_table
EXEC dbo.sp_rename @objname = N'temp_StudentAvailabilityRestriction', @newname = N'StudentAvailabilityRestriction', @objtype = N'OBJECT'

--re-add PK
ALTER TABLE dbo.StudentAvailabilityRestriction ADD CONSTRAINT
	PK_StudentAvailabilityRestriction PRIMARY KEY NONCLUSTERED 
	(
	Id
	)


ALTER TABLE [dbo].[StudentAvailabilityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_StudentAvailabilityDay_AvailabilityRestriction] FOREIGN KEY([Parent])
REFERENCES [dbo].[StudentAvailabilityDay] ([Id])
ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[StudentAvailabilityRestriction] CHECK CONSTRAINT [FK_StudentAvailabilityDay_AvailabilityRestriction]
GO


----------------  
--Name: David Jonsson
--Date: 2013-01-08
--Desc: Bug #21786. Remove dubplicates in PersonSkill
----------------  
--remove duplicates, but keep one.
--note: We don't consider "Active" it will be random
WITH Dubplicates AS
(
	select Id, Parent,Skill,ROW_NUMBER() OVER(PARTITION BY Parent,Skill ORDER BY Id) as rownumber
	from PersonSkill
) 
DELETE ps
FROM PersonSkill ps
INNER JOIN Dubplicates d
ON ps.Id = d.Id
WHERE d.rownumber >1;
GO

--add constraint to block new duplicates
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonSkill]') AND name = N'UC_Parent_Skill')
ALTER TABLE PersonSkill ADD CONSTRAINT UC_Parent_Skill UNIQUE (Parent,Skill)


----------------  
--Name: Jonas N
--Date: 2012-11-14  
--Desc: Add the following new application function ShiftTradeRequestsWeb
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0080' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ShiftTradeRequests' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxShiftTradeRequests' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (378,'7.3.378') 
