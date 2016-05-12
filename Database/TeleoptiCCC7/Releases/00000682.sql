ALTER TABLE ReadModel.PersonScheduleProjectionLoadTime ADD [Version] INT NULL
GO
ALTER TABLE ReadModel.PersonScheduleProjectionLoadTime ALTER COLUMN ScheduleLoadedTime DATETIME NULL
