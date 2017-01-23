CREATE NONCLUSTERED INDEX IX_ShiftLayer_AUD_Parent_REVTYPE
ON [Auditing].[ShiftLayer_AUD] ([Parent],[REVTYPE])


CREATE NONCLUSTERED INDEX IX_PersonAssignment_AUD_Person_Date
ON [Auditing].[PersonAssignment_AUD] ([Person],[Date])
