IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_queue_save]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[fact_queue_save]
GO

-- =============================================
-- Author:		Jonas
-- Create date: 2014-10-21
-- Description:	Update a row in table mart.fact_queue. If it not exists then insert. Used from web api.
-- =============================================
-- 
CREATE PROCEDURE [mart].[fact_queue_save]
@datasource_id int,
@date_id int,
@interval_id int,
@queue_id int,
@offered_calls decimal(19, 0),
@answered_calls decimal(19, 0),
@answered_calls_within_SL decimal(19, 0),
@abandoned_calls decimal(19, 0),
@abandoned_calls_within_SL decimal(19, 0),
@abandoned_short_calls decimal(19, 0),
@overflow_out_calls decimal(19, 0),
@overflow_in_calls decimal(19, 0),
@talk_time_s decimal(19, 0),
@after_call_work_s decimal(19, 0),
@handle_time_s decimal(19, 0),
@speed_of_answer_s decimal(19, 0),
@time_to_abandon_s decimal(19, 0),
@longest_delay_in_queue_answered_s decimal(19, 0),
@longest_delay_in_queue_abandoned_s decimal(19, 0)
AS

UPDATE mart.fact_queue
SET
    offered_calls = @offered_calls
    ,answered_calls = @answered_calls
    ,answered_calls_within_SL = @answered_calls_within_SL
    ,abandoned_calls = @abandoned_calls
    ,abandoned_calls_within_SL = @abandoned_calls_within_SL
    ,abandoned_short_calls = @abandoned_short_calls
    ,overflow_out_calls = @overflow_out_calls
    ,overflow_in_calls = @overflow_in_calls
    ,talk_time_s = @talk_time_s
    ,after_call_work_s = @after_call_work_s
    ,handle_time_s = @handle_time_s
    ,speed_of_answer_s = @speed_of_answer_s
    ,time_to_abandon_s = @time_to_abandon_s
    ,longest_delay_in_queue_answered_s = @longest_delay_in_queue_answered_s
    ,longest_delay_in_queue_abandoned_s = @longest_delay_in_queue_abandoned_s
    ,insert_date = GETDATE()
WHERE 
	date_id = @date_id AND
	interval_id = @interval_id AND
	queue_id = @queue_id

IF @@ROWCOUNT = 0
BEGIN
	INSERT INTO [mart].[fact_queue]
		([date_id]
		,[interval_id]
		,[queue_id]
		,[offered_calls]
		,[answered_calls]
		,[answered_calls_within_SL]
		,[abandoned_calls]
		,[abandoned_calls_within_SL]
		,[abandoned_short_calls]
		,[overflow_out_calls]
		,[overflow_in_calls]
		,[talk_time_s]
		,[after_call_work_s]
		,[handle_time_s]
		,[speed_of_answer_s]
		,[time_to_abandon_s]
		,[longest_delay_in_queue_answered_s]
		,[longest_delay_in_queue_abandoned_s]
		,[datasource_id])
	VALUES
		(@date_id,
		@interval_id,
		@queue_id,
		@offered_calls,
		@answered_calls,
		@answered_calls_within_SL,
		@abandoned_calls,
		@abandoned_calls_within_SL,
		@abandoned_short_calls,
		@overflow_out_calls,
		@overflow_in_calls,
		@talk_time_s,
		@after_call_work_s,
		@handle_time_s,
		@speed_of_answer_s,
		@time_to_abandon_s,
		@longest_delay_in_queue_answered_s,
		@longest_delay_in_queue_abandoned_s,
		@datasource_id)
END
GO