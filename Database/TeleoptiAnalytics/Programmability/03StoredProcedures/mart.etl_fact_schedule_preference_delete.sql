IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_preference_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_preference_delete]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Delete fact schedule preference data
-- =============================================
-- exec [mart].[etl_fact_schedule_preference_delete] '5B1F11F3-483B-4C2C-BF59-A47900B0DC01', 4809, 7
CREATE PROCEDURE [mart].[etl_fact_schedule_preference_delete]
	@person_code uniqueidentifier,
	@date_id int,
	@scenario_id int
AS
BEGIN
	CREATE TABLE #person_ids(person_id int)

	INSERT INTO #person_ids
		SELECT person_id
		FROM mart.dim_person
		WHERE person_code = @person_code

	IF @scenario_id IS NULL
	BEGIN
		DELETE fsp
		FROM mart.fact_schedule_preference fsp
		INNER JOIN #person_ids p 
			ON fsp.person_id = p.person_id
		WHERE
			fsp.date_id = @date_id
	END
	ELSE
	BEGIN
		DELETE fsp 
		FROM mart.fact_schedule_preference fsp
		INNER JOIN #person_ids p 
			ON fsp.person_id = p.person_id
		WHERE
			fsp.date_id = @date_id AND
			fsp.scenario_id = @scenario_id
	END
END
GO

