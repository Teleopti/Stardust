IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_business_unit_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_business_unit_add_or_update]
GO
-- =============================================
-- Description:	add or update fact_forecast_workload
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_business_unit_add_or_update] 
		@business_unit_code uniqueidentifier
		,@business_unit_name nvarchar(100)
		,@datasource_update_date smalldatetime
AS
declare @rows int

INSERT INTO mart.dim_business_unit
SELECT 
	@business_unit_code
	,@business_unit_name
	,1
	,GETUTCDATE()
	,GETUTCDATE()
	,@datasource_update_date
WHERE NOT EXISTS(SELECT 1 FROM mart.dim_business_unit 
	WHERE business_unit_code=@business_unit_code)

SET @rows = (SELECT @@ROWCOUNT)
IF @rows = 0
BEGIN
	UPDATE mart.dim_business_unit
	SET
		business_unit_name = @business_unit_name
		,update_date = GETUTCDATE()
		,datasource_update_date = @datasource_update_date
	WHERE business_unit_code=@business_unit_code
END
GO



		
