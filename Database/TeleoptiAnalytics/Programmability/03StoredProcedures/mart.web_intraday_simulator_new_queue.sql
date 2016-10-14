IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_simulator_new_queue]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_simulator_new_queue]
GO

-- =============================================
-- Author:		Jonas,Maria & Bharath
-- Create date: 2016-10-12
-- Description:	Create a new queue. Should be used only by ONE workload.
-- =============================================
-- EXEC [mart].[web_intraday_simulator_new_queue] 8, 'myQueue'
CREATE PROCEDURE [mart].[web_intraday_simulator_new_queue]
@workload_id int,
@queue_name nvarchar(100)

AS
BEGIN
	SET NOCOUNT ON;
    
	DECLARE @queue_id int, @queue_agg_id int

	SET @queue_agg_id = 90000 + @workload_id

	-- Create the queue
	INSERT INTO [mart].[dim_queue]
           ([queue_name]
           ,[queue_description],
		   [datasource_id],
		   [queue_agg_id],
		   [queue_original_id])
     SELECT
		@queue_name,
		@queue_name,
		1,
		@queue_agg_id,
		@queue_agg_id

	
	SET @queue_id = @@IDENTITY

	-- Add the new queue to the workload
	INSERT INTO [mart].[bridge_queue_workload]
           ([queue_id]
           ,[workload_id]
           ,[skill_id]
           ,[business_unit_id],
		   [datasource_id])
    SELECT
		@queue_id,
		@workload_id,
		w.skill_id,
		w.business_unit_id,
		1
	FROM mart.dim_workload w
	WHERE 
		w.workload_id = @workload_id

	SELECT @queue_id
END

GO

