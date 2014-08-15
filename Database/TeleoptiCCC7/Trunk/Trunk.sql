--Name: Xinfeng, Chundan
--Date: 2014-08-11  
--Desc: Add new column for agent badges
----------------  
ALTER TABLE [dbo].[AgentBadge] ADD [LastCalculatedDate] [datetime] NULL
GO
UPDATE [dbo].[AgentBadge]
SET [LastCalculatedDate] = '2000-01-01'
GO
ALTER TABLE [dbo].[AgentBadge] ALTER COLUMN [LastCalculatedDate] [datetime] NOT NULL
GO

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'CalculationTime' AND [object_id] = OBJECT_ID(N'AgentBadgeThresholdSettings'))
BEGIN
    ALTER TABLE [dbo].[AgentBadgeThresholdSettings] DROP COLUMN [CalculationTime]
END
GO


----------------  
--Name: Mingdi
--Date: 2014-08-13  
--Desc: Add new column for filter dayoff
----------------  
ALTER TABLE [ReadModel].[PersonScheduleDay] ADD [IsDayOff] [bit] NULL -- 245557 rows => 166765
GO

--populate with data from data source.
UPDATE psd
SET IsDayOff = 
	CASE
		WHEN pa.DayOffTemplate IS NULL THEN 0
		ELSE 1
	END
FROM dbo.PersonAssignment pa
INNER JOIN [ReadModel].[PersonScheduleDay] psd
	ON psd.PersonId = pa.Person
	AND psd.BelongsToDate = pa.[Date]
INNER JOIN dbo.Scenario s
	ON s.Id = pa.Scenario
	AND s.DefaultScenario = 1

--Next statment would be good in order to avoid a NULL design on the bit.
-- .... The PersonScheduleDay holds meeting and Absence. In that case NULL :-(
--ALTER TABLE [ReadModel].[PersonScheduleDay] ADD [IsDayOff] [bit NOT] NULL