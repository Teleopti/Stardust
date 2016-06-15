IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_workload_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_workload_add_or_update]
GO
-- =============================================
-- Description:	add or update dim_workload
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_workload_add_or_update] 
	   @workload_code uniqueidentifier
      ,@workload_name nvarchar(100)
      ,@skill_id int
      ,@skill_code uniqueidentifier
      ,@skill_name nvarchar(100)
      ,@time_zone_id int
      ,@forecast_method_code uniqueidentifier
      ,@forecast_method_name nvarchar(100)
      ,@percentage_offered float
      ,@percentage_overflow_in float
      ,@percentage_overflow_out float
      ,@percentage_abandoned float
      ,@percentage_abandoned_short float
      ,@percentage_abandoned_within_service_level float
      ,@percentage_abandoned_after_service_level float
      ,@business_unit_id int
	  ,@datasource_update_date datetime
	  ,@is_deleted bit
AS
declare @rows int

INSERT INTO mart.dim_workload
SELECT 
	@workload_code
	,@workload_name
	,@skill_id
	,@skill_code
	,@skill_name
	,@time_zone_id
	,@forecast_method_code
	,@forecast_method_name
	,@percentage_offered
	,@percentage_overflow_in
	,@percentage_overflow_out
	,@percentage_abandoned
	,@percentage_abandoned_short
	,@percentage_abandoned_within_service_level
	,@percentage_abandoned_after_service_level
	,@business_unit_id
	,1
	,getutcdate()
	,getutcdate()
	,@datasource_update_date
	,@is_deleted
WHERE NOT EXISTS(SELECT 1 FROM mart.dim_workload WHERE workload_code=@workload_code)

SET @rows = (SELECT @@ROWCOUNT)
IF @rows = 0
BEGIN
	UPDATE mart.dim_workload
	SET
		workload_name = @workload_name
		,skill_id = @skill_id
		,skill_code = @skill_code
		,skill_name = @skill_name
		,time_zone_id = @time_zone_id
		,forecast_method_code = @forecast_method_code
		,forecast_method_name = @forecast_method_name
		,percentage_offered = @percentage_offered
		,percentage_overflow_in = @percentage_overflow_in
		,percentage_overflow_out = @percentage_overflow_out
		,percentage_abandoned = @percentage_abandoned
		,percentage_abandoned_short = @percentage_abandoned_short
		,percentage_abandoned_within_service_level = @percentage_abandoned_within_service_level
		,percentage_abandoned_after_service_level = @percentage_abandoned_after_service_level
		,update_date = getutcdate()
		,datasource_update_date = @datasource_update_date
		,is_deleted = @is_deleted
	WHERE
	workload_code=@workload_code
END

select workload_id 
from mart.dim_workload
where 
workload_code=@workload_code

GO



		
