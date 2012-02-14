/* 
Trunk initiated: 
2010-03-14 
15:31
By: TOPTINET\andersf 
On ANDERSFNC8430 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-03-15
--Desc: Clean up of PersonAbsence, adding backup table for deleted rows
----------------
-- 1) Duplicates will be removed
-- This issue concernce both Intraday and fullday abscens

-- Customer specific, not executed by default
-- 2) FullDay covering a IntraDay Absence will be removed
-- Splitt multiple Fullday into single day absence, then keep fullday only if a Intraday absence does not exists

--Create a backup table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PersonAbsence_Backup]') AND type in (N'U'))
	CREATE TABLE dbo.PersonAbsence_Backup (
		[Id] [uniqueidentifier] NOT NULL,
		[Version] [int] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[UpdatedBy] [uniqueidentifier] NOT NULL,
		[CreatedOn] [datetime] NOT NULL,
		[UpdatedOn] [datetime] NOT NULL,
		[LastChange] [datetime] NULL,
		[Person] [uniqueidentifier] NOT NULL,
		[Scenario] [uniqueidentifier] NOT NULL,
		[PayLoad] [uniqueidentifier] NOT NULL,
		[Minimum] [datetime] NOT NULL,
		[Maximum] [datetime] NOT NULL,
		[BusinessUnit] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_PersonAbsence_Backup] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	) ON [PRIMARY]


--Ids to be saved and deleted
CREATE TABLE #DeleteAbsence (Id [uniqueidentifier] NOT NULL)

--We will fetch all duplicates from PersonAbsence, but filter out one (rowNumber > 1)
INSERT INTO #DeleteAbsence 
	SELECT Id FROM
	(
		SELECT
			pa.Id,
			pa.Person,
			pa.Scenario,
			pa.PayLoad,
			pa.Minimum,
			pa.Maximum,
			ROW_NUMBER() OVER(
				PARTITION BY 
					pa.Person,
					pa.Scenario,
					pa.PayLoad,
					pa.Minimum,
					pa.Maximum
				ORDER BY
					pa.Person,
					pa.Scenario,
					pa.PayLoad,
					pa.Minimum,
					pa.Maximum
			) rowNumber
		FROM
		(
		select person,Scenario,PayLoad,Minimum,Maximum
		from dbo.PersonAbsence
		group by person,Scenario,PayLoad,Minimum,Maximum
		having count(person) > 1
		) duplicate
		inner join dbo.PersonAbsence pa
			ON duplicate.Person		= pa.Person
			AND duplicate.Scenario	= pa.Scenario
			AND duplicate.PayLoad	= pa.PayLoad
			AND duplicate.Minimum	= pa.Minimum
			AND duplicate.Maximum	= pa.Maximum
	) remove
	WHERE rowNumber > 1

--Save duplicates
INSERT INTO dbo.PersonAbsence_Backup
SELECT pa.*
FROM PersonAbsence pa
INNER JOIN #DeleteAbsence del
ON pa.Id = del.Id

-- Delete duplicates
DELETE pa
FROM dbo.PersonAbsence pa
INNER JOIN #DeleteAbsence del
ON pa.Id = del.Id

--Part 2) starts here. NOTE: Customer specific!!
/*
--Create a working table for FullDay absence rows to be manipulated
CREATE TABLE #PersonAbsence_Cleanup(
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[LastChange] [datetime] NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[PayLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonAbsence_Cleanup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

--Get all FullDay Abscens having intra day abscenses under it
INSERT INTO #PersonAbsence_Cleanup
SELECT DISTINCT FullDayAbscens.*
FROM
(
	SELECT person,Scenario,Minimum,Maximum
	FROM PersonAbsence
	WHERE DATEDIFF(ss,Minimum,Maximum) < 60*60*24 --less the a full day
	AND DATEDIFF(ss,Minimum,Maximum) % (60*60*24) <> 0 --not a full day
) IntraDayAbscens
INNER JOIN PersonAbsence FullDayAbscens
	ON IntraDayAbscens.Person		= FullDayAbscens.Person
	AND IntraDayAbscens.Scenario	= FullDayAbscens.Scenario
WHERE DATEDIFF(ss,FullDayAbscens.Minimum,FullDayAbscens.Maximum) % (60*60*24) = 0
AND FullDayAbscens.Minimum < IntraDayAbscens.Minimum
AND FullDayAbscens.Maximum > IntraDayAbscens.Maximum

--Save Fullday from current storage into Backup table
INSERT INTO PersonAbsence_Backup
SELECT pa.*
FROM PersonAbsence pa
INNER JOIN #PersonAbsence_Cleanup cl
ON pa.Id = cl.Id

--Delete Fullday from current storage
DELETE pa
FROM PersonAbsence pa
INNER JOIN #PersonAbsence_Cleanup del
ON pa.Id = del.Id

--Execute a cursor to splitt all multi day FullDay abscens into one day Absence only
CREATE TABLE #PersonAbsence(
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[LastChange] [datetime] NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[PayLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonAbsence_temp] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

SET NOCOUNT ON
DECLARE @Counter int
SET @Counter = 0

DECLARE @Id UNIQUEIDENTIFIER -- Original Id for the full day abscens
DECLARE @NumberOfDay INT	 -- Number of days for this Absence

DECLARE CheckFullDay CURSOR FOR  
SELECT	
	Id,
	DATEDIFF(DD,Minimum,Maximum) as DayCount
FROM #PersonAbsence_Cleanup
ORDER BY Person,minimum

OPEN CheckFullDay   
FETCH NEXT FROM CheckFullDay INTO @Id, @NumberOfDay   

WHILE @@FETCH_STATUS = 0
BEGIN   

	WHILE @NumberOfDay > @Counter
		BEGIN
		
			INSERT INTO #PersonAbsence
			SELECT
				newid(),
				[Version],
				[CreatedBy],
				[UpdatedBy],
				[CreatedOn],
				[UpdatedOn],
				[LastChange],
				[Person],
				[Scenario],
				[PayLoad],
				DATEADD(DD,@Counter,Minimum),
				DATEADD(DD,@Counter+1,Minimum),
				[BusinessUnit]
			FROM #PersonAbsence_Cleanup
			WHERE Id = @Id
						
			SET @Counter = @Counter + 1

			IF @NumberOfDay <= @Counter
				BREAK
			ELSE
				CONTINUE
		END
 
		SET @Counter = 0
	
	FETCH NEXT FROM CheckFullDay INTO @Id, @NumberOfDay 
END   

CLOSE CheckFullDay   
DEALLOCATE CheckFullDay 
SET NOCOUNT OFF

--Delete all one Day Fullday Absence that still have Intra day under it
DELETE FROM #PersonAbsence
WHERE Id IN
(
	SELECT FullDayAbscens.Id
	FROM
	(
		SELECT person,Scenario,Minimum,Maximum
		FROM PersonAbsence
		WHERE DATEDIFF(ss,Minimum,Maximum) < 60*60*24 --Less then a full day
		AND DATEDIFF(ss,Minimum,Maximum) % (60*60*24) <> 0 --not a full day
	) IntraDayAbscens
	INNER JOIN #PersonAbsence FullDayAbscens
		ON IntraDayAbscens.Person		= FullDayAbscens.Person
		AND IntraDayAbscens.Scenario	= FullDayAbscens.Scenario
	WHERE DATEDIFF(ss,FullDayAbscens.Minimum,FullDayAbscens.Maximum) % (60*60*24) = 0
	AND FullDayAbscens.Minimum < IntraDayAbscens.Minimum
	AND FullDayAbscens.Maximum > IntraDayAbscens.Maximum
)

--Finally; Put the Fullday Abscens back into PersonAbscens (the one without Intra day)
INSERT INTO PersonAbsence
SELECT * FROM #PersonAbsence

DROP TABLE #PersonAbsence
DROP TABLE #PersonAbsence_Cleanup
*/
DROP TABLE #DeleteAbsence
GO

 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (216,'7.1.216') 
