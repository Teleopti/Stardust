IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[ChangedDataOnStep]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[ChangedDataOnStep]
GO

CREATE PROCEDURE [mart].[ChangedDataOnStep]
@stepName nvarchar(500),
@lastTime datetime,
@buId uniqueidentifier 
-- =============================================
-- Author:		Ola
-- Create date: 2013-04-10
-- Description:	Returns the persons and dates that needs to be updated by ETL
-- =============================================
-- Date			Who	Description
-- =============================================
--  exec [mart].[ChangedDataOnStep] 'stg_schedule_day_off_count, stg_day_off, dim_day_off', '2013-04-15 00:00:00.000' ,'FC2F309C-3E3C-4CFB-9C11-A1570077B92B'
AS
CREATE TABLE #persons (PersonId uniqueidentifier NOT NULL)
CREATE TABLE #PersonsUpdated
	(PersonId uniqueidentifier NOT NULL,
	BelongsToDate smalldatetime NOT NULL,
	InsertedOn datetime NOT NULL
	)

INSERT INTO #persons (PersonId)
SELECT DISTINCT Parent
FROM dbo.PersonPeriod pp
INNER JOIN dbo.team t
	ON t.Id = pp.team
INNER JOIN dbo.site s
	ON t.site = s.Id
INNER JOIN dbo.BusinessUnit bu
	ON bu.id = s.BusinessUnit
WHERE bu.id = @buId

--Get all Schedule days that are updated after last ETL run
INSERT INTO #PersonsUpdated (PersonId, BelongsToDate, InsertedOn)
SELECT s.PersonId, s.BelongsToDate, s.InsertedOn
FROM [ReadModel].[ScheduleDay] s
INNER JOIN #persons p ON p.PersonId = s.PersonId
WHERE InsertedOn >= @lastTime -- >= (not > ) to be sure we do not miss any

--return Persons
SELECT PersonId AS Person, BelongsToDate AS [Date]
FROM #PersonsUpdated
ORDER BY BelongsToDate

GO