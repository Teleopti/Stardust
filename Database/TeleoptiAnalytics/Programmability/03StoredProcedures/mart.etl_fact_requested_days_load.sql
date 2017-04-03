IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_fact_requested_days_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_requested_days_load]
GO

--exec [mart].[etl_fact_requested_days_load] '2012-02-08','2012-02-11'
CREATE PROCEDURE [mart].[etl_fact_requested_days_load]
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier	
	
AS
BEGIN
SET NOCOUNT ON
DECLARE @business_unit_id int
DECLARE @start_date_id int
DECLARE @end_date_id int

CREATE TABLE #request (request_code uniqueidentifier)

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
----------------
--Todo: Get @business_unit_code on interface instead!
----------------
--for now get business unit id via stg-table
/*SELECT TOP 1
	@business_unit_id = bu.business_unit_id
FROM mart.dim_business_unit bu
INNER JOIN stage.stg_request stg
	ON bu.business_unit_code = stg.business_unit_code*/

SET NOCOUNT OFF

/*Get business unit id*/
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

----------------
--Remove old data
----------------
DELETE mart.fact_requested_days
WHERE business_unit_id = @business_unit_id
AND request_date_id BETWEEN @start_date_id AND @end_date_id

----------------
--insert new ones
----------------
INSERT INTO mart.fact_requested_days
SELECT 
request_code		= stg.request_code,
person_id			= ISNULL(dp.person_id,-1),
request_date_id		= dLocal.date_id,
request_type_id		= rt.request_type_id,
request_status_id	= rs.request_status_id,
request_day_count	= 1, --add one day per day
business_unit_id	= bu.business_unit_id,
datasource_id		= stg.datasource_id,
insert_date			= stg.insert_date,
update_date			= stg.update_date,
datasource_update_date=stg.datasource_update_date,
absence_id			= isnull(ab.absence_id,-1)
FROM stage.stg_request stg
INNER JOIN mart.dim_date dLocal
	ON dLocal.date_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), stg.request_date, 112))	
INNER JOIN mart.dim_business_unit bu
	ON bu.business_unit_code = stg.business_unit_code
LEFT JOIN mart.dim_person dp WITH (NOLOCK)
ON
	stg.person_code	= dp.person_code
	AND	stg.request_date BETWEEN dp.valid_from_date AND dp.valid_to_date  --Is person valid in this range
	AND --trim
		(
				(stg.request_date	>= dp.valid_from_date)

			AND
				(stg.request_date < dp.valid_to_date)
		)
INNER JOIN mart.dim_request_type rt
	ON rt.request_type_id = stg.request_type_code
INNER JOIN mart.dim_request_status rs
	ON rs.request_status_id = stg.request_status_code
LEFT JOIN
	mart.dim_absence	ab
ON
	stg.absence_code = ab.absence_code
WHERE stg.request_date BETWEEN @start_date AND @end_date
	AND dp.to_be_deleted = 0

----------------
--updating request statuses
----------------
UPDATE mart.fact_requested_days
SET
 request_status_id = stg.request_status_code
FROM stage.stg_request stg
INNER JOIN mart.fact_requested_days f
 ON stg.request_code = f.request_code
INNER JOIN mart.dim_request_status rs
 ON rs.request_status_id = stg.request_status_code

END
GO
