IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_load_queues]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_load_queues]
GO

CREATE PROCEDURE mart.[raptor_load_queues]
AS
BEGIN
    SET NOCOUNT ON;

	SELECT	
		queue_original_id	QueueOriginalId, 
		queue_agg_id		QueueAggId, 
        queue_id			QueueMartId,
        datasource_id		DataSourceId,
        log_object_name		LogObjectName,
        queue_name			Name,
        queue_Description	[Description]                                        
	FROM mart.dim_queue dq
	WHERE queue_id > -1
	AND queue_original_id NOT IN (SELECT queue_original_id FROM mart.dim_queue_excluded excl 
						WHERE	excl.queue_original_id	= dq.queue_original_id COLLATE DATABASE_DEFAULT
						AND		excl.datasource_id		= dq.datasource_id)
END
GO

