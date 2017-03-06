ALTER TABLE dbo.OptionalColumn ADD
	EnableReporting bit NOT NULL CONSTRAINT DF_OptionalColumn_EnableReporting DEFAULT 0
GO
