create table #duplicates_to_remove(	
	group_page_code uniqueidentifier, 
	group_name nvarchar(1024), 
	business_unit_code uniqueidentifier,
	group_page_id int)

insert into #duplicates_to_remove
select group_page_code, group_name, business_unit_code, max(group_page_id) 
from mart.dim_group_page 
group by group_page_code, group_name, business_unit_code
having COUNT(*) > 1

delete b 
from mart.bridge_group_page_person b
inner join #duplicates_to_remove d 
	on b.group_page_id = d.group_page_id

delete gp 
from mart.dim_group_page gp
inner join #duplicates_to_remove d 
	on gp.group_page_id = d.group_page_id

drop table #duplicates_to_remove