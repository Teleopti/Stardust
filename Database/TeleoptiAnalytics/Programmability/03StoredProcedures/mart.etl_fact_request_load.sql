IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_fact_request_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_request_load]
GO

--exec [mart].[etl_fact_request_load] '2012-02-08','2012-02-11'
CREATE PROCEDURE [mart].[etl_fact_request_load]
@start_date smalldatetime,
@end_date smalldatetime
	
AS
BEGIN
SET NOCOUNT ON
DECLARE @business_unit_id int
DECLARE @start_date_id int
DECLARE @end_date_id int

------------
--Init
------------
--There should not be any timevalue on the interface values, since that will mess things up around midnight!
--Consider: DECLARE @end_date smalldatetime;SET @end_date = '2006-01-31 23:59:30';SELECT @end_date
SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

--select @start_date,@start_date_id,@end_date,@end_date_id
--------
--Todo: Get @business_unit_code on interface instead!
--------
--for now get business unit id via stg-table
SELECT TOP 1
	@business_unit_id = bu.business_unit_id
FROM mart.dim_business_unit bu
INNER JOIN stage.stg_request stg
	ON bu.business_unit_code = stg.business_unit_code

SET NOCOUNT OFF
--Remove old data
DELETE FROM mart.fact_request
WHERE date_id between @start_date_id AND @end_date_id
	AND business_unit_id = @business_unit_id

INSERT INTO mart.fact_request
SELECT
date_id				= dUTC.date_id,
interval_id			= stg.interval_from_id,
person_id			= dp.person_id,
request_type_id		= rt.request_type_id,
request_status_id	= rs.request_status_id,
date_from_local		= dLocal.date_id,
request_start_date_count = Sum(stg.request_start_date_count),
request_day_count = Sum(stg.request_day_count),
business_unit_id	= Min(bu.business_unit_id),
datasource_id		= Min(stg.datasource_id),
insert_date			= Min(stg.insert_date),
update_date			= Min(stg.update_date),
datasource_update_date=Min(stg.datasource_update_date)

FROM stage.stg_request stg
INNER JOIN mart.dim_date dUTC
	ON dUTC.date_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), stg.date_from, 112))
INNER JOIN mart.dim_date dLocal
	ON dLocal.date_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), stg.date_from_local, 112))	
INNER JOIN mart.dim_business_unit bu
	ON bu.business_unit_code = stg.business_unit_code
INNER JOIN
	mart.dim_person		dp
ON
	stg.person_code		=			dp.person_code
	AND	stg.date_from		BETWEEN		dp.valid_from_date	AND dp.valid_to_date  --Is person valid in this range
	AND --trim
		(
				(stg.date_from	>= dp.valid_from_date)

			AND
				(stg.date_from < dp.valid_to_date)
		)
INNER JOIN mart.dim_request_type rt
	ON rt.request_type_id = stg.request_type_code
INNER JOIN mart.dim_request_status rs
	ON rs.request_status_id = stg.request_status_code
WHERE bu.business_unit_id = @business_unit_id
AND dUTC.date_id BETWEEN @start_date_id AND @end_date_id
Group by dUTC.date_id,
stg.interval_from_id,
dp.person_id,
rt.request_type_id,
rs.request_status_id,
dLocal.date_id
END
GO
