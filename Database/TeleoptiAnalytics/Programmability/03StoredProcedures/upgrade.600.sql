/* Correct data in some fact tables where person_period is incorrect due to bug #47317 */

SET NOCOUNT ON

--Exit if version 600 was applied more than one day ago
IF EXISTS (SELECT * FROM dbo.DatabaseVersion WHERE BuildNumber>=599 AND AddedDate<=DATEADD(D,-1,GETDATE()))
	RETURN

DECLARE @current_person_code uniqueidentifier, @person_ids nvarchar(max)

DECLARE AgentCursor CURSOR FOR
SELECT person_code FROM mart.dim_person 
WHERE person_id <>-1
GROUP BY person_code
OPEN AgentCursor

FETCH NEXT FROM AgentCursor INTO @current_person_code
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @person_ids = (Select CONVERT(nvarchar(8), person_id) + ',' AS [text()]
	From mart.dim_person
	WHERE person_code = @current_person_code
	For XML PATH (''))

	EXEC mart.etl_fact_schedule_update_unlinked_personids @person_ids
	EXEC mart.etl_fact_schedule_preference_update_unlinked_personids @person_ids
	EXEC mart.etl_fact_request_update_unlinked_personids @person_ids
	EXEC mart.etl_fact_hourly_availability_update_unlinked_personids @person_ids

	FETCH NEXT FROM AgentCursor INTO @current_person_code
END
CLOSE AgentCursor
DEALLOCATE AgentCursor