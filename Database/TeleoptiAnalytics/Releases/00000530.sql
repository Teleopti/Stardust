delete rta.ActualAgentState 
from rta.ActualAgentState as s 
where not exists(
	select * from mart.dim_person 
	where person_code = s.PersonId
)