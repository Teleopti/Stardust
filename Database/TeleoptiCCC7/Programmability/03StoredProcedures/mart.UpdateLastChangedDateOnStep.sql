IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[UpdateLastChangedDateOnStep]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[UpdateLastChangedDateOnStep]
GO

-- =============================================
-- Author:		Ola
-- Create date: 2013-04-16
-- Description:	Updates the time who was the last change for schedule data
-- =============================================
-- Date			Who	Description
-- =============================================
-- exec [mart].[UpdateLastChangedDateOnStep] 'stg_schedule, stg_schedule_day_absence_count', '4F949017-AF0D-4DA3-80BC-A18900580184', '2013-04-01'
-- SELECT * FROM mart.LastUpdatedPerStep
-- DELETE mart.LastUpdatedPerStep
-- =============================================

CREATE PROCEDURE [mart].[UpdateLastChangedDateOnStep]
@stepName nvarchar(500),
@buId uniqueidentifier,
@thisTime datetime

AS
IF NOT EXISTS(SELECT * FROM mart.LastUpdatedPerStep WHERE StepName = @stepName AND BusinessUnit = @buId)
	INSERT mart.LastUpdatedPerStep VALUES(@stepName, @buId, @thisTime)
ELSE
	UPDATE mart.LastUpdatedPerStep SET Date = @thisTime WHERE StepName = @stepName AND BusinessUnit = @buId
