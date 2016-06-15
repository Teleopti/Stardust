IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_queue_workload_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_queue_workload_delete]
GO
-- =============================================
-- Description:	delete bridge_queue_workload
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_queue_workload_delete] 
	   @workload_id int
      ,@queue_id int
AS
BEGIN

DELETE FROM mart.bridge_queue_workload
WHERE workload_id=@workload_id AND queue_id=@queue_id

END
GO
