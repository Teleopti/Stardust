/* Remove potential duplicates from mart.fact_schedule_deviation */
CREATE TABLE #duplicates_fsd (shift_startdate_local_id int,
				person_code uniqueidentifier,
				interval_id smallint)

insert into #duplicates_fsd
	SELECT a.shift_startdate_local_id, p.person_code, a.interval_id
	FROM (
			SELECT DISTINCT shift_startdate_local_id, person_id, interval_id 
			FROM mart.fact_schedule_deviation
		) a
		inner join mart.dim_person p on a.person_id = p.person_id
	GROUP BY shift_startdate_local_id, p.person_code, interval_id
	HAVING COUNT(*) > 1 


delete fsd
from #duplicates_fsd d
inner join mart.dim_person p on d.person_code = p.person_code
inner join mart.fact_schedule_deviation fsd 
	on d.shift_startdate_local_id = fsd.shift_startdate_local_id and
		p.person_id = fsd.person_id and
		d.interval_id = fsd.interval_id 
where fsd.shift_startdate_local_id not between p.valid_from_date_id_local and p.valid_to_date_id_local
		
drop table #duplicates_fsd


/* Remove potential duplicates from mart.fact_schedule_preference */
CREATE TABLE #duplicates_fsp (date_id int,
				person_code uniqueidentifier,
				scenario_id int)

insert into #duplicates_fsp
	SELECT a.date_id, p.person_code, a.scenario_id
	FROM (
			SELECT DISTINCT date_id, person_id, scenario_id 
			FROM mart.fact_schedule_preference
		) a
		inner join mart.dim_person p on a.person_id = p.person_id
	GROUP BY date_id, p.person_code, scenario_id
	HAVING COUNT(*) > 1 


delete fsp
from #duplicates_fsp d
inner join mart.dim_person p on d.person_code = p.person_code
inner join mart.fact_schedule_preference fsp 
	on d.date_id= fsp.date_id and
		p.person_id = fsp.person_id and
		d.scenario_id = fsp.scenario_id
where fsp.date_id not between p.valid_from_date_id_local and p.valid_to_date_id_local
		
drop table #duplicates_fsp