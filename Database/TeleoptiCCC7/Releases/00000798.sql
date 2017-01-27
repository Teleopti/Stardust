CREATE TABLE ReadModel.CurrentSchedule (
	PersonId uniqueidentifier not null,
	Schedule nvarchar(max),
	Valid bit
)
GO

INSERT INTO ReadModel.CurrentSchedule (PersonId, Schedule, Valid)
SELECT PersonId, MIN(Schedule), 0 
FROM dbo.AgentState 
WHERE Schedule IS NOT NULL
GROUP BY PersonId
GO

ALTER TABLE dbo.AgentState DROP COLUMN Schedule
GO
ALTER TABLE dbo.AgentState DROP COLUMN NextCheck
GO
