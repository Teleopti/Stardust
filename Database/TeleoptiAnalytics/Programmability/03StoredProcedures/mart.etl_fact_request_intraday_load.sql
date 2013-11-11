IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_fact_request_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_request_intraday_load]
GO

--exec [mart].[etl_fact_request_load] '2012-02-08','2012-02-11'
CREATE PROCEDURE [mart].[etl_fact_request_intraday_load]
@business_unit_code uniqueidentifier
	
AS
BEGIN

----------------
--Remove old data
----------------
DELETE fact
FROM stage.stg_request stg
INNER JOIN mart.fact_request fact
	ON stg.request_code = fact.request_code
INNER JOIN mart.dim_date dLocal
	ON dLocal.date_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), stg.request_date, 112))	
	AND stg.request_start_date_count = 1  --1 marks that this is the startday of a request
INNER JOIN mart.dim_person dp
ON
	stg.person_code	= dp.person_code
	AND	stg.request_date BETWEEN dp.valid_from_date AND dp.valid_to_date  --Is person valid in this range
	AND --trim
		(
				(stg.request_date	>= dp.valid_from_date)

			AND
				(stg.request_date <= dp.valid_to_date)
		)

----------------
--insert new ones
----------------
INSERT INTO mart.fact_request
SELECT
request_code		= stg.request_code,
person_id			= ISNULL(dp.person_id,-1),
request_start_date_id = dLocal.date_id,
application_datetime= stg.application_datetime,
request_startdate	= stg.request_startdate,
request_enddate		= stg.request_enddate,
request_type_id		= rt.request_type_id,
request_status_id	= rs.request_status_id,
request_day_count	= -1,
request_start_date_count= stg.request_start_date_count,
business_unit_id	= bu.business_unit_id,
datasource_id		= stg.datasource_id,
insert_date			= stg.insert_date,
update_date			= stg.update_date,
datasource_update_date=stg.datasource_update_date,
absence_id			= isnull(ab.absence_id,-1),
request_starttime	= ISNULL(stg.request_starttime,'1900-01-01 00:00:00') ,
request_endtime		= ISNULL(stg.request_endtime, '1900-01-01 00:00:00'),
requested_time_m	= ISNULL(DATEDIFF(minute,stg.request_starttime, stg.request_endtime), 0)
FROM stage.stg_request stg
INNER JOIN mart.dim_date dLocal
	ON dLocal.date_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), stg.request_date, 112))	
	AND stg.request_start_date_count = 1  --1 marks that this is the startday of a request
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
	AND dp.to_be_deleted = 0
INNER JOIN mart.dim_request_type rt
	ON rt.request_type_id = stg.request_type_code
INNER JOIN mart.dim_request_status rs
	ON rs.request_status_id = stg.request_status_code
LEFT JOIN
	mart.dim_absence	ab
ON
	stg.absence_code = ab.absence_code
	

--update the count
update mart.fact_request
set request_day_count = temp.Numday
from 
(select stg.request_code, SUM(stg.request_day_count) as Numday
from stage.stg_request stg
inner join mart.fact_request f
on f.request_code = stg.request_code
group by stg.request_code
) temp
inner join mart.fact_request f
on temp.request_code = f.request_code

--set requested time for shift trades to 0
UPDATE mart.fact_request
SET requested_time_m=0
FROM stage.stg_request s
WHERE s.request_code = mart.fact_request.request_code
AND mart.fact_request.request_type_id=2



END
GO
