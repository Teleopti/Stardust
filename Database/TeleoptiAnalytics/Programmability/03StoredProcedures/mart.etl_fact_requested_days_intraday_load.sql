IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_fact_requested_days_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_requested_days_intraday_load]
GO

--exec [mart].[etl_fact_requested_days_intraday_load] '27766456-9DD5-45F0-911E-9D430116B318'
CREATE PROCEDURE [mart].[etl_fact_requested_days_intraday_load]
@business_unit_code uniqueidentifier	
	
AS
BEGIN
SET NOCOUNT ON

--Delete all request in stage 
DELETE f
FROM stage.stg_request stg
INNER JOIN mart.fact_requested_days f
on f.request_code = stg.request_code

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
LEFT JOIN mart.dim_person dp
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
WHERE dp.to_be_deleted = 0

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
