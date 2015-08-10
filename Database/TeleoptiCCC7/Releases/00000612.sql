
Update dbo.PersonPeriod 
Set PersonPeriod.EndDate = Person.TerminalDate
FROM PersonPeriod 
	INNER JOIN Person on Person.Id = PersonPeriod.Parent
	Where PersonPeriod.EndDate is null and TerminalDate is not null
		
GO