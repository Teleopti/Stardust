
----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE dbo.PersonAssignment
ADD [Date] datetime
GO

declare @Date datetime
set @Date = '1800-01-01T00:00:00'
update dbo.PersonAssignment
set [Date] = @Date
GO

ALTER TABLE dbo.PersonAssignment
ALTER COLUMN [Date] datetime not null
GO

----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment audit table. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE Auditing.PersonAssignment_AUD
ADD [Date] datetime
GO

declare @Date datetime
set @Date = '1800-01-01T00:00:00'

update Auditing.PersonAssignment_AUD
set [Date] = @Date
GO

----------------  
--Name: Roger Kratz
--Date: 2013-05-15

---------------- MAIN TABLES --------------------------

--Add shiftcategory to personassignment
ALTER TABLE dbo.PersonAssignment
add ShiftCategory uniqueidentifier

ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO

ALTER TABLE [dbo].[PersonAssignment] CHECK CONSTRAINT [FK_PersonAssignment_ShiftCategory]
GO

--move data for ShiftCategory
UPDATE pa
SET pa.ShiftCategory = ms.ShiftCategory
FROM dbo.PersonAssignment pa
INNER JOIN dbo.MainShift ms
	ON pa.Id = ms.Id

--add mainshiftlayers from personassignment
ALTER TABLE [dbo].[MainShiftActivityLayer] DROP CONSTRAINT [FK_MainShiftActivityLayer_MainShift]
GO

ALTER TABLE [dbo].[MainShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayer_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
on delete cascade
GO

ALTER TABLE [dbo].[MainShiftActivityLayer] CHECK CONSTRAINT [FK_MainShiftActivityLayer_PersonAssignment]
GO

--drop mainshift table
DROP TABLE dbo.MainShift
GO

------------------ AUDIT TABLES ----------------------------
--Add shiftcategory to personassignment
ALTER TABLE auditing.PersonAssignment_AUD
add ShiftCategory uniqueidentifier
GO

--move data for ShiftCategory
UPDATE pa
SET pa.ShiftCategory = ms.ShiftCategory
FROM auditing.PersonAssignment_AUD pa
INNER JOIN auditing.MainShift_AUD ms
	ON pa.Id = ms.Id

--drop mainshift table
drop table auditing.mainshift_aud
GO

----------------  
--Name: Kunning Mao
--Date: 2013-05-20
--Desc: Empty read models in order to re-fill it with compressed shifts
----------------  
TRUNCATE TABLE [ReadModel].[PersonScheduleDay]
GO

ALTER TABLE [ReadModel].[PersonScheduleDay] ALTER COLUMN [Shift] nvarchar(2000)
GO


--Name: Erik Sundberg
--Date: 2013-05-20
--Desc: Bug #23519 Duplicate OrderIndex in ActivityExtender
----------------  

SET NOCOUNT ON
-------------------
--Create table to hold error layers
-------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityExtenderWrongOrderIndex]') AND type in (N'U'))
BEGIN 
	CREATE TABLE dbo.ActivityExtenderWrongOrderIndex
	(
		Batch int NOT NULL,
		UpdatedOn smalldatetime NOT NULL,
		Id uniqueidentifier NOT NULL,
		Parent uniqueidentifier NOT NULL,
		OrderIndexOld int NOT NULL,
		OrderIndexNew int NOT NULL
	)

	ALTER TABLE dbo.ActivityExtenderWrongOrderIndex 
	ADD CONSTRAINT PK_ActivityExtenderWrongOrderIndex PRIMARY KEY (Batch, Id)
END

-------------------
--Save error layers
-------------------
DECLARE @Batch int
DECLARE @UpdatedOn smalldatetime
SELECT @UpdatedOn = GETDATE()
SELECT @Batch = ISNULL(MAX(Batch),0) FROM ActivityExtenderWrongOrderIndex
SET @Batch = @Batch + 1

INSERT INTO dbo.ActivityExtenderWrongOrderIndex
SELECT
		@Batch,
		@UpdatedOn,
		ps1.Id,
		ps1.Parent,
		ps1.OrderIndex as OrderIndexOld,
		ActivityExtenderIndexChange.rn-1 as OrderindexNew
	FROM ActivityExtender ps1
	INNER JOIN
	(
		SELECT ps2.id, ps2.parent, ps2.orderindex, ROW_NUMBER()OVER(PARTITION BY ps2.parent ORDER BY ps2.orderindex) rn
		FROM ActivityExtender ps2
	) ActivityExtenderIndexChange
	ON ps1.id=ActivityExtenderIndexChange.id
	WHERE ActivityExtenderIndexChange.OrderIndex <> ActivityExtenderIndexChange.rn-1

-------------------
--Update error layers
-------------------
UPDATE ActivityExtender
SET 
	OrderIndex	= fix.OrderindexNew
FROM dbo.ActivityExtenderWrongOrderIndex fix
WHERE fix.Id = ActivityExtender.Id AND fix.Parent = ActivityExtender.Parent
AND fix.Batch = @Batch

-------------------
--Report error layers
-------------------
IF (SELECT COUNT(1) FROM dbo.ActivityExtenderWrongOrderIndex WHERE Batch=@Batch)> 0
PRINT 'Shifts have been updated'

SET NOCOUNT OFF
GO

----------------  
--Name: David
--Date: 2013-05-31
--Desc: #23675 - Force ETL permission to run once
-----------------
DECLARE @isoDate datetime
SET @isoDate = '2001-01-01T00:00:00.000'

INSERT INTO [mart].[LastUpdatedPerStep]
SELECT
	[StepName] =  'Permissions',
	[BusinessUnit] = bu.Id,
	[Date] = @isoDate
FROM  dbo.BusinessUnit bu
WHERE NOT EXISTS (
			SELECT *
			FROM [mart].[LastUpdatedPerStep] a
			WHERE bu.Id = a.BusinessUnit
			)
GO