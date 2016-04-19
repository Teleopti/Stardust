IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_preference_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_preference_delete]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Delete fact schedule preference data
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_preference_delete]
	@personId int,
	@dateId int,
	@scenarioId int
AS
BEGIN
	IF @scenarioId IS NULL
	BEGIN
		DELETE mart.fact_schedule_preference
		WHERE	person_id=@personId AND
				date_id=@dateId
	END
	ELSE
	BEGIN
		DELETE mart.fact_schedule_preference
		WHERE	person_id=@personId AND
				date_id=@dateId AND
				scenario_id=@scenarioId
	END
END
GO

