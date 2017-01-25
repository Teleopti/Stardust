----------------  
--Name: Yanyi
--Date: 2017-01-25
--Desc: Fix the duplicate absence layer problem as described in BUG #42526
----------------  
SET NOCOUNT ON
	
DECLARE @TO_REMOVE_PERSON_ABSENCE TABLE(
	ID uniqueidentifier NOT NULL
)

INSERT INTO @TO_REMOVE_PERSON_ABSENCE 
SELECT ID FROM (
	SELECT Row_number() OVER (PARTITION BY pa.Person, pa.PayLoad, pa.Minimum, pa.Maximum, pa.Scenario ORDER BY pa.updatedOn DESC) AS row#, pa.*  
	FROM dbo.personabsence pa 
	INNER JOIN (SELECT Person, PayLoad, Minimum, Maximum, Scenario FROM dbo.PersonAbsence GROUP BY Person, PayLoad, Minimum, Maximum, Scenario HAVING count(*) > 1) t 
	ON pa.Person = t.Person and pa.PayLoad = t.PayLoad and pa.maximum = t.maximum and pa.minimum = t.minimum AND pa.Scenario = t.Scenario
	) t2 
WHERE t2.[row#] > 1

DELETE pa   
FROM dbo.PersonAbsence pa INNER JOIN @TO_REMOVE_PERSON_ABSENCE rpa ON pa.Id = rpa.Id 

DELETE aud 
FROM Auditing.PersonAbsence_AUD aud INNER JOIN @TO_REMOVE_PERSON_ABSENCE rpa ON aud.Id = rpa.Id AND aud.REVTYPE = 0
	
	
GO
