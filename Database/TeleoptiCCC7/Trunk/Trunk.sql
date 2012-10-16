----------------  
--Name: Anders Forsberg
--Date: 2012-10-15
--Desc: Adding possiblity to purge old payroll exports cause it's been asked for
----------------  
if not exists (select 1 from PurgeSetting where [Key] = 'Payroll')
begin
	insert into PurgeSetting values ('Payroll', 20)
end
GO