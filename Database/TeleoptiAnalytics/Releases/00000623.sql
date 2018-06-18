--Fix incorrectly pseudonymized agents in mart #76444
update mart.dim_person set person_name = g.person_name, first_name = g.first_name, last_name = g.last_name, employment_number = g.employment_number, email = g.email, note = g.note, windows_username = g.windows_username
from mart.dim_person f inner join (
select a.person_code, min(c.person_name) as person_name, min(c.first_name) as first_name, min(c.last_name) as last_name, min(c.employment_number) as employment_number, min(c.email) as email, min(c.note) as note, min(c.windows_username) as windows_username
from mart.dim_person a
inner join mart.dim_person c on a.person_code = c.person_code and c.first_name <> 'Pseudo'
where a.person_name = 'Pseudo' and a.first_name = 'Pseudo' and a.last_name = 'Pseudo' and a.employment_number = '' and a.email = '' and a.note = '' and a.windows_username = ''
and exists (select 1 from mart.dim_person b where a.person_code = b.person_code and b.first_name <> 'Pseudo')
group by a.person_code--, min(c.first_name)
) g on g.person_code = f.person_code
where f.first_name = 'Pseudo'
