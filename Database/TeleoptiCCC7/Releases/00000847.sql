UPDATE Readmodel.AgentState
SET 
	IsDeleted = 1
FROM Readmodel.AgentState a
INNER JOIN dbo.Person p
	ON a.PersonId = p.Id
WHERE p.IsDeleted = 1 