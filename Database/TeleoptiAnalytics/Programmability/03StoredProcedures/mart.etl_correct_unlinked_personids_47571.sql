IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_correct_unlinked_personids_47571]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_correct_unlinked_personids_47571]
GO

-- =============================================
-- Description:	For bug #47571. Correct unlinked person periods in fact tables.
-- =============================================
CREATE PROCEDURE [mart].[etl_correct_unlinked_personids_47571]
@schedule_id int,
@is_delayed_job bit = 0
AS
BEGIN
	DECLARE @current_person_code uniqueidentifier, @person_ids nvarchar(max)

	CREATE TABLE #schedule_intervals_to_correct(
       date_id int,
	   interval_id int,
       scenario_id int)

	DECLARE AgentCursor CURSOR FOR
	SELECT TOP 10 person_code FROM mart.bug_47571
	WHERE isFixed = 0
	ORDER BY datasource_update_date desc

	OPEN AgentCursor

	FETCH NEXT FROM AgentCursor INTO @current_person_code
	WHILE @@FETCH_STATUS = 0
	BEGIN
		/* A. Remove potential duplicate of person_ids from mart.fact_schedule */
		-- A1. Get duplicate schedule intervals
		INSERT INTO #schedule_intervals_to_correct
		SELECT fs.shift_startdate_local_id, fs.interval_id, fs.scenario_id
		FROM mart.fact_schedule fs
			INNER JOIN mart.dim_person p ON fs.person_id = p.person_id
		WHERE p.person_code = @current_person_code
		GROUP BY fs.shift_startdate_local_id, fs.interval_id, fs.scenario_id
		HAVING COUNT(1) > 1

		-- A2. Delete duplicate schedule intervals
		DELETE fs
		FROM 
			#schedule_intervals_to_correct ptc
			INNER JOIN mart.dim_person p 
				ON p.person_code = @current_person_code
			INNER JOIN mart.fact_schedule fs 
				ON p.person_id = fs.person_id
					AND ptc.date_id = fs.shift_startdate_local_id
					AND ptc.interval_id = fs.interval_id
					AND ptc.scenario_id = fs.scenario_id
		WHERE NOT fs.shift_startdate_local_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local

		TRUNCATE TABLE #schedule_intervals_to_correct


		/* B. Correct incorrect person_id in fact tables */
		-- B1. get person ids
		SET @person_ids = (Select CONVERT(nvarchar(8), person_id) + ',' AS [text()]
		From mart.dim_person
		WHERE person_code = @current_person_code
		For XML PATH (''))

		--B2. update this persons personPeriods in all fact tables we messed up
		EXEC mart.etl_fact_schedule_update_unlinked_personids @person_ids
		EXEC mart.etl_fact_schedule_preference_update_unlinked_personids @person_ids
		EXEC mart.etl_fact_request_update_unlinked_personids @person_ids
		EXEC mart.etl_fact_hourly_availability_update_unlinked_personids @person_ids

		--B3. marked as fixed
		UPDATE mart.bug_47571
		SET isFixed=1
		WHERE person_code = @current_person_code

		FETCH NEXT FROM AgentCursor INTO @current_person_code
	END
	CLOSE AgentCursor
	DEALLOCATE AgentCursor

	-- If we are ready, delete Scheduled ETL job 
	IF EXISTS(SELECT TOP 1 1 FROM mart.bug_47571 WHERE isFixed = 0)
	BEGIN
		INSERT INTO mart.etl_job_delayed (stored_procedured, parameter_string)
		SELECT 'mart.etl_correct_unlinked_personids_47571', '@schedule_id=' + CONVERT(nvarchar(5), @schedule_id)
	END
	ELSE
	BEGIN
		-- Delete job schedule here
		EXEC [mart].[etl_job_delete_schedule] @schedule_id
	END
END

GO