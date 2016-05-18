IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_overtime_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_overtime_add_or_update]
GO
-- =============================================
-- Description:	add or update dim_overtime
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_overtime_add_or_update] 
@overtime_code uniqueidentifier,
@overtime_name nvarchar(100),
@business_unit_id int,
@datasource_update_date smalldatetime,
@is_deleted bit
AS
declare @rows int

INSERT INTO mart.dim_overtime
SELECT 
	@overtime_code
	,@overtime_name
	,@business_unit_id
	,1
	,GETUTCDATE()
	,GETUTCDATE()
	,@datasource_update_date
	,@is_deleted 
WHERE NOT EXISTS(SELECT 1 FROM mart.dim_overtime WHERE overtime_code=@overtime_code)

SET @rows = (SELECT @@ROWCOUNT)
IF @rows = 0
BEGIN
	UPDATE mart.dim_overtime
	SET
		overtime_name = @overtime_name
		,business_unit_id = @business_unit_id
		,update_date = GETUTCDATE()
		,datasource_update_date = @datasource_update_date
		,is_deleted = @is_deleted
	WHERE
	overtime_code = @overtime_code
	and datasource_update_date < @datasource_update_date -- Only update if the information is newer
END
GO
