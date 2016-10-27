IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_deviation_unlinked_dates]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_deviation_unlinked_dates]
GO

-- example: exec [mart].[etl_fact_schedule_deviation_unlinked_dates] '357,358,551'
CREATE PROCEDURE [mart].[etl_fact_schedule_deviation_unlinked_dates]
@person_periodids nvarchar(max)
AS
BEGIN

-- select persons with the specific person period ids
select person_id, valid_from_date, valid_to_date, to_be_deleted, valid_from_date_id_local, valid_to_date_id_local into #person
from mart.dim_person with (nolock) where person_id IN (select id from mart.SplitStringInt(@person_periodids))
-- update the eternity date id with max date id
update #person
set valid_to_date_id_local = (select max(date_id) from mart.dim_date with (nolock)) where valid_to_date_id_local = -2

-- select fact schedule deviation which need to be updated
select distinct date_date
	from mart.fact_schedule_deviation f with (nolock)
		inner join #person p
		ON p.person_id = f.person_id
		inner join mart.dim_date d
		ON f.shift_startdate_local_id = d.date_id
		WHERE p.valid_to_date_id_local < f.shift_startdate_local_id  or p.valid_from_date_id_local > f.shift_startdate_local_id or p.to_be_deleted=1 order by date_date

END