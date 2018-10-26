create index IX_Note_Person_NoteDate_Scenario_BusinessUnit
on dbo.Note (Person, NoteDate, Scenario, BusinessUnit)

go

create index IX_PublicNote_Person_NoteDate_Scenario_BusinessUnit
on dbo.PublicNote (Person, NoteDate, Scenario, BusinessUnit)