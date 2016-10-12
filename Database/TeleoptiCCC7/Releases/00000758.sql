if not exists (select * from sys.columns where name = 'AbsenceRequestExpiredThreshold' and object_id = OBJECT_ID('WorkflowControlSet'))
begin
	alter table WorkflowControlSet add AbsenceRequestExpiredThreshold int default 15
end


CREATE TABLE ReadModel.HistoricalAdherence (
  PersonId uniqueidentifier not null,
  AgentName nvarchar(max),
  Schedules nvarchar(max),
  OutOfAdherences nvarchar(max),
  [Date] datetime
);