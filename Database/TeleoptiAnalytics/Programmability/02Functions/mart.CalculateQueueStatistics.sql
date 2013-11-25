IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[CalculateQueueStatistics]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[CalculateQueueStatistics]
GO

-- =============================================
-- Author:		RobinK
-- Create date: 2010-07-05
-- Description:	Function that returns the calculated calls for the given
--				parameters
-- Note:		If this function is altered in any way the changes must be applied to the domain QueueStatisticsCalculator as well!
-- =============================================
CREATE FUNCTION [mart].[CalculateQueueStatistics]
(
	@percentage_offered float,
	@percentage_overflow_in float,
	@percentage_overflow_out float,
	@percentage_abandoned float,
	@percentage_abandoned_short float,
	@percentage_abandoned_within_service_level float,
	@percentage_abandoned_after_service_level float,
	@offered_calls decimal(24,5),
	@abandoned_calls decimal(24,5),
	@abandoned_calls_within_SL decimal(24,5),
	@abandoned_short_calls decimal(18,0),
	@overflow_out_calls decimal(24,5),
	@overflow_in_calls decimal(24,5)
)
RETURNS decimal(24,5) 
AS
BEGIN
	DECLARE @abandoned_calls_after_SL decimal(24,5)
	DECLARE @result decimal(24,5)
	
	SET @abandoned_calls_after_SL = @abandoned_calls - @abandoned_calls_within_SL - @abandoned_short_calls
	SET @result =	@percentage_offered * @offered_calls +
					@percentage_overflow_in * @overflow_in_calls +
					@percentage_overflow_out * @overflow_out_calls +
					@percentage_abandoned * @abandoned_calls +
					@percentage_abandoned_short * @abandoned_short_calls +
					@percentage_abandoned_within_service_level * @abandoned_calls_within_SL +
					@percentage_abandoned_after_service_level * @abandoned_calls_after_SL

	IF @result<0
		SET @result = 0

	RETURN @result
END

GO

