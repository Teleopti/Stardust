if not exists (select * from sys.columns where name = 'BrokenBusinessRules' and object_id = OBJECT_ID('PersonRequest'))
begin
	alter table PersonRequest add BrokenBusinessRules int
end