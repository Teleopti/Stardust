IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auditing].[PersonDayOff_AUD]') AND type in (N'U'))
BEGIN
	DROP TABLE [Auditing].[PersonDayOff_AUD]
END

GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auditing].[PersonDayOff_AUD_old]') AND type in (N'U'))
BEGIN
	DROP TABLE [Auditing].[PersonDayOff_AUD_old]
END

GO
