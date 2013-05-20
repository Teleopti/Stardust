
----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE dbo.PersonAssignment
ADD TheDate datetime
GO

update dbo.PersonAssignment
set TheDate ='1800-1-1'
GO

ALTER TABLE dbo.PersonAssignment
ALTER COLUMN TheDate datetime not null
GO

----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment audit table. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE Auditing.PersonAssignment_AUD
ADD TheDate datetime
GO

update Auditing.PersonAssignment_AUD
set TheDate ='1800-1-1'
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