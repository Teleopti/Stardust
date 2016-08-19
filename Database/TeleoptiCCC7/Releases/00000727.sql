IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsLogOutState' AND Object_ID = Object_ID(N'dbo.RtaStateGroup'))
BEGIN
	ALTER TABLE dbo.RtaStateGroup ADD IsLogOutState BIT
	UPDATE dbo.RtaStateGroup SET IsLogOutState = 0
	ALTER TABLE dbo.RtaStateGroup ALTER COLUMN IsLogOutState BIT NOT NULL
END
