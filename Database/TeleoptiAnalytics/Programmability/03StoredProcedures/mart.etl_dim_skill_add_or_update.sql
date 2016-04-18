IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_skill_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_skill_add_or_update]
GO
-- =============================================
-- Description:	add or update skill
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_skill_add_or_update] 
@skill_code uniqueidentifier,
@skill_name nvarchar(100),
@time_zone_id int,
@forecast_method_code uniqueidentifier,
@forecast_method_name nvarchar(100),
@business_unit_id int,
@datasource_update_date smalldatetime,
@is_deleted bit

AS
declare @rows int

INSERT INTO mart.dim_skill
select 
	@skill_code
	,@skill_name
	,@time_zone_id
	,@forecast_method_code
	,@forecast_method_name
	,@business_unit_id
	,1
	,GETUTCDATE()
	,GETUTCDATE()
	,@datasource_update_date
	,@is_deleted 
where NOT EXISTS(select 1 from mart.dim_skill where skill_code=@skill_code)

SET @rows = (select @@ROWCOUNT)
if @rows = 0
	UPDATE mart.dim_skill
	SET
		skill_name = @skill_name
		,time_zone_id =@time_zone_id
		,forecast_method_code = @forecast_method_code
		,forecast_method_name = @forecast_method_name
		,business_unit_id = @business_unit_id
		,update_date = GETUTCDATE()
		,datasource_update_date = @datasource_update_date
		,is_deleted = @is_deleted
	where
	skill_code = @skill_code

GO
