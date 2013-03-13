
/****** Object:  StoredProcedure [mart].[report_mobilereport_init]    Script Date: 01/26/2012 14:11:51 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_mobilereport_init]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_mobilereport_init]
GO


CREATE PROCEDURE [mart].[report_mobilereport_init]
	@person_code UNIQUEIDENTIFIER, 
	@language_id INT,	
	@bu_code UNIQUEIDENTIFIER,
	@skill_set NVARCHAR(MAX),
	@time_zone_code NVARCHAR(100)
	AS
	/*
	Purpose: Simplify mapping between raptor and analytics db
	Created: 20120119 Peter W 
	Last Modified: 
	*/
	
	-- EXEC [mart].[report_mobilereport_init] '928DD0BC-BF40-412E-B970-9B5E015AADEA', 1053, '928DD0BC-BF40-412E-B970-9B5E015AADEA', '2,3,4,7,98,22', 'UTC'
	
	DECLARE @scenario_id INT
	DECLARE @interval_from INT
	DECLARE @interval_to INT
	DECLARE @time_zone_id INT
	DECLARE @sl_calc_id INT


	SET @sl_calc_id = 1 --  Answered Calls Within Service Level Threshold /Offered Calls, 2  Answered and Abandoned Calls Within Service Level Threshold /Offered Calls, 3  Answered Calls Within Service Level Threshold / Answered Calls
	
	
	SELECT TOP 1 @scenario_id = scenario_id 
			FROM [mart].[dim_scenario] 
		WHERE default_scenario=1 AND 
				business_unit_code=@bu_code
	
	SELECT TOP 1 @interval_from = MIN(interval_id), @interval_to = MAX(interval_id) 
			FROM [mart].[dim_interval] 
	
	SELECT TOP 1 @time_zone_id = ISNULL(time_zone_id, 0) 
			FROM [mart].[dim_time_zone] 
		WHERE time_zone_code = @time_zone_code
			ORDER BY datasource_id DESC

	-- logic [report_control_workload_get]
	CREATE TABLE #skills(id int)
	CREATE TABLE #workloads(id int)
	INSERT INTO #skills
		SELECT * FROM SplitStringInt(@skill_set)

	DECLARE @skill_id int
	SET @skill_id = (select id from #skills where id=-2)
	IF @Skill_id = -2
		INSERT INTO #skills
			SELECT skill_id FROM [mart].[dim_skill]
				WHERE skill_id NOT IN (select id from #skills)
					AND	is_deleted = 0
	
	
	INSERT INTO #workloads
		SELECT workload_id FROM [mart].[dim_workload]
			WHERE skill_id IN (select id from #skills)
				AND	is_deleted = 0

			
	SELECT TOP 1 @scenario_id as Scenario,	
			@interval_from as IntervalFrom, 
			@interval_to as IntervalTo, 
			@time_zone_id as TimeZone, 
			@sl_calc_id as ServiceLevelCalculationId,
			SUBSTRING((Select ',' + CAST(id AS VARCHAR(10))
					FROM #Skills WHERE id<>-2
                FOR XML Path('')), 2, 2147483647) AS SkillSet,
            SUBSTRING((Select ',' + CAST(id AS VARCHAR(10))
					FROM #workloads
                FOR XML Path('')), 2, 2147483647) AS WorkloadSet
				
					
	



GO

