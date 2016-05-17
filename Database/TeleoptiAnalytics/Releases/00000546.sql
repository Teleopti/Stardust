--#38301,#38303,#38304 make sure no duplicates from event based updates
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PK_AbsenceCode]') AND type in (N'UQ'))
BEGIN
	ALTER TABLE [mart].[dim_absence]
	ADD CONSTRAINT [PK_AbsenceCode] UNIQUE ([absence_code])
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PK_ActivityCode]') AND type in (N'UQ'))
BEGIN
	ALTER TABLE [mart].[dim_activity]
	ADD CONSTRAINT [PK_ActivityCode] UNIQUE ([activity_code])
END
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PK_OvertimeCode]') AND type in (N'UQ'))
BEGIN
	ALTER TABLE [mart].[dim_overtime]
	ADD CONSTRAINT [PK_OvertimeCode] UNIQUE ([overtime_code])
END
