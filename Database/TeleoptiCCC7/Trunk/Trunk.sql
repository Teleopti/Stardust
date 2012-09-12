
----------------  
--Name: Xianwei Shen
--Date: 2012-08-21
--Desc: Add options for whether contract time should come from contract of schedule period
----------------  	
ALTER TABLE dbo.Contract ADD
	IsWorkTimeFromContract int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromContract DEFAULT 1,
	IsWorkTimeFromSchedulePeriod int NOT NULL CONSTRAINT DF_Contract_IsWorkTimeFromSchedulePeriod DEFAULT 0
GO

----------------  
--Name: Asad MIrza
--Date: 2012-08-29
--Desc: Added an extra column in activity to indicate if we can overwrite it or not
---------------- 
ALTER TABLE dbo.Activity ADD
	AllowOverwrite bit NOT NULL CONSTRAINT DF_Activity_AllowOverwrite DEFAULT 1
GO

update  dbo.activity SET AllowOverwrite = 0 where InWorkTime  = 0


GO
----------------  
--Name: David Jonsson
--Date: 2012-09-10 Againg. Still no clue _why_ this is happening.
--Desc: Bug #20008
--Dubplicates in OrderIndex exists for a shift
-- This time it's possible to add UNIQUE INDEX (nhib bug last time)
----------------
--Date: 2010-12-03
--Desc: Bug #12662 + #12663
--Dubplicates in OrderIndex exists for a shift
--		Note: this script is delivered on 306 as a "function" with NO MERGE
--			  It should and can be executed multiple times
--			  It's also added on root in the same manner
----------------  

SET NOCOUNT ON
-------------------
--Create table to hold error layers
-------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MainShiftActivityLayerFix306]') AND type in (N'U'))
BEGIN 
	CREATE TABLE dbo.MainShiftActivityLayerFix306
	(
		Batch int NOT NULL,
		UpdatedOn smalldatetime NOT NULL,
		Id uniqueidentifier NOT NULL,
		Parent uniqueidentifier NOT NULL,
		OrderIndexOld int NOT NULL,
		OrderIndexNew int NOT NULL
	)

	ALTER TABLE dbo.MainShiftActivityLayerFix306 
	ADD CONSTRAINT PK_MainShiftActivityLayerFix306 PRIMARY KEY (Batch, Id)
END

-------------------
--Save error layers
-------------------
DECLARE @Batch int
DECLARE @UpdatedOn smalldatetime
SELECT @UpdatedOn = GETDATE()
SELECT @Batch = ISNULL(MAX(Batch),0) FROM MainShiftActivityLayerFix306
SET @Batch = @Batch + 1

INSERT INTO dbo.MainShiftActivityLayerFix306
SELECT
		@Batch,
		@UpdatedOn,
		ps1.Id,
		ps1.Parent,
		ps1.OrderIndex as OrderIndexOld,
		MainShiftActivityLayerCheck.rn-1 as OrderindexNew
	FROM MainShiftActivityLayer ps1
	INNER JOIN
	(
		SELECT ps2.id, ps2.parent, ps2.orderindex, ROW_NUMBER()OVER(PARTITION BY ps2.parent ORDER BY ps2.orderindex) rn
		FROM MainShiftActivityLayer ps2
	) MainShiftActivityLayerCheck
	ON ps1.id=MainShiftActivityLayerCheck.id
	WHERE MainShiftActivityLayerCheck.OrderIndex <> MainShiftActivityLayerCheck.rn-1

-------------------
--Update error layers
-------------------
UPDATE MainShiftActivityLayer
SET 
	OrderIndex	= fix.OrderindexNew
FROM dbo.MainShiftActivityLayerFix306 fix
WHERE fix.Id = MainShiftActivityLayer.Id AND fix.Parent = MainShiftActivityLayer.Parent
AND fix.Batch = @Batch

-------------------
--Report error layers
-------------------
IF (SELECT COUNT(1) FROM dbo.MainShiftActivityLayerFix306 WHERE Batch=@Batch)> 0
PRINT 'Shifts have been updated'
GO
-------------------
--Add UNIQUE INDEX to prevetn this from happing again
-------------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MainShiftActivityLayer]') AND name = N'UIX_MainShiftActivityLayer_Parent_OrderIndex')
CREATE UNIQUE NONCLUSTERED INDEX [UIX_MainShiftActivityLayer_Parent_OrderIndex] ON [dbo].[MainShiftActivityLayer] 
(
	[Parent] ASC,
	[OrderIndex] ASC
)
GO
----------------  
--Name: Ola
--Date: 2012-08-31
--Desc: Add new ReadModel
---------------- 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleDay]') AND type in (N'U'))
BEGIN
CREATE TABLE [ReadModel].[ScheduleDay](
		[Id] [uniqueidentifier] NOT NULL,
		[PersonId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[StartDateTime] [datetime] NOT NULL,
		[EndDateTime] [datetime] NOT NULL,
		[Workday] [bit] NOT NULL,
		[WorkTime] [bigint] NOT NULL,
		[ContractTime] [bigint] NOT NULL,
		[Label] [nvarchar](50) NOT NULL,
		[DisplayColor] [int] NOT NULL,
		[InsertedOn] [datetime] NOT NULL
	)
	
	
	ALTER TABLE ReadModel.[ScheduleDay] ADD CONSTRAINT
		PK_ScheduleDayReadOnly PRIMARY KEY NONCLUSTERED 
		(
		Id
		)

	CREATE CLUSTERED INDEX [CIX_ScheduleDayReadOnly] ON [ReadModel].[ScheduleDay] 
	(
		[PersonId] ASC,
		[BelongsToDate] ASC	
	)

	ALTER TABLE [ReadModel].[ScheduleDay] ADD  CONSTRAINT [DF_ScheduleDayReadOnly_Id]  DEFAULT (newid()) FOR [Id]
END
GO

----------------  
--Name: Kunning Mao
--Date: 2012-09-11  
--Desc: Add the following new application function> ExtendedPreferencesWeb
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
SELECT @ForeignId = '0078' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ExtendedPreferencesWeb' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxExtendedPreferencesWeb' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO
