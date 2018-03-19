IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_delete]
GO

-- =============================================
-- Author:		Jonas
-- Create date: 2014-12-08
-- Description:	Delete all schedule rows for shift starting on given date id
-- Ola 2014-12-10 And on just one person ;)
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_delete]
	@shift_startdate_local_id int,
	@person_code uniqueidentifier,
	@scenario_id int
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #DimPerson (
		person_id INT NOT NULL
	)

	INSERT INTO #DimPerson
	SELECT person_id
	  FROM mart.dim_person WITH (NOLOCK)
	 WHERE person_code = @person_code

	DELETE s
	  FROM mart.fact_schedule s
	 INNER JOIN #DimPerson p ON s.person_id = p.person_id
	 WHERE shift_startdate_local_id = @shift_startdate_local_id
	   AND scenario_id = @scenario_id

	DELETE dc
	  FROM mart.fact_schedule_day_count dc
	 INNER JOIN #DimPerson p ON dc.person_id = p.person_id
	 WHERE shift_startdate_local_id = @shift_startdate_local_id
	   AND scenario_id = @scenario_id
END

GO

