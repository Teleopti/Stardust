IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_update_unlinked_personids]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_update_unlinked_personids]
GO

-- example: exec [mart].[etl_fact_schedule_update_unlinked_personids] '357,358,551'
CREATE PROCEDURE [mart].[etl_fact_schedule_update_unlinked_personids]
@person_periodids nvarchar(max)
AS
BEGIN
-- select persons with the specific person period ids
select person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local into #person
from mart.dim_person with (nolock) where person_id IN (select id from mart.SplitStringInt(@person_periodids))
-- update the eternity date id with max date id
update #person
set valid_to_date_id_local = (select max(date_id) from mart.dim_date with (nolock)) where valid_to_date_id_local = -2

-- select fact schedule which need to be updated
select distinct shift_startdate_local_id into #schedule
	from mart.fact_schedule f
	inner join #person p
	ON p.person_id = f.person_id
	WHERE p.valid_to_date <= f.shift_starttime or p.valid_from_date > f.shift_starttime or p.to_be_deleted=1
-- update fact schedule with correct person period id
UPDATE mart.fact_schedule
SET [person_id]=p.person_id
FROM mart.fact_schedule f
	INNER JOIN (SELECT person_id, valid_from_date, valid_to_date FROM #person  WHERE to_be_deleted=0)p
		ON f.shift_starttime between p.valid_from_date and p.valid_to_date
		where f.person_id in (select person_id from #person)
		and f.shift_startdate_local_id in 
		(select shift_startdate_local_id from #schedule)

-- select fact schedule day count which need to be updated
select distinct shift_startdate_local_id into #schedule_day
	from mart.fact_schedule_day_count f
	inner join #person p
	ON p.person_id = f.person_id
	WHERE p.valid_to_date <= f.starttime or p.valid_from_date > f.starttime or p.to_be_deleted=1
-- update fact schedule day count with correct person period id
UPDATE mart.fact_schedule_day_count
SET [person_id]=p.person_id
FROM mart.fact_schedule_day_count f
	INNER JOIN (SELECT person_id, valid_from_date, valid_to_date FROM #person WHERE to_be_deleted=0)p
		ON f.starttime between p.valid_from_date and p.valid_to_date
		where f.person_id in (select person_id from #person)
		and f.shift_startdate_local_id in 
		(select shift_startdate_local_id from #schedule_day)

-- select fact schedule deviation which need to be updated
--select distinct shift_startdate_local_id into #schedule_deviation
--			from mart.fact_schedule_deviation f
--			inner join #person p
--			ON p.person_id = f.person_id
--			WHERE p.valid_to_date_id_local <= f.shift_startdate_local_id or p.valid_from_date_id_local > f.shift_startdate_local_id or p.to_be_deleted=1
-- update fact schedule deviation with correct person period id
--UPDATE mart.fact_schedule_deviation
--SET [person_id]=p.person_id
--FROM mart.fact_schedule_deviation f
--	INNER JOIN (SELECT person_id, valid_from_date, valid_to_date, valid_from_date_id_local, valid_to_date_id_local FROM #person WHERE to_be_deleted=0)p
--		ON f.shift_startdate_local_id between p.valid_from_date_id_local and p.valid_to_date_id_local
--		where f.person_id in (select person_id from #person)
--		and f.shift_startdate_local_id in 
--		(select shift_startdate_local_id from #schedule_deviation)


drop table #person, #schedule, #schedule_day --, #schedule_deviation

END
