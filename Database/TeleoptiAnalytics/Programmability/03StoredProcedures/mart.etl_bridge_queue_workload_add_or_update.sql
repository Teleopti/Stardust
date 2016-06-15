IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_queue_workload_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_queue_workload_add_or_update]
GO
-- =============================================
-- Description:	add or update bridge_queue_workload
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_queue_workload_add_or_update] 
		@queue_id int
		,@workload_id int
		,@skill_id int
		,@business_unit_id int
		,@datasource_update_date smalldatetime
AS
declare @rows int

INSERT INTO mart.bridge_queue_workload
SELECT 
	@queue_id
	,@workload_id
	,@skill_id
	,@business_unit_id
	,1
	,GETUTCDATE()
	,GETUTCDATE()
	,@datasource_update_date
WHERE NOT EXISTS(SELECT 1 FROM mart.bridge_queue_workload WHERE queue_id=@queue_id AND workload_id=@workload_id)

SET @rows = (SELECT @@ROWCOUNT)
IF @rows = 0
BEGIN
	UPDATE mart.bridge_queue_workload
	SET
		skill_id=@skill_id
		,update_date=GETUTCDATE()
		,datasource_update_date=@datasource_update_date
	WHERE
	queue_id=@queue_id AND workload_id=@workload_id
END
GO



		
