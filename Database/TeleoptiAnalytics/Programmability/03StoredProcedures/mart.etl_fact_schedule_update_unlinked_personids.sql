IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_update_unlinked_personids]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_update_unlinked_personids]
GO

-- example: exec [mart].[etl_fact_schedule_update_unlinked_personids] '357,358,551'
CREATE PROCEDURE [mart].[etl_fact_schedule_update_unlinked_personids]
@person_periodids nvarchar(max)
AS
BEGIN

DECLARE @person TABLE
(
  person_id int primary key, 
  valid_from_date smalldatetime, 
  valid_to_date smalldatetime, 
  to_be_deleted bit, 
  valid_from_date_id_local int, 
  valid_to_date_id_local int,
  valid_to_date_id int
)

-- select persons with the specific person period ids
INSERT @person(person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local, valid_to_date_id)
SELECT person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local, valid_to_date_id
FROM mart.dim_person WITH (NOLOCK) 
WHERE person_id IN (SELECT id from mart.SplitStringInt(@person_periodids))

-- update the eternity date id with max date id
UPDATE @person
SET valid_to_date_id_local = (SELECT TOP 1 date_id from mart.dim_date with (nolock) ORDER BY date_id DESC) 
WHERE valid_to_date_id = -2

-- update fact schedule with correct person period id
UPDATE fs
SET fs.[person_id]=newpp.person_id
FROM mart.fact_schedule fs
INNER JOIN @person newpp ON 
	fs.person_id in (select person_id from @person)
	AND fs.shift_startdate_local_id BETWEEN newpp.valid_from_date_id_local AND newpp.valid_to_date_id_local
	AND newpp.to_be_deleted=0
INNER JOIN @person oldpp ON oldpp.person_id = fs.person_id
WHERE oldpp.valid_to_date_id_local < fs.shift_startdate_local_id 
	OR oldpp.valid_from_date_id_local > fs.shift_startdate_local_id 
	OR oldpp.to_be_deleted=1

-- update fact schedule day count with correct person period id
UPDATE fsdc
SET fsdc.[person_id]=newpp.person_id
FROM mart.fact_schedule_day_count fsdc
INNER JOIN @person newpp ON 
	fsdc.person_id in (select person_id from @person)
	AND fsdc.shift_startdate_local_id BETWEEN newpp.valid_from_date_id_local AND newpp.valid_to_date_id_local 
	AND newpp.to_be_deleted=0
INNER JOIN @person oldpp ON oldpp.person_id = fsdc.person_id
WHERE oldpp.valid_to_date_id_local < fsdc.shift_startdate_local_id 
	OR oldpp.valid_from_date_id_local > fsdc.shift_startdate_local_id 
	OR oldpp.to_be_deleted=1

-- update fact schedule deviation with correct person period id
UPDATE fsd
SET fsd.[person_id]=newpp.person_id
FROM mart.fact_schedule_deviation fsd
INNER JOIN @person newpp ON 
	fsd.person_id in (select person_id from @person)
	AND fsd.shift_startdate_local_id BETWEEN newpp.valid_from_date_id_local AND newpp.valid_to_date_id_local 
	AND newpp.to_be_deleted=0
INNER JOIN @person oldpp ON oldpp.person_id = fsd.person_id
WHERE oldpp.valid_to_date_id_local < fsd.shift_startdate_local_id 
	OR oldpp.valid_from_date_id_local > fsd.shift_startdate_local_id 
	OR oldpp.to_be_deleted=1

END
