IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_contract_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_contract_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-18
-- Description:	Write contract schedule time from staging table 'stg_contract'
--				to data mart table 'fact_contract'.
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_contract_load] 
@start_date smalldatetime,
@end_date smalldatetime
	
AS
----------------------------------------------------------------------------------
DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

DECLARE @max_date smalldatetime
DECLARE @min_date smalldatetime

--SET @end_date = dateadd(dd,1,@end_date)-- behövs ej sålänge datum i stg ej innheåller klockslag.

SELECT  
	@max_date= max(date),
	@min_date= min(date)
FROM
	Stage.stg_contract
 
SET	@start_date = CASE WHEN @min_date > @start_date THEN @min_date ELSE @start_date END
SET	@end_date	= CASE WHEN @max_date < @end_date	THEN @max_date ELSE @end_date	END

SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,8),@start_date )))
SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,8),@end_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

--SELECT @start_date

-----------------------------------------------------------------------------------
-- Delete rows

DELETE FROM mart.fact_contract
WHERE date_id between @start_date_id AND @end_date_id
	AND business_unit_id = 
	(
		SELECT DISTINCT
			bu.business_unit_id
		FROM 
			Stage.stg_contract c
		INNER JOIN
			mart.dim_business_unit bu
		ON
			c.business_unit_code = bu.business_unit_code
	)

-----------------------------------------------------------------------------------
-- Insert rows

INSERT INTO mart.fact_contract
	(
	date_id, 
	person_id, 
	interval_id, 
	scheduled_contract_time_m, 
	business_unit_id,
	datasource_id
	)
SELECT
	date_id					= dsd.date_id, 
	person_id				= dp.person_id, 
	interval_id				= di.interval_id, 
	scheduled_contract_time_m =f.scheduled_contract_time_m,
	business_unit_id		= dp.business_unit_id,
	datasource_id			= f.datasource_id
FROM 
	Stage.stg_contract f
INNER JOIN	
	mart.dim_person	dp	
ON	
	f.person_code	=	dp.person_code	AND	
	f.date			BETWEEN	valid_from_date	AND valid_to_date
LEFT JOIN	
	mart.dim_date dsd	
ON	
	f.date	= dsd.date_date
LEFT JOIN	
	mart.dim_interval di 
ON	
	f.interval_id = di.interval_id
LEFT JOIN	
	mart.dim_interval dist 
ON 
	f.interval_id = dist.interval_id
WHERE 
	f.date between @start_date and @end_date
ORDER BY 
	dsd.date_id,
	dp.person_id,
	di.interval_id

GO

