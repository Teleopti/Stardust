IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Auditing].[PersonAssignment_AUD]') AND name = N'IX_PersonAssignment_AUD_REV')
	CREATE NONCLUSTERED INDEX IX_PersonAssignment_AUD_REV
	ON [Auditing].[PersonAssignment_AUD] ([REV])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Auditing].[PersonAbsence_AUD]') AND name = N'IX_PersonAbsence_AUD_REV')
	CREATE NONCLUSTERED INDEX IX_PersonAbsence_AUD_REV
	ON [Auditing].[PersonAbsence_AUD] ([REV])

