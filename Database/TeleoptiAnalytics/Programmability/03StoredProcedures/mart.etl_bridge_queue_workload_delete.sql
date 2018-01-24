IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_queue_workload_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_queue_workload_delete]
GO
-- =============================================
-- Description:	delete bridge_queue_workload
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_queue_workload_delete] 
       @workload_id int
     , @queue_id int
AS
BEGIN

DECLARE @queueBridgeCount int;
SELECT @queueBridgeCount = COUNT(*)
  FROM mart.bridge_queue_workload
 WHERE queue_id = @queue_id

IF @queueBridgeCount = 1
  -- If this is the only one record to be deleted, then connect the queue to "Not Defined"
  UPDATE mart.bridge_queue_workload
     SET workload_id = -1, skill_id = -1, business_unit_id = -1, datasource_id = -1, insert_date = GETDATE(), update_date = GETDATE()
   WHERE queue_id = @queue_id
ELSE IF @queueBridgeCount > 1
  DELETE FROM mart.bridge_queue_workload
   WHERE workload_id=@workload_id AND queue_id=@queue_id

END
GO