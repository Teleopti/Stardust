-- =============================================
-- Author:		Jonas
-- Create date: 2015-02-10
-- Description:	
--				1. Make sure that Name columns not only contains of whitespace(s)
--				2. Remove unused columns.
-- =============================================
UPDATE RuleSetBag SET Name='_' WHERE LEN(Name)=0
UPDATE Site SET Name='_' WHERE LEN(Name)=0
UPDATE Team SET Name='_' WHERE LEN(Name)=0
UPDATE Contract SET Name='_' WHERE LEN(Name)=0
UPDATE ContractSchedule SET Name='_' WHERE LEN(Name)=0
UPDATE PartTimePercentage SET Name='_' WHERE LEN(Name)=0
UPDATE Activity SET Name='_' WHERE LEN(Name)=0
UPDATE Absence SET Name='_' WHERE LEN(Name)=0
UPDATE Scenario SET Name='_' WHERE LEN(Name)=0
UPDATE ShiftCategory SET Name='_' WHERE LEN(Name)=0
UPDATE DayOffTemplate SET Name='_' WHERE LEN(Name)=0
UPDATE AlarmType SET Name='_' WHERE LEN(Name)=0


IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ShortName' AND [object_id] = OBJECT_ID(N'AvailabilityRotation'))
BEGIN
	ALTER TABLE dbo.AvailabilityRotation
		DROP COLUMN ShortName
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ShortName' AND [object_id] = OBJECT_ID(N'Rotation'))
BEGIN
	ALTER TABLE dbo.Rotation
		DROP COLUMN ShortName
END