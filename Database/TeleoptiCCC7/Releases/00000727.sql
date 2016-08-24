IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsLogOutState' AND Object_ID = Object_ID(N'dbo.RtaStateGroup'))
	ALTER TABLE dbo.RtaStateGroup ADD IsLogOutState BIT
GO

UPDATE dbo.RtaStateGroup SET IsLogOutState = 0 WHERE IsLogOutState IS NULL
GO

ALTER TABLE dbo.RtaStateGroup ALTER COLUMN IsLogOutState BIT NOT NULL
GO

