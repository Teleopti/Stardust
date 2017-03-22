IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_CurrentSchedule_UpdatedAt')
CREATE NONCLUSTERED INDEX [IX_CurrentSchedule_UpdatedAt] ON [ReadModel].[CurrentSchedule]
(
 [UpdatedAt] ASC
)
GO