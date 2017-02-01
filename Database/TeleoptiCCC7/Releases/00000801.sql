UPDATE Readmodel.AgentState
SET 
	FirstName = p.FirstName,
	LastName = p.LastName,
	EmploymentNumber = p.EmploymentNumber
FROM Readmodel.AgentState a
INNER JOIN dbo.Person p
	ON a.PersonId = p.Id