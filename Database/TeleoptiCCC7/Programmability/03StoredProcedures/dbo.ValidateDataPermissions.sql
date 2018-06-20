------------------------  
-- Name: Bockemon
-- Date: 2018-06-15
-- Desc: Procedure for validating datapermissions for a user against a list of personIds
------------------------------------------------------------------------------------------------
-- exec [dbo].[ValidateDataPermissions] @ids_string='7FFE9DCD-4F8C-4511-8BFB-A1410093D9C9,90AF6B7A-849D-4535-9F4D-A1C5008931B7', @date='2018-06-14 00:00:00', @userId='76939942-AE47-46FF-86BB-A1410093C99A' , @appFuncForeginId='0154' 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateDataPermissions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ValidateDataPermissions]
GO

CREATE PROCEDURE [dbo].[ValidateDataPermissions]
@idsString nvarchar(max),
@date datetime, 
@userId uniqueidentifier, 
@appFuncForeginId nvarchar(255) 
AS
SET NOCOUNT ON

--if empty input, then RETURN
IF @idsString = '' SELECT CAST(1 AS BIT)

--create needed temp tables
CREATE TABLE #personIds (Id nvarchar(200) COLLATE database_default  NOT NULL)

-- Get searchString into temptable
INSERT INTO #personIds
SELECT * FROM dbo.SplitStringString(@idsString)

-- Find all permitted persons for this person @userId
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
WHERE p.Id = @userId
AND @date BETWEEN pp.StartDate AND pp.EndDate
AND (af.ForeignId = @appFuncForeginId OR af.ForeignId = '0000')
AND ad.AvailableDataRange = 2
UNION 
SELECT atiar.AvailableTeam FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.AvailableTeamsInApplicationRole atiar ON atiar.AvailableData = ad.Id
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
WHERE p.Id = @userId
AND (af.ForeignId = @appFuncForeginId OR af.ForeignId = '0000')
 
INSERT INTO #AvailableSites
SELECT t.Site AvailableSite FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
LEFT JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
LEFT JOIN dbo.Team t ON pp.Team = t.Id
LEFT JOIN dbo.Site s ON s.Id = t.Site
WHERE p.Id = @userId
AND @date BETWEEN pp.StartDate AND pp.EndDate
AND (af.ForeignId = @appFuncForeginId OR af.ForeignId = '0000')
AND ad.AvailableDataRange = 3
UNION 
SELECT atiar.AvailableSite FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.AvailableSitesInApplicationRole atiar ON atiar.AvailableData = ad.Id
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
WHERE p.Id = @userId
AND (af.ForeignId = @appFuncForeginId OR af.ForeignId = '0000')
 
INSERT INTO #AvailableBusinessUnits
SELECT s.BusinessUnit AvailableBusinessUnit FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
LEFT JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
LEFT JOIN dbo.Team t ON pp.Team = t.Id
LEFT JOIN dbo.Site s ON s.Id = t.Site
WHERE p.Id = @userId
AND @date BETWEEN pp.StartDate AND pp.EndDate
AND (af.ForeignId = @appFuncForeginId OR af.ForeignId = '0000')
AND ad.AvailableDataRange = 4
UNION 
SELECT atiar.AvailableBusinessUnit FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.AvailableUnitsInApplicationRole atiar ON atiar.AvailableData = ad.Id
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
WHERE p.Id = @userId
AND (af.ForeignId = @appFuncForeginId OR af.ForeignId = '0000')
 
-- 5 -> Always all allowed! --> Special case
DECLARE @allAvailable bit
SET @allAvailable = 0
 
SELECT @allAvailable=1 FROM dbo.PersonInApplicationRole piar 
INNER JOIN dbo.Person p ON piar.Person = p.Id
INNER JOIN dbo.AvailableData ad ON piar.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunctionInRole afir ON afir.ApplicationRole = ad.ApplicationRole
INNER JOIN dbo.ApplicationFunction af ON af.Id = afir.ApplicationFunction
WHERE p.Id = @userId
AND (af.ForeignId = @appFuncForeginId OR af.ForeignId = '0000')
AND ad.AvailableDataRange = 5
 
IF (@allAvailable = 1)
    BEGIN
		INSERT INTO #PermittedIds
        SELECT p.Id FROM dbo.Person p 
        INNER JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
        INNER JOIN dbo.Team t ON t.Id = pp.Team
        INNER JOIN dbo.Site s ON t.Site = s.Id
        WHERE 
        @date BETWEEN pp.StartDate AND pp.EndDate
    END
ELSE
    BEGIN
		INSERT INTO #PermittedIds
        SELECT p.Id FROM dbo.Person p 
        INNER JOIN dbo.PersonPeriod pp ON pp.Parent = p.Id
        INNER JOIN dbo.Team t ON t.Id = pp.Team
        INNER JOIN dbo.Site s ON t.Site = s.Id
        WHERE 
        @date BETWEEN pp.StartDate AND pp.EndDate AND
        (EXISTS (SELECT 1 FROM #AvailableTeams WHERE team=t.Id) OR
        EXISTS (SELECT 1 FROM #AvailableSites WHERE site=s.Id) OR
        EXISTS (SELECT 1 FROM #AvailableBusinessUnits WHERE businessunit=s.BusinessUnit)) 
    END

DROP TABLE #AvailableTeams
DROP TABLE #AvailableSites
DROP TABLE #AvailableBusinessUnits

IF (SELECT COUNT(*)  FROM #personIds ids INNER JOIN #PermittedIds pids ON ids.Id = pids.Id) <> (SELECT COUNT(*) FROM #personids)
BEGIN
	SELECT CAST(0 AS BIT) -- NOT EQ
END
ELSE
BEGIN
	SELECT CAST(1 AS BIT) -- 'EQ!'
END

