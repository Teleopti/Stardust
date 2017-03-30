ALTER TABLE [ReadModel].[CurrentSchedule] ADD LastUpdate int
GO

IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_CurrentSchedule_LastUpdate')
CREATE NONCLUSTERED INDEX [IX_CurrentSchedule_LastUpdate] ON [ReadModel].[CurrentSchedule]
(
 [LastUpdate] ASC
)
GO
