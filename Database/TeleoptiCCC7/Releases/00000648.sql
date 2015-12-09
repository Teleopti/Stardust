ALTER TABLE dbo.RtaRule ADD
	AlarmColor int NULL
GO
UPDATE [dbo].[RtaRule] SET AlarmColor = DisplayColor
GO
ALTER TABLE [dbo].[RtaRule] ALTER COLUMN AlarmColor INTEGER NOT NULL
GO