IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_preference_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_preference_delete]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Delete fact schedule preference data
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_preference_delete]
	@person_id int,
	@date_id int,
	@scenario_id int
AS
BEGIN
	IF @scenario_id IS NULL
	BEGIN
		DELETE mart.fact_schedule_preference
		WHERE	person_id=@person_id AND
				date_id=@date_id
	END
	ELSE
	BEGIN
		DELETE mart.fact_schedule_preference
		WHERE	person_id=@person_id AND
				date_id=@date_id AND
				scenario_id=@scenario_id
	END
END
GO

