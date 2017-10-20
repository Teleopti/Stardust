IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_queue_workload_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_queue_workload_add_or_update]
GO
-- =============================================
-- Description:	add or update bridge_queue_workload
-- =============================================
-- exec [mart].[etl_bridge_queue_workload_add_or_update] 28, 11, 9, 1, '2017-10-20 13:21:33'
CREATE PROCEDURE [mart].[etl_bridge_queue_workload_add_or_update] 
		@queue_id int
		,@workload_id int
		,@skill_id int
		,@business_unit_id int
		,@datasource_update_date smalldatetime
AS
DECLARE @rows int, @should_insert bit
DECLARE @maxdate as smalldatetime = CAST('20591231' as smalldatetime)

IF EXISTS (SELECT 1 FROM mart.bridge_queue_workload WHERE queue_id=@queue_id AND workload_id=@workload_id)
	SET @should_insert = 0
ELSE
	SET @should_insert= 1

IF @should_insert = 1
BEGIN
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

	-- Since the queue now is conected to a proper workload we disconnect it from 'Not Defined'
	DELETE b
	FROM 
		mart.bridge_queue_workload b
	WHERE 
		queue_id = @queue_id AND
		workload_id = -1 AND
		skill_id = -1
END
ELSE
BEGIN
	UPDATE mart.bridge_queue_workload
	SET
		skill_id=@skill_id
		,update_date=GETUTCDATE()
		,datasource_update_date=@datasource_update_date
	WHERE
		queue_id=@queue_id AND 
		workload_id=@workload_id
END
GO