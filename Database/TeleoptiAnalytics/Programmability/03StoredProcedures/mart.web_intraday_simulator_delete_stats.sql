IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_simulator_delete_stats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_simulator_delete_stats]
GO

-- =============================================
-- Author:		Jonas & Maria
-- Create date: 2016-04-04
-- Description:	Delete all queue statistics for today
-- =============================================
-- EXEC [mart].[web_intraday_simulator_delete_stats] '2017-08-24', 16, '2017-08-26', 16
CREATE PROCEDURE [mart].[web_intraday_simulator_delete_stats]
@from_date smalldatetime,
@from_interval_id int,
@to_date smalldatetime,
@to_interval_id int

AS
BEGIN
	SET NOCOUNT ON;

	DELETE fq 
	FROM
		mart.fact_queue fq
		INNER JOIN mart.dim_date d ON fq.date_id = d.date_id
	WHERE
		(
			(@from_date <> @to_date)
			AND
			(
				(d.date_date = @from_date AND fq.interval_id >= @from_interval_id)
				OR
				(d.date_date = @to_date AND fq.interval_id < @to_interval_id)
			)
		)
		OR
		(
			(@from_date = @to_date)
			AND
			(d.date_date = @from_date AND (fq.interval_id >= @from_interval_id AND fq.interval_id < @to_interval_id))
		)
END

GO

