if not exists (select * from sys.columns where name = 'BrokenBusinessRule' and object_id = OBJECT_ID('PersonRequest'))
begin
	alter table PersonRequest add BrokenBusinessRule int
end