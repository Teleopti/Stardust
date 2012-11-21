--#21539
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_Team_Id_Parent')
CREATE NONCLUSTERED INDEX IX_PersonPeriod_Team_Id_Parent
ON [dbo].[PersonPeriod] ([Team])
INCLUDE ([Id],[Parent])
GO
