-- Remove records that are connected to an incorrect person periods where that day is also connected to the correct person period

create table #persons_to_correct(
	person_code uniqueidentifier, 
	date_id int, 
	scenario_id int)

insert into #persons_to_correct
select p.person_code, fsdc.shift_startdate_local_id, fsdc.scenario_id
from mart.fact_schedule_day_count fsdc
inner join mart.dim_person p on fsdc.person_id = p.person_id
group by p.person_code, fsdc.shift_startdate_local_id, fsdc.scenario_id 
having count(1) > 1


delete fsdc
from #persons_to_correct ptc
inner join mart.dim_person p 
	on ptc.person_code = p.person_code
inner join mart.fact_schedule_day_count fsdc 
	on p.person_id = fsdc.person_id
		and ptc.date_id = fsdc.shift_startdate_local_id
		and ptc.scenario_id = fsdc.scenario_id
where not fsdc.shift_startdate_local_id between p.valid_from_date_id_local and p.valid_to_date_id_local

drop table #persons_to_correct