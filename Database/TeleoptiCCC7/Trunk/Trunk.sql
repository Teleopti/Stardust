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
----------------  
--Name: Xianwei Shen
--Date: 2012-10-15
--Desc: #21058 Purge OptionalColumnValue when its parent is deleted
----------------  
DELETE FROM dbo.OptionalColumnValue 
WHERE Parent in (
	SELECT Id FROM dbo.OptionalColumn WHERE IsDeleted = 1
)
GO