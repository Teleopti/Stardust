-- Remove records that are connected to an incorrect person periods where that day is also connected to the correct person period

create table #persons_to_correct(
	person_code uniqueidentifier, 
	date_id int, 
	scenario_id int)

insert into #persons_to_correct
select p.person_code, fha.date_id, fha.scenario_id
from mart.fact_hourly_availability fha
inner join mart.dim_person p on fha.person_id = p.person_id
group by p.person_code, fha.date_id, fha.scenario_id 
having count(1) > 1


delete fha
from #persons_to_correct ptc
inner join mart.dim_person p 
	on ptc.person_code = p.person_code
inner join mart.fact_hourly_availability fha 
	on p.person_id = fha.person_id
		and ptc.date_id = fha.date_id
		and ptc.scenario_id = fha.scenario_id
where not fha.date_id between p.valid_from_date_id_local and p.valid_to_date_id_local

drop table #persons_to_correct