----------------  
--Name: David J
--Date: 2011-11-02
--Desc: #16772 - Add PK to OpenHourList
----------------  
SET NOCOUNT ON

--First we need to handle potential duplicates
CREATE TABLE #removeUs (
	[Parent] [uniqueidentifier] NOT NULL,
	[Minimum] [bigint] NOT NULL,
	[Maximum] [bigint] NOT NULL
)

--Add clustered key. needed for Azure
CREATE CLUSTERED INDEX [PK_removeUs] ON #removeUs
(
	[Parent] ASC
)

INSERT INTO #removeUs
SELECT Parent,Minimum,Maximum
FROM dbo.OpenHourList
GROUP BY Parent,Minimum,Maximum
HAVING count(Parent) > 1

--Cursor to remove duplicates from dbo.OpenHourList
declare @parent uniqueidentifier
declare @Minimum bigint
declare @Maximum bigint
 
declare OpenHourList cursor for 
SELECT Parent,Minimum,Maximum from #removeUs
OPEN OpenHourList 
FETCH NEXT FROM OpenHourList INTO @parent,@Minimum,@Maximum
WHILE @@FETCH_STATUS = 0
BEGIN
	if (select COUNT(*) from dbo.OpenHourList
		where	Parent	=@parent
		and		Minimum	=@Minimum
		and		Maximum	=@Maximum
		) > 1
	begin
		delete top(1) from dbo.OpenHourList
		where	Parent	=@parent
		and		Minimum	=@Minimum
		and		Maximum	=@Maximum
	end
	FETCH NEXT FROM OpenHourList INTO @parent,@Minimum,@Maximum
END
CLOSE OpenHourList
DEALLOCATE OpenHourList

--Add PK
ALTER TABLE dbo.OpenHourList ADD CONSTRAINT
PK_OpenHourList PRIMARY KEY NONCLUSTERED 
(
Parent,
Minimum,
Maximum
)

DROP TABLE #removeUs
SET NOCOUNT OFF
GO

----------------  
--Name: David J
--Date: 2011-11-02
--Desc: #16613 - [Auditing].[Revision] change Int => BigInt
----------------  
--*************************
--Drop PK and FK on child table
--*************************
CREATE TABLE [Auditing].[temp_Revision](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ModifiedAt] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NOT NULL
)

CREATE TABLE [Auditing].[temp_MainShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[RefId] [uniqueidentifier] NULL,
	[ShiftCategory] [uniqueidentifier] NULL
	)

CREATE TABLE [Auditing].[temp_MainShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL
)

CREATE TABLE [Auditing].[temp_OvertimeShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[OrderIndex] [int] NULL,
	[Parent] [uniqueidentifier] NULL
)

CREATE TABLE [Auditing].[temp_OvertimeShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[DefinitionSet] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL
	)

CREATE TABLE [Auditing].[temp_PersonAbsence_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Version] [int] NULL,
	[LastChange] [datetime] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[Person] [uniqueidentifier] NULL,
	[Scenario] [uniqueidentifier] NULL,
	[Payload] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NULL
	)

CREATE TABLE [Auditing].[temp_PersonalShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[OrderIndex] [int] NULL,
	[Parent] [uniqueidentifier] NULL
 )

CREATE TABLE [Auditing].[temp_PersonalShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL
	)
	
CREATE TABLE [Auditing].[temp_PersonAssignment_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Version] [int] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[Person] [uniqueidentifier] NULL,
	[Scenario] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NULL
)

CREATE TABLE [Auditing].[temp_PersonDayOff_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [bigint] NOT NULL,
	[REVTYPE] [tinyint] NOT NULL,
	[Version] [int] NULL,
	[Anchor] [datetime] NULL,
	[TargetLength] [bigint] NULL,
	[Flexibility] [bigint] NULL,
	[DisplayColor] [int] NULL,
	[PayrollCode] [nvarchar](20) NULL,
	[Name] [nvarchar](50) NULL,
	[ShortName] [nvarchar](25) NULL,
	[Person] [uniqueidentifier] NULL,
	[Scenario] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NULL
)

ALTER TABLE [Auditing].[temp_Revision] ADD  CONSTRAINT [temp_PK_Revision] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)

ALTER TABLE [Auditing].[temp_MainShift_AUD] ADD  CONSTRAINT [temp_PK_MainShift_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)

ALTER TABLE [Auditing].[temp_MainShiftActivityLayer_AUD] ADD  CONSTRAINT [temp_PK_MainShiftActivityLayer_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
ALTER TABLE [Auditing].[temp_OvertimeShift_AUD] ADD  CONSTRAINT [temp_PK_OvertimeShift_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
ALTER TABLE [Auditing].[temp_OvertimeShiftActivityLayer_AUD] ADD  CONSTRAINT [temp_PK_OvertimeShiftActivityLayer_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
ALTER TABLE [Auditing].[temp_PersonAbsence_AUD] ADD  CONSTRAINT [temp_PK_personAbsence_aud] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
ALTER TABLE [Auditing].[temp_PersonalShift_AUD] ADD  CONSTRAINT [temp_PK_personalshift_aud] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
ALTER TABLE [Auditing].[temp_PersonalShiftActivityLayer_AUD] ADD  CONSTRAINT [temp_PK_personalshiftactivitylayer_aud] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
ALTER TABLE [Auditing].[temp_PersonAssignment_AUD] ADD  CONSTRAINT [temp_PK_personassignment_aud] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
ALTER TABLE [Auditing].[temp_PersonDayOff_AUD] ADD  CONSTRAINT [temp_PK_persondayoff_aud] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)

SET IDENTITY_INSERT [Auditing].[temp_Revision] ON
INSERT INTO [Auditing].[temp_Revision](Id, ModifiedAt, ModifiedBy)
SELECT Id, ModifiedAt, ModifiedBy FROM [Auditing].[Revision]
SET IDENTITY_INSERT [Auditing].[temp_Revision] OFF

INSERT INTO [Auditing].[temp_MainShift_AUD]	SELECT * FROM [Auditing].[MainShift_AUD]
INSERT INTO [Auditing].[temp_MainShiftActivityLayer_AUD] SELECT * FROM [Auditing].[MainShiftActivityLayer_AUD]
INSERT INTO [Auditing].[temp_OvertimeShift_AUD] SELECT * FROM [Auditing].[OvertimeShift_AUD]
INSERT INTO [Auditing].[temp_OvertimeShiftActivityLayer_AUD] SELECT * FROM [Auditing].[OvertimeShiftActivityLayer_AUD] 
INSERT INTO [Auditing].[temp_PersonAbsence_AUD] SELECT * FROM [Auditing].[PersonAbsence_AUD] 
INSERT INTO [Auditing].[temp_PersonalShift_AUD] SELECT * FROM [Auditing].[PersonalShift_AUD] 
INSERT INTO [Auditing].[temp_PersonDayOff_AUD] SELECT * FROM [Auditing].[PersonDayOff_AUD] 
INSERT INTO [Auditing].[temp_PersonAssignment_AUD] SELECT * FROM [Auditing].[PersonAssignment_AUD]
INSERT INTO [Auditing].[temp_PersonalShiftActivityLayer_AUD] SELECT * FROM [Auditing].[PersonalShiftActivityLayer_AUD] 

DROP TABLE [Auditing].[MainShift_AUD]
DROP TABLE [Auditing].[MainShiftActivityLayer_AUD] 
DROP TABLE [Auditing].[OvertimeShift_AUD]
DROP TABLE [Auditing].[OvertimeShiftActivityLayer_AUD]
DROP TABLE [Auditing].[PersonAbsence_AUD]
DROP TABLE [Auditing].[PersonalShift_AUD]
DROP TABLE [Auditing].[PersonDayOff_AUD]
DROP TABLE [Auditing].[PersonAssignment_AUD]
DROP TABLE [Auditing].[PersonalShiftActivityLayer_AUD]
DROP TABLE [Auditing].[Revision]

EXEC dbo.sp_rename @objname = N'[Auditing].[temp_Revision]', @newname = N'Revision', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_MainShift_AUD]', @newname = N'MainShift_AUD', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_MainShiftActivityLayer_AUD]', @newname = N'MainShiftActivityLayer_AUD', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_OvertimeShift_AUD]', @newname = N'OvertimeShift_AUD', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_OvertimeShiftActivityLayer_AUD]', @newname = N'OvertimeShiftActivityLayer_AUD', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_PersonAbsence_AUD]', @newname = N'PersonAbsence_AUD', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_PersonalShift_AUD]', @newname = N'PersonalShift_AUD', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_PersonDayOff_AUD]', @newname = N'PersonDayOff_AUD', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_PersonAssignment_AUD]', @newname = N'PersonAssignment_AUD', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[Auditing].[temp_PersonalShiftActivityLayer_AUD]', @newname = N'PersonalShiftActivityLayer_AUD', @objtype = N'OBJECT'

EXEC dbo.sp_rename @objname = N'[Auditing].[MainShift_AUD].[temp_PK_MainShift_AUD]', @newname = N'PK_MainShift_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[MainShiftActivityLayer_AUD].[temp_PK_MainShiftActivityLayer_AUD]', @newname = N'PK_MainShiftActivityLayer_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[OvertimeShift_AUD].[temp_PK_OvertimeShift_AUD]', @newname = N'PK_OvertimeShift_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[OvertimeShiftActivityLayer_AUD].[temp_PK_OvertimeShiftActivityLayer_AUD]', @newname = N'PK_OvertimeShiftActivityLayer_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[PersonAbsence_AUD].[temp_PK_PersonAbsence_AUD]', @newname = N'PK_PersonAbsence_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[PersonalShift_AUD].[temp_PK_PersonalShift_AUD]', @newname = N'PK_PersonalShift_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[PersonalShiftActivityLayer_AUD].[temp_PK_PersonalShiftActivityLayer_AUD]', @newname = N'PK_PersonalShiftActivityLayer_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[PersonAssignment_AUD].[temp_PK_PersonAssignment_AUD]', @newname = N'PK_PersonAssignment_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[PersonDayOff_AUD].[temp_PK_PersonDayOff_AUD]', @newname = N'PK_PersonDayOff_AUD', @objtype =N'INDEX'
EXEC dbo.sp_rename @objname = N'[Auditing].[Revision].[temp_PK_Revision]', @newname = N'PK_Revision', @objtype =N'INDEX'


--*************************
--Re-Add FK on child table
--*************************
ALTER TABLE [Auditing].[Revision]  WITH CHECK ADD  CONSTRAINT [FK_Revision_Person] FOREIGN KEY([ModifiedBy])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [Auditing].[Revision] CHECK CONSTRAINT [FK_Revision_Person]

ALTER TABLE [Auditing].[MainShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShift_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[MainShift_AUD] CHECK CONSTRAINT [FK_MainShift_REV]

ALTER TABLE [Auditing].[OvertimeShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShift_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[OvertimeShift_AUD] CHECK CONSTRAINT [FK_OvertimeShift_REV]

ALTER TABLE [Auditing].[OvertimeShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[OvertimeShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_REV]

ALTER TABLE [Auditing].[PersonAbsence_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalAbsence_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[PersonAbsence_AUD] CHECK CONSTRAINT [FK_PersonalAbsence_REV]

ALTER TABLE [Auditing].[PersonalShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShift_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[PersonalShift_AUD] CHECK CONSTRAINT [FK_PersonalShift_REV]

ALTER TABLE [Auditing].[PersonalShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayer_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[PersonalShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_PersonalShiftActivityLayer_REV]

ALTER TABLE [Auditing].[PersonAssignment_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[PersonAssignment_AUD] CHECK CONSTRAINT [FK_PersonAssignment_REV]

ALTER TABLE [Auditing].[PersonDayOff_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[PersonDayOff_AUD] CHECK CONSTRAINT [FK_PersonDayOff_REV]

ALTER TABLE [Auditing].[MainShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayer_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[MainShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_MainShiftActivityLayer_REV]

GO

----------------  
--Name: David J
--Date: 2011-11-03
--Desc: Fix name on unique constraint
----------------  
--Make sure the constraint name is correct
declare @indexname nvarchar(2000)
select @indexname = i.name
from sys.indexes i
inner join sys.objects o
           on i.object_id = o.object_id
           and object_name(o.object_id)= 'QueueSourceCollection' --Table name
inner join sys.schemas s
           on s.schema_id = o.schema_id
           and s.name = 'dbo' --schema name
where i.is_unique = 1 --Index type: UNIQUE
 

exec sp_rename @objname = @indexname, @newname = 'UQ_QueueSourceCollection', @objtype = 'OBJECT'
GO

---------------
--Name: Xianwei Shen
--Date: 2011-10-26
--Desc: Add AbsenceThreshold, AbsenceExtra and AbsenceOverride
---------------
ALTER TABLE dbo.BudgetDay ADD
	AbsenceThreshold float(53) NOT NULL CONSTRAINT DF_BudgetDay_Threshold DEFAULT 1,
	AbsenceExtra float(53) NULL,
	AbsenceOverride float(53) NULL
GO
---------------
--Name: Xianwei Shen
--Date: 2011-10-31
--Desc: Add IncludedInAllowance to CustomShrinkage and CustomEfficiencyShirinkage
---------------
ALTER TABLE dbo.CustomShrinkage ADD
	IncludedInAllowance bit NOT NULL CONSTRAINT DF_CustomShrinkage_IncludedInAllowance DEFAULT 0
GO

ALTER TABLE dbo.CustomEfficiencyShrinkage ADD
	IncludedInAllowance bit NOT NULL CONSTRAINT DF_CustomEfficiencyShrinkage_IncludedInAllowance DEFAULT 0
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (339,'7.1.339') 
