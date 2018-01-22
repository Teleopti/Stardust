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

	DECLARE AgentCursor CURSOR FOR
	SELECT TOP 10 person_code FROM mart.bug_47571
	WHERE isFixed = 0
	ORDER BY datasource_update_date desc

	OPEN AgentCursor

	FETCH NEXT FROM AgentCursor INTO @current_person_code
	WHILE @@FETCH_STATUS = 0
	BEGIN
		--get person ids
		SET @person_ids = (Select CONVERT(nvarchar(8), person_id) + ',' AS [text()]
		From mart.dim_person
		WHERE person_code = @current_person_code
		For XML PATH (''))

		--update this persons personPeriods in all fact tables we messed up
		EXEC mart.etl_fact_schedule_update_unlinked_personids @person_ids
		EXEC mart.etl_fact_schedule_preference_update_unlinked_personids @person_ids
		EXEC mart.etl_fact_request_update_unlinked_personids @person_ids
		EXEC mart.etl_fact_hourly_availability_update_unlinked_personids @person_ids

		--marked as fixed
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



