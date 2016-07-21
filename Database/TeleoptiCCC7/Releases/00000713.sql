BEGIN TRANSACTION
GO
ALTER TABLE ReadModel.PersonScheduleDay
	DROP COLUMN TeamId, SiteId, BusinessUnitId
GO
ALTER TABLE ReadModel.PersonScheduleDay SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
