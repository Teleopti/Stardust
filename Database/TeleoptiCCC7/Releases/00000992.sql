----------------  
-- Name: Bockemon
-- Date: 2018-05-30
-- Desc: New procedure for searching persons while taking data permission into account.
----------------------------------------------------------------------------------------
--exec [ReadModel].[PersonFinderWithDataPermission] @search_string='a', @search_type='All', @leave_after='2018-05-28 00:00:00', @start_row=1, @end_row=500, @order_by=1, @sort_direction=1, @culture=1053, @perm_date='2018-05-28 00:00:00', @perm_userid='76939942-AE47-46FF-86BB-A1410093C99A' , @perm_foreignId='0154' 

CREATE PROCEDURE [ReadModel].[PersonFinderWithDataPermission]
@search_string nvarchar(max),
@search_type nvarchar(200),
@leave_after datetime,
@start_row int,
@end_row int, 
@order_by int, 
@sort_direction int,
@culture int,
@perm_date datetime, 
@perm_userid uniqueidentifier, 
@perm_foreignId nvarchar(255) 
AS
SET NOCOUNT ON

--if empty input, then RETURN
IF @search_string = '' RETURN

--declare
DECLARE @dynamicSQL nvarchar(4000)
DECLARE @eternityDate datetime
DECLARE @totalRows int
DECLARE @countWords int
DECLARE @collation nvarchar(50)

--Set collation
SELECT @collation = cc.[Collation]
FROM ReadModel.CollationCulture() cc
inner join fn_helpcollations() fn
on cc.[Collation] = fn.name collate database_default
WHERE cc.Culture = @culture

--no match? Go for database_default
SELECT @collation = ISNULL(@collation,'database_default')

--create needed temp tables
CREATE TABLE #strings (SearchWord nvarchar(200) COLLATE database_default  NOT NULL  )

CREATE TABLE #result(
	[PersonId] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[EmploymentNumber] [nvarchar](50) NOT NULL,
	[Note] [nvarchar](1024) NOT NULL,
	[TerminalDate] [datetime] NULL,
	[TeamId] [uniqueidentifier]  NULL,
	[SiteId] [uniqueidentifier]  NULL,
	[BusinessUnitId] [uniqueidentifier]  NULL
	)

-- Get searchString into temptable
SELECT @search_string = REPLACE(@search_string,' ', ',') --separated by space from client
SELECT @search_string = REPLACE(@search_string,'%', '[%]') --make '%' valuable search value
INSERT INTO #strings
SELECT * FROM dbo.SplitStringString(@search_string)

--count number of SearchWord
SELECT @countWords = COUNT(SearchWord) FROM #strings

-- Find all permitted persons for this person @perm_userid
CREATE TABLE #AvailableTeams (team uniqueidentifier)
CREATE TABLE #AvailableSites (site uniqueidentifier)
CREATE TABLE #AvailableBusinessUnits (businessunit uniqueidentifier)
CREATE TABLE #PermittedIds (Id uniqueidentifier)
 
INSERT INTO #AvailableTeams
SELECT pp.Team AvailableTeam FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
LEFT JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
LEFT JOIN dbo.Team t ON pp.Team = t.Id
LEFT JOIN dbo.Site s ON s.Id = t.Site
WHERE p.Id = @perm_userid
AND @perm_date BETWEEN pp.StartDate AND pp.EndDate
AND af.ForeignId = @perm_foreignId
AND ad.AvailableDataRange = 2
UNION 
SELECT atiar.AvailableTeam FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.AvailableTeamsInApplicationRole atiar ON atiar.AvailableData = ad.Id
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
WHERE p.Id = @perm_userid
AND af.ForeignId = @perm_foreignId 
 
INSERT INTO #AvailableSites
SELECT t.Site AvailableSite FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
LEFT JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
LEFT JOIN dbo.Team t ON pp.Team = t.Id
LEFT JOIN dbo.Site s ON s.Id = t.Site
WHERE p.Id = @perm_userid
AND @perm_date BETWEEN pp.StartDate AND pp.EndDate
AND af.ForeignId = @perm_foreignId
AND ad.AvailableDataRange = 3
UNION 
SELECT atiar.AvailableSite FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.AvailableSitesInApplicationRole atiar ON atiar.AvailableData = ad.Id
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
WHERE p.Id = @perm_userid
AND af.ForeignId = @perm_foreignId
 
INSERT INTO #AvailableBusinessUnits
SELECT s.BusinessUnit AvailableBusinessUnit FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
LEFT JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
LEFT JOIN dbo.Team t ON pp.Team = t.Id
LEFT JOIN dbo.Site s ON s.Id = t.Site
WHERE p.Id = @perm_userid
AND @perm_date BETWEEN pp.StartDate AND pp.EndDate
AND af.ForeignId = @perm_foreignId
AND ad.AvailableDataRange = 4
UNION 
SELECT atiar.AvailableBusinessUnit FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.AvailableUnitsInApplicationRole atiar ON atiar.AvailableData = ad.Id
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
WHERE p.Id = @perm_userid
AND af.ForeignId = @perm_foreignId
 
-- 5 -> Always all allowed! --> Special case
DECLARE @allAvailable bit
SET @allAvailable = 0
 
SELECT @allAvailable=1 FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
WHERE p.Id = @perm_userid
AND af.ForeignId = @perm_foreignId
AND ad.AvailableDataRange = 5
 
IF (@allAvailable = 1)
    BEGIN
		INSERT INTO #PermittedIds
        SELECT p.Id FROM dbo.Person p 
        INNER JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
        INNER JOIN dbo.Team t ON t.Id = pp.Team
        INNER JOIN dbo.Site s ON t.Site = s.Id
        WHERE 
        @perm_date BETWEEN pp.StartDate AND pp.EndDate
    END
ELSE
    BEGIN
		INSERT INTO #PermittedIds
        SELECT p.Id FROM dbo.Person p 
        INNER JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
        INNER JOIN dbo.Team t ON t.Id = pp.Team
        INNER JOIN dbo.Site s ON t.Site = s.Id
        WHERE 
        @perm_date BETWEEN pp.StartDate AND pp.EndDate AND
        (EXISTS (SELECT 1 FROM #AvailableTeams WHERE team=t.Id) OR
        EXISTS (SELECT 1 FROM #AvailableSites WHERE site=s.Id) OR
        EXISTS (SELECT 1 FROM #AvailableBusinessUnits WHERE businessunit=s.BusinessUnit)) 
    END

DROP TABLE #AvailableTeams
DROP TABLE #AvailableSites
DROP TABLE #AvailableBusinessUnits
-- Now stored all permitted userId in #PermittedIds

IF @search_type <> 'All'
	BEGIN
		INSERT INTO #Result 
		SELECT PersonId, FirstName, LastName, EmploymentNumber, Note, TerminalDate, TeamId, SiteId, BusinessUnitId 
		FROM ReadModel.FindPerson with (nolock) 
		CROSS JOIN #strings s 
		WHERE ISNULL(TerminalDate, '2100-01-01') >= @leave_after 
			AND SearchType=@search_type 
			AND SearchValue like N'%' + s.SearchWord + '%'
		GROUP BY PersonId, FirstName, LastName, EmploymentNumber, Note, TerminalDate, TeamId, SiteId, BusinessUnitId 
		HAVING COUNT(DISTINCT s.SearchWord) >= @countWords --AND => Must have hit on all words
	END
ELSE
	BEGIN
		INSERT INTO #Result 
		SELECT PersonId, FirstName, LastName, EmploymentNumber, Note, TerminalDate, TeamId, SiteId, BusinessUnitId 
		FROM ReadModel.FindPerson with (nolock) 
		CROSS JOIN #strings s 
		WHERE ISNULL(TerminalDate, '2100-01-01') >= @leave_after 
			AND SearchValue like N'%' + s.SearchWord + '%'
		GROUP BY PersonId, FirstName, LastName, EmploymentNumber, Note, TerminalDate, TeamId, SiteId, BusinessUnitId
		HAVING COUNT(DISTINCT s.SearchWord) >= @countWords --AND => Must have hit on all words
	END

--get total count
DECLARE @total int 
SELECT @total = COUNT(*) FROM #result r JOIN #PermittedIds p ON r.PersonId = p.Id

SELECT @dynamicSQL=''

SELECT @dynamicSQL='SELECT ' + cast(@total as nvarchar(10)) + ' AS TotalCount, *
    FROM (
    SELECT PersonId, FirstName, LastName, EmploymentNumber, Note, TerminalDate, TeamId, SiteId, BusinessUnitId, ROW_NUMBER() OVER(
				ORDER BY ' +
						CASE @order_by
							WHEN 0 THEN 'PC.FirstName collate ' + @collation
							WHEN 1 THEN 'PC.LastName collate ' + @collation
							WHEN 2 THEN 'PC.EmploymentNumber collate ' + @collation
							WHEN 3 THEN 'PC.Note collate ' + @collation
							ELSE 'CONVERT(varchar(50), PC.TerminalDate, 120) collate ' + @collation
						END +' ' + 
						CASE @sort_direction
							WHEN 1 THEN 'ASC) AS RowNumber'
							ELSE 'DESC) AS RowNumber'
						END +' ' + 
        ' FROM    #result PC JOIN #PermittedIds PID on PC.PersonId = PID.Id) #result WHERE  RowNumber >= '+ cast(@start_row as nvarchar(10)) +' AND RowNumber < '+ cast(@end_row as nvarchar(10))

--debug
--print @dynamicSQL

--return
EXEC sp_executesql @dynamicSQL   
