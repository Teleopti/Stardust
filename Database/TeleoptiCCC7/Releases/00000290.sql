/* 
Trunk initiated: 
2010-07-07 
10:54
By: TOPTINET\NiclasH 
On TELEOPTI554 
*/ 
----------------  
--Name: Johan Ryding  
--Date: 2010-07-13  
--Desc: Removing multiple rows in TemplateSkillDataPeriod, and just saving the recent values. 
--		Also addning a constring on that table on columns Parent, StartDateTime and EndDateTime
--		Needed to use If Exists, becaus it already deployed at some customers
--		2010-08-06 DJ: removing the UNIQUE constraint, since it prevents you from creating new Skills
----------------  


CREATE TABLE [dbo].[temp_TemplateSkillDataPeriod](
           [Id] [uniqueidentifier] NOT NULL DEFAULT (NEWID()),
           [Parent] [uniqueidentifier] NOT NULL,
           [Value] [float] NULL,
           [Seconds] [float] NULL,
           [MaxOccupancy] [float] NULL,
           [MinOccupancy] [float] NULL,
           [PersonBoundary_Maximum] [int] NULL,
           [PersonBoundary_Minimum] [int] NULL,
           [Shrinkage] [float] NULL,
           [StartDateTime] [datetime] NOT NULL,
           [EndDateTime] [datetime] NOT NULL,
           [Efficiency] [float] NULL,
		   [IdId] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO

-- Get data that needs to be deleted, where multiple rows
INSERT INTO dbo.temp_TemplateSkillDataPeriod (Parent,StartDateTime,EndDateTime)
SELECT  Parent,StartDateTime,EndDateTime
FROM dbo.TemplateSkillDataPeriod 
GROUP BY Parent,StartDateTime,EndDateTime
HAVING COUNT(*) > 1

-- Get correct value for each interval
DECLARE @IdId INT
DECLARE @Parent UNIQUEIDENTIFIER
DECLARE @StartDateTime DATETIME
DECLARE @Value FLOAT
DECLARE @Seconds FLOAT
DECLARE @MaxOccupancy FLOAT
DECLARE @MinOccupancy FLOAT
DECLARE @PersonBoundary_Maximum INT
DECLARE @PersonBoundary_Minimum INT
DECLARE @Shrinkage FLOAT
DECLARE @Efficiency FLOAT
SELECT @IdId = 1

WHILE (SELECT MAX(IdId) FROM dbo.temp_TemplateSkillDataPeriod) >= @IdId  
BEGIN
	SELECT @Parent = Parent FROM dbo.temp_TemplateSkillDataPeriod WHERE IdId = @IdId
	SELECT @StartDateTime = StartDateTime FROM dbo.temp_TemplateSkillDataPeriod WHERE IdId = @IdId

	SELECT TOP 1 @Value = Value,
	@Seconds = Seconds,
	@MaxOccupancy = MaxOccupancy,
	@MinOccupancy = MinOccupancy,
	@PersonBoundary_Maximum = PersonBoundary_Maximum,
	@PersonBoundary_Minimum = PersonBoundary_Minimum,
	@Shrinkage = Shrinkage,
	@Efficiency = Efficiency
	FROM TemplateSkillDataPeriod 
	WHERE Parent = @Parent AND StartDateTime = @StartDateTime


	UPDATE temp_TemplateSkillDataPeriod
	SET Value = @Value,
	Seconds = @Seconds,
	MaxOccupancy = @MaxOccupancy,
	MinOccupancy = @MinOccupancy,
	PersonBoundary_Maximum = @PersonBoundary_Maximum,
	PersonBoundary_Minimum = @PersonBoundary_Minimum,
	Shrinkage = @Shrinkage,
	Efficiency = @Efficiency
	WHERE Parent = @Parent
	AND StartDateTime = @StartDateTime

	SELECT @IdId = @IdId + 1 

	CONTINUE 
END

DELETE FROM dbo.TemplateSkillDataPeriod 
WHERE EXISTS (SELECT * FROM dbo.temp_TemplateSkillDataPeriod 
				WHERE Parent = dbo.TemplateSkillDataPeriod.Parent
				AND StartDateTime = dbo.TemplateSkillDataPeriod.StartDateTime
				AND EndDateTime = dbo.TemplateSkillDataPeriod.EndDateTime)

INSERT INTO dbo.TemplateSkillDataPeriod 
SELECT Id,Parent,Value,Seconds,MaxOccupancy,MinOccupancy,PersonBoundary_Maximum,
PersonBoundary_Minimum,Shrinkage,StartDateTime,EndDateTime, Efficiency
FROM dbo.temp_TemplateSkillDataPeriod

DROP TABLE dbo.Temp_TemplateSkillDataPeriod


-- Add constraint
-- Needed to use If Exists, becaus it already deployed at some customers
-- 2010-08-06 DJ: removing the UNIQUE constraint, since it prevents you from creating new Skills
/*
IF NOT EXISTS (SELECT * FROM SYS.INDEXES WHERE name = 'IX_TemplateSkillDataPeriod')
BEGIN
	ALTER TABLE [dbo].[TemplateSkillDataPeriod] ADD  CONSTRAINT [IX_TemplateSkillDataPeriod] UNIQUE  
	(
		[Parent] ASC,
		[StartDateTime] ASC,
		[EndDateTime] ASC
		
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
END
*/
GO

ALTER TABLE [dbo].[ActivityExtender] ADD [AutoPosIntervalSegment] BIGINT

GO


 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (290,'7.1.290') 
