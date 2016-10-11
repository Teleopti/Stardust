if not exists (select * from sys.columns where name = 'AbsenceRequestExpiredThreshold' and object_id = OBJECT_ID('WorkflowControlSet'))
begin
	alter table WorkflowControlSet add AbsenceRequestExpiredThreshold int default 15
	update WorkflowControlSet set AbsenceRequestExpiredThreshold = 15
end