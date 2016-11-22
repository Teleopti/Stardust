IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_hourly_availability_update_unlinked_personids]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_hourly_availability_update_unlinked_personids]
GO

-- example: exec [mart].[etl_fact_hourly_availability_update_unlinked_personids] '275,276,548,549'
CREATE PROCEDURE [mart].[etl_fact_hourly_availability_update_unlinked_personids]
@person_periodids nvarchar(max)
AS
BEGIN

CREATE TABLE #person(person_id int, 
					valid_from_date smalldatetime, 
					valid_to_date smalldatetime, 
					to_be_deleted bit, 
					valid_from_date_id_local int, 
					valid_to_date_id_local int)

CREATE TABLE #hourly_availability(date_id int)
	
-- select persons with the specific person period ids
INSERT #person(person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local)
SELECT person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local
FROM mart.dim_person WITH (NOLOCK) 
WHERE person_id IN (SELECT id from mart.SplitStringInt(@person_periodids))

-- update the eternity date id with max date id
UPDATE #person
SET valid_to_date_id_local = (SELECT TOP 1 date_id from mart.dim_date with (nolock) ORDER BY date_id DESC) 
WHERE valid_to_date_id_local = -2

INSERT #hourly_availability(date_id)
SELECT DISTINCT date_id 
FROM mart.fact_hourly_availability f 
INNER JOIN #person p
ON p.person_id = f.person_id
WHERE p.valid_to_date_id_local < f.date_id OR p.valid_from_date_id_local > f.date_id OR p.to_be_deleted=1

-- update fact_hourly_availability with correct person period id
UPDATE mart.fact_hourly_availability
SET [person_id]=p.person_id
FROM mart.fact_hourly_availability f
	INNER JOIN (SELECT person_id, valid_from_date_id_local, valid_to_date_id_local FROM #person WHERE to_be_deleted=0)p
		ON f.date_id between p.valid_from_date_id_local and p.valid_to_date_id_local
		WHERE f.person_id in (SELECT person_id FROM #person)
		AND f.date_id in (SELECT date_id FROM #hourly_availability)

drop table #person, #hourly_availability

END
