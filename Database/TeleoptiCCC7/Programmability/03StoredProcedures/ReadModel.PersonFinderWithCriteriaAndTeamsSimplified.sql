IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[PersonFinderWithCriteriaAndTeamsSimplified]')
   AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[PersonFinderWithCriteriaAndTeamsSimplified]
GO

-- exec [ReadModel].[PersonFinderWithCriteriaAndTeams_New] N'FirstName:Ash,LastName:an', '2017-02-14 00:00:00', N'0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495,e5f968d7-6f6d-407c-81d5-9b5e015ab495,d7a9c243-8cd8-406e-9889-9b5e015ab495,7f38bb91-d33b-4f5b-8161-9b5e015ab495,34590a63-6331-4921-bc9f-9b5e015ab495,e7ce8892-4db3-49c8-bdf6-9b5e015ab495'
-- =============================================
-- Author:      Chundan
-- Create date: 2016-12-09
-- Description: Gets the person(s) match search criteria and teams specified in the ReadModel (AND-search), Based on ReadModel.PersonFinder
-- Change Log
------------------------------------------------
-- When         Who       What
-- 2016-12-09   Chundan   create
-- 2017-02-14   Yanyi     Simplify by removing sorting and paging
-- =============================================
CREATE PROCEDURE [ReadModel].[PersonFinderWithCriteriaAndTeamsSimplified]
@search_criterias nvarchar(max),
@belongs_to_date datetime,
@team_ids nvarchar(max)
AS
BEGIN
SET NOCOUNT ON

--if empty input, then RETURN
IF @team_ids = '' AND @search_criterias = '' RETURN
SELECT @search_criterias = REPLACE(@search_criterias, '%', '[%]') --make '%' valuable search value

--declare
DECLARE @belongs_to_date_ISO nvarchar(10)
DECLARE @dynamicSQL nvarchar(max)
DECLARE @cursorString nvarchar(50)
DECLARE @cursorCount int
DECLARE @criteriaCount int
DECLARE @isSearchInAll bit

--create needed temp tables
-- Temp table for combined criteria string (FirstName:ashley)
DECLARE @SearchStrings TABLE(
	SearchString nvarchar(200) NOT NULL
)

DECLARE @SearchCriteria TABLE(
	SearchType nvarchar(200) NOT NULL,
	SearchValue nvarchar(max) NULL
)

CREATE TABLE #TeamIds (tId uniqueidentifier NOT NULL)
CREATE TABLE #IntermediatePersonId (PersonId uniqueidentifier)

SELECT @dynamicSQL=''

-- Get searchString into temptable
INSERT INTO @SearchStrings
SELECT * FROM dbo.SplitStringString(@search_criterias)

--Get teamIds to tempt table #teamIds
INSERT INTO #TeamIds
SELECT * FROM dbo.SplitStringString(@team_ids)


--select * from #SearchStrings

DECLARE @searchString nvarchar(max)
DECLARE SearchStringCursor CURSOR FOR
SELECT RTRIM(LTRIM(SearchString)) FROM @SearchStrings ORDER BY SearchString;
OPEN SearchStringCursor;

FETCH NEXT FROM SearchStringCursor INTO @searchString

SELECT @isSearchInAll = 0

DECLARE @splitterIndex int
select @splitterIndex = -1

Declare @searchType nvarchar(200)
Declare @searchValue nvarchar(max)

Declare @keywordSplitterIndex int
Declare @searchKeyword nvarchar(max)
Declare @notProcessedSearchValue nvarchar(100)

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @splitterIndex = CHARINDEX(':', @searchString)
	IF @splitterIndex > 0
	BEGIN
		SELECT @searchType = LTRIM(RTRIM(SUBSTRING(@searchString, 0, @splitterIndex)))
		SELECT @searchValue = LTRIM(RTRIM(SUBSTRING(@searchString, @splitterIndex + 1, LEN(@searchString) - @splitterIndex)))
		IF @searchValue = ''
		BEGIN			
			CLOSE SearchStringCursor
			DEALLOCATE SearchStringCursor;
			RETURN
		END

		SELECT @keywordSplitterIndex = CHARINDEX(';', @searchValue)

		IF @searchType = 'All' SELECT @isSearchInAll = 1

		IF @searchType = 'All' AND @keywordSplitterIndex > 0
		BEGIN
			Select @notProcessedSearchValue = @searchValue
			IF RIGHT(@notProcessedSearchValue, 1) <> ';'
				Select @notProcessedSearchValue = @notProcessedSearchValue + ';'
			WHILE @keywordSplitterIndex > 0
			BEGIN
				SELECT @searchKeyword = LTRIM(RTRIM(SUBSTRING(@notProcessedSearchValue, 0, @keywordSplitterIndex)))

				IF @searchKeyword <> ''
					INSERT INTO @SearchCriteria VALUES('All', @searchKeyword)

				SELECT @notProcessedSearchValue = LTRIM(RTRIM(SUBSTRING(@notProcessedSearchValue, @keywordSplitterIndex + 1,
					LEN(@notProcessedSearchValue) - @keywordSplitterIndex)))
				SELECT @keywordSplitterIndex = CHARINDEX(';', @notProcessedSearchValue)
			END
		END
		ELSE
			INSERT INTO @SearchCriteria VALUES(@searchType, @searchValue)
	END

	FETCH NEXT FROM SearchStringCursor INTO @searchString
END


CLOSE SearchStringCursor
DEALLOCATE SearchStringCursor;

--count number of search criterias
SELECT @criteriaCount = COUNT(SearchType) FROM @SearchCriteria


--convert @belongs_to_date to ISO-format string
SELECT @belongs_to_date_ISO = CONVERT(NVARCHAR(10), @belongs_to_date,120)


INSERT INTO  #IntermediatePersonId
SELECT p.Id FROM #TeamIds t 
INNER JOIN dbo.PersonPeriod pp with (nolock)   ON t.tId = pp.Team 
INNER JOIN dbo.Person p with (nolock) ON pp.Parent = p.Id 
WHERE p.IsDeleted = 0
AND ISNULL(p.TerminalDate, '2100-01-01') >=  @belongs_to_date_ISO  
AND  (pp.StartDate IS NULL OR pp.StartDate <=  @belongs_to_date_ISO  ) AND ( pp.EndDate IS NULL OR pp.EndDate >=  @belongs_to_date_ISO )

IF @criteriaCount = 0 AND @team_ids <> ''
BEGIN
	SELECT @dynamicSQL = 'SELECT PersonId FROM #IntermediatePersonId '
END

------------
--search in specific one or sevaral types
--cursor for adding "AND" or "OR" conditions for each search criteria
------------


DECLARE SearchCriteriaCursor CURSOR FOR
SELECT SearchType, SearchValue, ROW_NUMBER() OVER(ORDER BY SearchType DESC) as RowNum FROM @SearchCriteria;
OPEN SearchCriteriaCursor;

FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue, @cursorCount

WHILE @@FETCH_STATUS = 0
BEGIN
	IF @isSearchInAll = 1 AND @searchType <> 'All' 
	BEGIN
		--fectch next
		FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue, @cursorCount
		CONTINUE
	END

	DECLARE @valueClause nvarchar(max)

	--DECLARE @keywordSplitterIndex int
	SELECT @keywordSplitterIndex = CHARINDEX(';', @searchValue)

	-- If the search value contains multiple keyword (combined with ';')
	-- Then generate search conditions for every search keyword combined with OR
	IF @keywordSplitterIndex > 0
	BEGIN
		SELECT @notProcessedSearchValue = @searchValue

		IF RIGHT(@searchValue, 1) <> ';'
			SELECT @notProcessedSearchValue = @searchValue + ';'

		SELECT @valueClause = '('
		WHILE @keywordSplitterIndex > 0
		BEGIN
			SELECT @searchKeyword = LTRIM(RTRIM(SUBSTRING(@notProcessedSearchValue, 0, @keywordSplitterIndex)))
			IF @searchKeyword <> ''
			BEGIN
			IF @valueClause <> '('
				SELECT @valueClause = @valueClause + ' OR '
			SELECT @valueClause = @valueClause + 'fp.SearchValue LIKE N''%' + REPLACE(@searchKeyword, '''', '''''') + '%'''
			END
			SELECT @notProcessedSearchValue = LTRIM(RTRIM(SUBSTRING(@notProcessedSearchValue, @keywordSplitterIndex + 1,
			LEN(@notProcessedSearchValue) - @keywordSplitterIndex)))
			SELECT @keywordSplitterIndex = CHARINDEX(';', @notProcessedSearchValue)
		END
		SELECT @valueClause = @valueClause + ')'
	END
	ELSE
		SELECT @valueClause = 'fp.SearchValue like N''%' + REPLACE(@searchValue, '''', '''''') + '%'''

	IF @valueClause <> '()'
	BEGIN
		SELECT @dynamicSQL = @dynamicSQL + 'SELECT DISTINCT fp.PersonId FROM #IntermediatePersonId t '
					 + 'INNER JOIN ReadModel.FindPerson fp with (nolock)   ON t.PersonId = fp.PersonId '
					 + 'WHERE '
					 + @valueClause
				
		--If 'All' set searchtype as a separate AND condition
		IF @searchType <> 'All'
			SELECT @dynamicSQL = @dynamicSQL + ' AND (fp.SearchType = '''' OR fp.SearchType = '''+ @searchType + ''')'

		--add INTERSECT between each result set
		IF @cursorCount <> @criteriaCount --But NOT on last condition, the syntax is different
			SELECT @dynamicSQL = @dynamicSQL + CHAR(13)+CHAR(10) + ' INTERSECT ' + CHAR(13)+CHAR(10)
	END

	--fectch next
	FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue, @cursorCount
END

CLOSE SearchCriteriaCursor;
DEALLOCATE SearchCriteriaCursor;


--select * from #IntermediatePersonId
--select @dynamicSQL

EXEC sp_executesql @dynamicSQL

END
GO
