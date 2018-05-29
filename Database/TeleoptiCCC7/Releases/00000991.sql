CREATE TABLE dbo.ForecastDayOverride
(
	Id uniqueidentifier NOT NULL,
	Version int NOT NULL,
	UpdatedBy uniqueidentifier NOT NULL,
	UpdatedOn smalldatetime NOT NULL,
	Scenario uniqueidentifier NOT NULL,
	Workload uniqueidentifier NOT NULL,
	Date smalldatetime NOT NULL,
	OriginalTasks float NOT NULL,
	OriginalAverageTaskTime float NOT NULL,
	OriginalAverageAfterTaskTime float NOT NULL,
	OverriddenTasks float NULL,
	OverriddenAverageTaskTime float NULL,
	OverriddenAverageAfterTaskTime float NULL
)
GO

ALTER TABLE dbo.ForecastDayOverride ADD CONSTRAINT PK_ForecastDayOverride PRIMARY KEY CLUSTERED	( Id )
GO

ALTER TABLE dbo.ForecastDayOverride ADD CONSTRAINT Unique_ForecastDayOverride UNIQUE NONCLUSTERED
(
	Workload,
	Scenario,
	Date
)
GO

ALTER TABLE dbo.ForecastDayOverride ADD CONSTRAINT FK_ForecastDayOverride_Person_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Person (Id)
GO

ALTER TABLE dbo.ForecastDayOverride ADD CONSTRAINT FK_ForecastDayOverride_Workload FOREIGN KEY (Workload) REFERENCES dbo.Workload (Id)
GO

ALTER TABLE dbo.ForecastDayOverride ADD CONSTRAINT FK_ForecastDayOverride_Scenario FOREIGN KEY (Scenario) REFERENCES dbo.Scenario (Id)
GO
