/* 
Trunk initiated: 
2010-09-08 
08:32
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Jonas N
--Date: 2010-09-15  
--Desc: Clean duplicates out of TemplateTsaskPeriods to avoid ETL error  
----------------  
CREATE TABLE [dbo].[TemplateTaskPeriod_temp](
	[Id] [uniqueidentifier] NOT NULL DEFAULT (NEWID()),
	[Parent] [uniqueidentifier] NOT NULL,
	[Tasks] [float] NULL,
	[AverageTaskTime] [bigint] NULL,
	[AverageAfterTaskTime] [bigint] NULL,
	[CampaignTasks] [float] NULL,
	[CampaignTaskTime] [float] NULL,
	[CampaignAfterTaskTime] [float] NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[IdId] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]

GO

-- Get data that needs to be deleted, where multiple rows
INSERT INTO [dbo].[TemplateTaskPeriod_temp] (Parent, Minimum, Maximum)
SELECT 	Parent,	Minimum, Maximum
FROM TemplateTaskPeriod
GROUP BY Parent, Minimum, Maximum
HAVING COUNT(*) > 1
ORDER BY Parent, Minimum

-- Get correct value for each interval
DECLARE @IdId INT
DECLARE @Parent UNIQUEIDENTIFIER
DECLARE @Tasks FLOAT
DECLARE @AverageTaskTime BIGINT
DECLARE @AverageAfterTaskTime BIGINT
DECLARE @CampaignTasks FLOAT
DECLARE @CampaignTaskTime FLOAT
DECLARE @CampaignAfterTaskTime FLOAT
DECLARE @Minimum DATETIME
DECLARE @Maximum DATETIME
SELECT @IdId = 1

WHILE (SELECT MAX(IdId) FROM dbo.TemplateTaskPeriod_temp) >= @IdId
BEGIN
	SELECT @Parent = Parent,
			@Minimum = Minimum,
			@Maximum = Maximum
	FROM dbo.TemplateTaskPeriod_temp 
	WHERE IdId = @IdId
	
	SELECT TOP 1
		@Tasks					= Tasks,
		@AverageTaskTime		= AverageTaskTime,
		@AverageAfterTaskTime	= AverageAfterTaskTime,
		@CampaignTasks			= CampaignTasks,
		@CampaignTaskTime		= CampaignTaskTime,
		@CampaignAfterTaskTime	= CampaignAfterTaskTime
	FROM dbo.TemplateTaskPeriod
	WHERE Parent = @Parent
		AND Minimum = @Minimum
		AND Maximum = @Maximum
	
	UPDATE dbo.TemplateTaskPeriod_temp
	SET 
		Tasks					= @Tasks,
		AverageTaskTime		= @AverageTaskTime,
		AverageAfterTaskTime	= @AverageAfterTaskTime,
		CampaignTasks			= @CampaignTasks,
		CampaignTaskTime		= @CampaignTaskTime,
		CampaignAfterTaskTime	= @CampaignAfterTaskTime
	WHERE Parent = @Parent
		AND Minimum = @Minimum
		AND Maximum = @Maximum
	
	SET @IdId = @IdId + 1
	CONTINUE
END

DELETE FROM dbo.TemplateTaskPeriod
WHERE EXISTS	(
					SELECT * FROM dbo.TemplateTaskPeriod_temp
					WHERE Parent	= dbo.TemplateTaskPeriod.Parent
					AND Minimum		= dbo.TemplateTaskPeriod.Minimum
					AND Maximum		= dbo.TemplateTaskPeriod.Maximum
				)

INSERT INTO dbo.TemplateTaskPeriod
SELECT	Id, 
		Parent, 
		Tasks, 
		AverageTaskTime, 
		AverageAfterTaskTime, 
		CampaignTasks, 
		CampaignTaskTime, 
		CampaignAfterTaskTime, 
		Minimum, 
		Maximum
FROM dbo.TemplateTaskPeriod_temp

DROP TABLE [dbo].[TemplateTaskPeriod_temp]

----------------  
--Name: Claes S
--Date: 2010-09-22  
--Desc: Clean database from faulty StartDays in PersonRotation
----------------  
UPDATE dbo.PersonRotation
SET StartDay = 0
WHERE StartDay < 0
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (295,'7.1.295') 
