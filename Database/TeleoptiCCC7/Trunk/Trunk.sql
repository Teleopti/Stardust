--Robin: Removing column IsDeleted as it isn't used anymore.

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		DELETE FROM dbo.StateGroupActivityAlarm	WHERE IsDeleted=1
	END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		ALTER TABLE dbo.StateGroupActivityAlarm	DROP COLUMN IsDeleted
	END
	
GO