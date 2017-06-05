IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_simulator_delete_stats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_simulator_delete_stats]
GO

-- =============================================
-- Author:		Jonas & Maria
-- Create date: 2016-04-04
-- Description:	Delete all queue statistics for today
-- =============================================
-- EXEC [mart].[web_intraday_simulator_delete_stats] 'W. Europe Standard Time', '2016-04-04'
CREATE PROCEDURE [mart].[web_intraday_simulator_delete_stats]
@time_zone_code nvarchar(100),
@date smalldatetime

AS
BEGIN
	SET NOCOUNT ON;
            
	DECLARE @time_zone_id as int
	
	SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = @time_zone_code

	DELETE fq 
	FROM
		mart.fact_queue fq
		INNER JOIN mart.bridge_time_zone bz ON fq.date_id = bz.date_id AND fq.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i ON bz.local_interval_id = i.interval_id
	WHERE
		bz.time_zone_id = @time_zone_id
		AND d.date_date = @date
END

GO

