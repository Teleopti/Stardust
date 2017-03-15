ALTER TABLE dbo.OptionalColumn ADD
	AvailableAsGroupPage bit NOT NULL CONSTRAINT DF_OptionalColumn_AvailableAsGroupPage DEFAULT 0
GO
