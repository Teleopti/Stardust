DECLARE @persons VARCHAR(MAX)
SELECT @persons = COALESCE(@persons+', ','') + CAST(PersonId AS varchar(50)) FROM (SELECT DISTINCT PersonId,TeamId FROM [ReadModel].[FindPerson]) AS temp GROUP BY PersonId HAVING COUNT(PersonId)>1 ORDER BY personid
EXEC Readmodel.UpdateFindPerson @persons = @persons
GO