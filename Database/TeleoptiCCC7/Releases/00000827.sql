IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PersonDayOff') AND type in (N'U'))
BEGIN
	DROP TABLE dbo.PersonDayOff
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PersonDayOff_old') AND type in (N'U'))
BEGIN
	DROP TABLE dbo.PersonDayOff_old
END

GO
