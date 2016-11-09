IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_update_unlinked_personids]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_update_unlinked_personids]
GO

-- example: exec [mart].[etl_fact_schedule_update_unlinked_personids] '357,358,551'
CREATE PROCEDURE [mart].[etl_fact_schedule_update_unlinked_personids]
@person_periodids nvarchar(max)
AS
BEGIN

CREATE TABLE #person(person_id int, 
					valid_from_date smalldatetime, 
					valid_to_date smalldatetime, 
					to_be_deleted bit, 
					valid_from_date_id_local int, 
					valid_to_date_id_local int)

CREATE TABLE #schedule(shift_startdate_local_id int)
CREATE TABLE #schedule_day(shift_startdate_local_id int)
CREATE TABLE #schedule_deviation(shift_startdate_local_id int)						

-- select persons with the specific person period ids
INSERT #person(person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local)
SELECT person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local
FROM mart.dim_person WITH (NOLOCK) 
WHERE person_id IN (SELECT id from mart.SplitStringInt(@person_periodids))

-- update the eternity date id with max date id
UPDATE #person
SET valid_to_date_id_local = (SELECT TOP 1 date_id from mart.dim_date with (nolock) ORDER BY date_id DESC) 
WHERE valid_to_date_id_local = -2

INSERT #schedule(shift_startdate_local_id)
SELECT DISTINCT shift_startdate_local_id -- 
FROM mart.fact_schedule f 
INNER JOIN #person p
ON p.person_id = f.person_id
WHERE p.valid_to_date_id_local < f.shift_startdate_local_id OR p.valid_from_date_id_local > f.shift_startdate_local_id OR p.to_be_deleted=1

-- update fact schedule with correct person period id
UPDATE mart.fact_schedule
SET [person_id]=p.person_id
FROM mart.fact_schedule f
	INNER JOIN (SELECT person_id, valid_from_date_id_local, valid_to_date_id_local FROM #person WHERE to_be_deleted=0)p
		ON f.shift_startdate_local_id between p.valid_from_date_id_local and p.valid_to_date_id_local
		WHERE f.person_id in (SELECT person_id FROM #person)
		AND f.shift_startdate_local_id in (SELECT shift_startdate_local_id FROM #schedule)

-- select fact schedule day count which need to be updated
INSERT #schedule_day(shift_startdate_local_id)
SELECT DISTINCT shift_startdate_local_id
FROM mart.fact_schedule_day_count f
INNER JOIN #person p
ON p.person_id = f.person_id
WHERE p.valid_to_date_id_local < f.shift_startdate_local_id or p.valid_from_date_id_local > f.shift_startdate_local_id or p.to_be_deleted=1

-- update fact schedule day count with correct person period id
UPDATE mart.fact_schedule_day_count
SET [person_id]=p.person_id
FROM mart.fact_schedule_day_count f
INNER JOIN (SELECT person_id, valid_from_date_id_local, valid_to_date_id_local FROM #person WHERE to_be_deleted=0)p
ON f.shift_startdate_local_id between p.valid_from_date_id_local and p.valid_to_date_id_local
WHERE f.person_id in (SELECT person_id FROM #person)
and f.shift_startdate_local_id in (SELECT shift_startdate_local_id FROM #schedule_day)

-- select fact schedule deviation which need to be updated
INSERT #schedule_deviation(shift_startdate_local_id)
SELECT DISTINCT shift_startdate_local_id 
FROM mart.fact_schedule_deviation f
INNER JOIN #person p
ON p.person_id = f.person_id
WHERE p.valid_to_date_id_local < f.shift_startdate_local_id OR p.valid_from_date_id_local > f.shift_startdate_local_id OR p.to_be_deleted=1


-- update fact schedule deviation with correct person period id
UPDATE mart.fact_schedule_deviation
SET [person_id]=p.person_id
FROM mart.fact_schedule_deviation f
INNER JOIN (SELECT person_id, valid_from_date, valid_to_date, valid_from_date_id_local, valid_to_date_id_local FROM #person WHERE to_be_deleted=0)p
ON f.shift_startdate_local_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
WHERE f.person_id IN (SELECT person_id FROM #person)
AND f.shift_startdate_local_id IN (SELECT shift_startdate_local_id FROM #schedule_deviation)


drop table #person, #schedule, #schedule_day, #schedule_deviation

END
