
-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-19
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

ALTER TABLE dbo.PlanningPeriodJobResult
	DROP CONSTRAINT UQ_PlanningPeriodJobResult

ALTER TABLE dbo.PlanningPeriodJobResult ADD CONSTRAINT
	PK_PlanningPeriodJobResult PRIMARY KEY CLUSTERED 
	(
		PlanningPeriod,
		JobResult
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.PlanningPeriodJobResult SET (LOCK_ESCALATION = TABLE)
GO
-----------------------------------------------------------

ALTER TABLE dbo.QueueSourceCollection
	DROP CONSTRAINT UQ_QueueSourceCollection
GO
ALTER TABLE dbo.QueueSourceCollection ADD CONSTRAINT
	PK_QueueSourceCollection PRIMARY KEY CLUSTERED 
	(
		Workload,
		QueueSource
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.QueueSourceCollection SET (LOCK_ESCALATION = TABLE)
GO
-----------------------------------------------------------

ALTER TABLE dbo.RecurrentWeeklyMeetingWeekDays
	DROP CONSTRAINT FK_RecurrentWeeklyMeetingWeekDays_RecurrentWeeklyMeeting
GO

;WITH dublett (RecurrentWeeklyMeeting, DayOfWeek, rank) AS
(
SELECT
	RecurrentWeeklyMeeting, 
	DayOfWeek, 
	ROW_NUMBER() OVER (PARTITION BY RecurrentWeeklyMeeting, DayOfWeek ORDER BY RecurrentWeeklyMeeting, DayOfWeek ASC) AS 'RANK'
FROM RecurrentWeeklyMeetingWeekDays 
)
DELETE FROM dublett
WHERE RANK >1;
GO

ALTER TABLE dbo.RecurrentWeeklyMeeting SET (LOCK_ESCALATION = TABLE)
GO

CREATE TABLE dbo.Tmp_RecurrentWeeklyMeetingWeekDays
	(
		RecurrentWeeklyMeeting uniqueidentifier NOT NULL,
		DayOfWeek int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_RecurrentWeeklyMeetingWeekDays SET (LOCK_ESCALATION = TABLE)
GO

IF EXISTS(SELECT * FROM dbo.RecurrentWeeklyMeetingWeekDays)
	 EXEC('INSERT INTO dbo.Tmp_RecurrentWeeklyMeetingWeekDays (RecurrentWeeklyMeeting, DayOfWeek)
		SELECT RecurrentWeeklyMeeting, DayOfWeek FROM dbo.RecurrentWeeklyMeetingWeekDays WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.RecurrentWeeklyMeetingWeekDays
GO

EXECUTE sp_rename N'dbo.Tmp_RecurrentWeeklyMeetingWeekDays', N'RecurrentWeeklyMeetingWeekDays', 'OBJECT' 
GO

ALTER TABLE dbo.RecurrentWeeklyMeetingWeekDays ADD CONSTRAINT
	PK_RecurrentWeeklyMeetingWeekDays PRIMARY KEY CLUSTERED 
	(
		RecurrentWeeklyMeeting,
		DayOfWeek
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.RecurrentWeeklyMeetingWeekDays ADD CONSTRAINT
	FK_RecurrentWeeklyMeetingWeekDays_RecurrentWeeklyMeeting FOREIGN KEY
	(
		RecurrentWeeklyMeeting
	) REFERENCES dbo.RecurrentWeeklyMeeting
	(
		RecurrentMeetingOption
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
