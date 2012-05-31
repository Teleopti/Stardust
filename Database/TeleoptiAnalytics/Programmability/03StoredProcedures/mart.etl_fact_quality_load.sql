IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_quality_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_quality_load]
GO


-- =============================================
-- Author:		DavidJ
-- Create date: 2012-05-31
-- Description:	Loads fact_quality.
-- =============================================
--exec [mart].[etl_fact_quality_load] '2009-01-01' ,'2012-03-01',4

CREATE PROCEDURE [mart].[etl_fact_quality_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@datasource_id int
	
AS

--------------------------------------------------------------------------
--If we get All = -2 loop existing log objects and call this SP in a cursor for each log object
--------------------------------------------------------------------------
IF @datasource_id = -2 --All
BEGIN
	DECLARE DataSouceCursor CURSOR FOR
	SELECT datasource_id FROM mart.sys_datasource WHERE datasource_id NOT IN (-1,1) AND time_zone_id IS NOT NULL AND inactive = 0 
	OPEN DataSouceCursor

	FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC [mart].[etl_fact_quality_load] @start_date, @end_date, @datasource_id
		FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END

ELSE  --Single datasource_id
BEGIN
	DECLARE @start_date_id int
	DECLARE @end_date_id int

	------------
	--Init
	------------
	--There should not be any timevalue on the interface values, since that will mess things up around midnight!
	--Consider:
	--DECLARE @end_date smalldatetime;SET @end_date = '2006-01-31 23:59:30';SELECT @end_date
	SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
	SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))

	SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
	SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

	----------------
	--Remove old data
	----------------
	DELETE mart.fact_quality
	WHERE datasource_id = @datasource_id
	AND date_id BETWEEN @start_date_id AND @end_date_id

	----------------
	--insert new ones
	----------------
	INSERT INTO mart.fact_quality
	SELECT
	date_id					= dLocal.date_id,
	acd_login_id			= ISNULL(a.acd_login_id,-1),
	evaluation_id			= agg.evaluation_id,
	quality_quest_id		= q.quality_quest_id,
	quality_quest_type_id	= qt.quality_quest_type_id,
	score					= agg.score,
	datasource_id			= @datasource_id
	FROM dbo.quality_logg agg
	INNER JOIN mart.dim_date dLocal
		ON dLocal.date_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), agg.date_from, 112))	
		AND dLocal.date_id BETWEEN @start_date_id AND @end_date_id
	LEFT JOIN mart.dim_acd_login a 
		ON a.acd_login_agg_id = agg.agent_id
		AND a.datasource_id = @datasource_id
	INNER JOIN mart.dim_quality_quest q
		ON q.quality_quest_agg_id = agg.quality_id
	INNER JOIN mart.dim_quality_quest_type qt
		ON q.quality_quest_type_id = qt.quality_quest_type_id
END

GO

--SELECT agg.* FROM dbo.quality_logg agg
--select * from Demoreg_TeleoptiCCCAgg.dbo.queue_logg
