ALTER TABLE ReadModel.PersonScheduleDay ADD
	ScheduleLoadedTime datetime NOT NULL CONSTRAINT DF_PersonScheduleDay_ScheduleLoadedTime DEFAULT GETDATE()
GO