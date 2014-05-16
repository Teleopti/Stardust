IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_data_mart_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_data_mart_delete]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-02-19
-- Description:	Delete data in all dimension and fact tables
-- exec [mart].[etl_data_mart_delete] 1
-- ---Change Log---
-- 2009-02-11 New Mart schema KJ
-- 2010-08-17 change order for dim_scorecard, removed fact_contract
-- =============================================
CREATE PROCEDURE [mart].[etl_data_mart_delete]
@DeleteAll BIT = 0, --Delete initial load too 
@DeleteFrom smalldatetime = NULL --Only from a certain UTC-date. Make sure you start ETL from one day earlier than this UTC-date to cover any time zone related gaps.

WITH EXECUTE AS OWNER
AS
BEGIN
Declare @Date_Id int


IF @DeleteFrom IS NULL
BEGIN
	-- Delete data from bridge and other tables
	DELETE FROM mart.bridge_queue_workload
	DELETE FROM mart.bridge_skillset_skill
	DELETE FROM mart.v_permission_report
	DELETE FROM mart.scorecard_kpi
	DELETE FROM dbo.aspnet_Membership
	DELETE FROM dbo.aspnet_Users
	DELETE FROM mart.bridge_acd_login_person
	DELETE FROM mart.bridge_group_page_person

	
    -- Delete data from fact tables
	TRUNCATE TABLE mart.fact_schedule
--	TRUNCATE TABLE mart.fact_contract
	TRUNCATE TABLE mart.fact_queue
	TRUNCATE TABLE mart.fact_forecast_workload
	TRUNCATE TABLE mart.fact_schedule_forecast_skill
	TRUNCATE TABLE mart.fact_agent
	TRUNCATE TABLE mart.fact_agent_queue
	TRUNCATE TABLE mart.fact_kpi_targets_team
	TRUNCATE TABLE mart.fact_schedule_deviation
	TRUNCATE TABLE mart.fact_schedule_day_count
	TRUNCATE TABLE mart.fact_schedule_preference
	TRUNCATE TABLE mart.fact_request
	TRUNCATE TABLE mart.fact_requested_days
	TRUNCATE TABLE mart.fact_quality
	TRUNCATE TABLE mart.fact_agent_skill
	
	
	-- Delete data from dim tables
	DELETE FROM mart.dim_day_off
	DELETE FROM mart.dim_shift_length
	DELETE FROM mart.dim_shift_category
	DELETE FROM mart.dim_scenario
	DELETE FROM mart.dim_acd_login
	DELETE FROM mart.dim_person
	DELETE FROM mart.dim_activity
	DELETE FROM mart.dim_absence
	DELETE FROM mart.dim_queue
	--DELETE FROM mart.dim_preference_type
	DELETE FROM mart.dim_kpi
	DELETE FROM mart.dim_workload
	DELETE FROM mart.dim_skillset
	DELETE FROM mart.dim_skill 
	DELETE FROM mart.dim_team
	DELETE FROM mart.dim_scorecard
	DELETE FROM mart.dim_site
	DELETE FROM mart.dim_business_unit
	DELETE FROM mart.dim_overtime
	DELETE FROM mart.dim_group_page
	DELETE FROM mart.dim_quality_quest

	--To realy delete All data, call this proc with @DeleteAll = 1
	--This data corresponds to mart.sys_datasource_load followed by ETL.InitalLoad 
	IF (@DeleteAll = 1)
		BEGIN
			TRUNCATE TABLE mart.bridge_time_zone
			DELETE FROM mart.dim_time_zone
			DELETE FROM mart.dim_date
			DELETE FROM mart.dim_interval
			DELETE FROM mart.sys_datasource WHERE datasource_id > 1
			TRUNCATE TABLE Queue.Messages
		END
END
ELSE --Only from a certain date!
BEGIN
-- exec [mart].[etl_data_mart_delete] 0,'2012-01-01'
	SELECT @Date_Id = date_id
	FROM mart.dim_date
	WHERE date_date = @DeleteFrom
	
	--SELECT @Date_Id
	
	--First directly by date_id
	DELETE FROM mart.fact_schedule					WHERE schedule_date_id >= @Date_Id
	DELETE FROM mart.fact_schedule					WHERE activity_startdate_id >= @Date_Id
	DELETE FROM mart.fact_schedule					WHERE activity_enddate_id >= @Date_Id
	DELETE FROM mart.fact_schedule					WHERE shift_startdate_id >= @Date_Id
	DELETE FROM mart.fact_schedule					WHERE shift_enddate_id >= @Date_Id
	DELETE FROM mart.fact_queue						WHERE date_id >= @Date_Id
	DELETE FROM mart.fact_queue						WHERE local_date_id >= @Date_Id
	DELETE FROM mart.fact_forecast_workload			WHERE date_id >= @Date_Id
	DELETE FROM mart.fact_schedule_forecast_skill	WHERE date_id >= @Date_Id
	DELETE FROM mart.fact_agent						WHERE date_id >= @Date_Id
	DELETE FROM mart.fact_agent						WHERE local_date_id >= @Date_Id
	DELETE FROM mart.fact_agent_queue				WHERE date_id >= @Date_Id
	DELETE FROM mart.fact_agent_queue				WHERE local_date_id >= @Date_Id
	DELETE FROM mart.fact_schedule_deviation		WHERE date_id >= @Date_Id
	DELETE FROM mart.fact_schedule_day_count		WHERE date_id >= @Date_Id
	DELETE FROM mart.fact_schedule_preference		WHERE date_id >= @Date_Id
	DELETE FROM mart.fact_quality					WHERE date_id >= @Date_Id

	--Bridge and dim date
	DELETE mart.bridge_time_zone
	FROM mart.bridge_time_zone a
	INNER JOIN mart.dim_date b ON a.date_id = b.date_id
	WHERE b.date_id >= @Date_Id
	
	DELETE mart.bridge_time_zone
	FROM mart.bridge_time_zone a
	INNER JOIN mart.dim_date b ON a.local_date_id = b.date_id
	WHERE b.date_id >= @Date_Id
	
	DELETE FROM mart.dim_date WHERE date_id >= @Date_Id

END
	

END

GO

