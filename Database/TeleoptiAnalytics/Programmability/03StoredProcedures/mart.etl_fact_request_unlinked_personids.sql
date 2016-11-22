IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_request_unlinked_personids]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_request_unlinked_personids]
GO

-- example: exec [mart].[etl_fact_request_unlinked_personids] '275,276,548,549'
CREATE PROCEDURE [mart].[etl_fact_request_unlinked_personids]
@person_periodids nvarchar(max)
AS
BEGIN

CREATE TABLE #person(person_id int, 
					valid_from_date smalldatetime, 
					valid_to_date smalldatetime, 
					to_be_deleted bit, 
					valid_from_date_id_local int, 
					valid_to_date_id_local int)

CREATE TABLE #request(request_start_date_id int)						
CREATE TABLE #requested_days(request_date_id int)	
	
-- select persons with the specific person period ids
INSERT #person(person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local)
SELECT person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local
FROM mart.dim_person WITH (NOLOCK) 
WHERE person_id IN (SELECT id from mart.SplitStringInt(@person_periodids))

-- update the eternity date id with max date id
UPDATE #person
SET valid_to_date_id_local = (SELECT TOP 1 date_id from mart.dim_date with (nolock) ORDER BY date_id DESC) 
WHERE valid_to_date_id_local = -2

-- select fact_request which need to be updated
INSERT #request(request_start_date_id)
SELECT DISTINCT request_start_date_id
FROM mart.fact_request f
INNER JOIN #person p
ON p.person_id = f.person_id
WHERE p.valid_to_date_id_local < f.request_start_date_id OR p.valid_from_date_id_local > f.request_start_date_id OR p.to_be_deleted=1

-- update fact_request with correct person period id
UPDATE mart.fact_request
SET [person_id]=p.person_id
FROM mart.fact_request f
INNER JOIN (SELECT person_id, valid_from_date, valid_to_date, valid_from_date_id_local, valid_to_date_id_local FROM #person WHERE to_be_deleted=0)p
ON f.request_start_date_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
WHERE f.person_id IN (SELECT person_id FROM #person)
AND f.request_start_date_id IN (SELECT request_start_date_id FROM #request)

---- select fact_requested_days which need to be updated
INSERT #requested_days(request_date_id)
SELECT DISTINCT request_date_id
FROM mart.fact_requested_days f
INNER JOIN #person p
ON p.person_id = f.person_id
WHERE p.valid_to_date_id_local < f.request_date_id OR p.valid_from_date_id_local > f.request_date_id OR p.to_be_deleted=1

-- update fact_requested_days with correct person period id
UPDATE mart.fact_requested_days
SET [person_id]=p.person_id
FROM mart.fact_requested_days f
INNER JOIN (SELECT person_id, valid_from_date, valid_to_date, valid_from_date_id_local, valid_to_date_id_local FROM #person WHERE to_be_deleted=0)p
ON f.request_date_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
WHERE f.person_id IN (SELECT person_id FROM #person)
AND f.request_date_id IN (SELECT request_date_id FROM #requested_days)

drop table #person, #request, #requested_days

END
