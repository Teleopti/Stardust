ALTER TABLE ReadModel.CurrentSchedule ADD Revision int
GO
UPDATE ReadModel.CurrentSchedule SET Revision = 0
GO
DROP INDEX IX_CurrentSchedule_LastUpdate ON ReadModel.CurrentSchedule
GO
ALTER TABLE ReadModel.CurrentSchedule DROP COLUMN LastUpdate
GO

DELETE FROM ReadModel.KeyValueStore WHERE [Key] = 'CurrentScheduleReadModelVersion'
GO