IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[LastChangedDataInit]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[LastChangedDataInit]
GO

CREATE PROCEDURE [mart].[LastChangedDataInit]
@stepName nvarchar(500),
@lastTime datetime,
@buId uniqueidentifier 
-- =============================================
-- Author:		David
-- Create date: 2013-05-03
-- Description:	Insert all BUs and Steps, on ETL startup
-- =============================================
-- Date			Who	Description
-- =============================================
AS
SET NOCOUNT ON
DECLARE @StepName TABLE (stepName varchar(500) NOT NULL)

INSERT INTO @StepName
SELECT 'Schedules'
INSERT INTO @StepName
SELECT 'Preferences'
INSERT INTO @StepName
SELECT 'Requests'
INSERT INTO @StepName
SELECT 'Permissions'

--insert missing stepName for each BU
INSERT INTO mart.LastUpdatedPerStep(StepName,BusinessUnit,[Date])
SELECT stepName, bu.Id,GETUTCDATE()
FROM @StepName s
CROSS JOIN BusinessUnit bu
WHERE NOT EXISTS (
	SELECT *
	FROM mart.LastUpdatedPerStep etl
	WHERE etl.stepName = s.stepName
	AND etl.BusinessUnit = bu.id
	AND bu.IsDeleted = 0
	)
GO