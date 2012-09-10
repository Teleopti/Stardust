--Robin: Removing column IsDeleted as it isn't used anymore.

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		EXECUTE('DELETE FROM dbo.StateGroupActivityAlarm WHERE IsDeleted=1')
	END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		ALTER TABLE dbo.StateGroupActivityAlarm	DROP COLUMN IsDeleted
	END
	
GO

--Anders: Adding possiblity to purge old messages cause it's been asked for
if not exists (select 1 from PurgeSetting where [Key] = 'Message')
begin
	insert into PurgeSetting values ('Message', 10)
end

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