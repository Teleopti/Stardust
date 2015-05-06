IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[PersonFinderWithCriteria]')
   AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[PersonFinderWithCriteria]
GO

-- exec [ReadModel].PersonFinderWithCriteria 'FirstName : ashley; pierre, Organization: london', '2001-01-01', 1, 100, 1, 1, 1053
-- =============================================
-- Author:      Xinfeng
-- Create date: 2005-05-09
-- Description: Gets the person(s) match search criteria specified in the ReadModel (AND-search), Based on ReadModel.PersonFinder
-- Change Log
------------------------------------------------
-- When         Who       What
-- =============================================
CREATE PROCEDURE [ReadModel].[PersonFinderWithCriteria]
@search_criterias nvarchar(max),
@leave_after datetime,
@start_row int,
@end_row int,
@order_by int,
@sort_direction int,
@culture int
AS
SET NOCOUNT ON

--if empty input, then RETURN
IF @search_criterias = '' RETURN
SELECT @search_criterias = REPLACE(@search_criterias, '''', '') -- Remove all single quote to prevent SQL injection (Is that OK?)
SELECT @search_criterias = REPLACE(@search_criterias, '%', '[%]') --make '%' valuable search value

--declare
DECLARE @leave_after_ISO nvarchar(10)
DECLARE @dynamicSQL nvarchar(max)
DECLARE @cursorString nvarchar(50)
DECLARE @cursorCount int
DECLARE @criteriaCount int
DECLARE @collation nvarchar(50)

--Set collation
SELECT @collation = cc.[Collation]
FROM ReadModel.CollationCulture() cc
INNER JOIN fn_helpcollations() fn
ON cc.[Collation] = fn.name collate database_default
WHERE cc.Culture = @culture

--no match? Go for database_default
SELECT @collation = ISNULL(@collation,'database_default')

--create needed temp tables
-- Temp table for combined criteria string (FirstName:ashley)
CREATE TABLE #SearchStrings (SearchString nvarchar(200) COLLATE database_default NOT NULL)
-- Temp table for splitted criteria
CREATE TABLE #SearchCriteria (SearchType nvarchar(200) NOT NULL, SearchValue nvarchar(max) NULL)
CREATE TABLE #PersonId (PersonId uniqueidentifier)

CREATE TABLE #result (
   [PersonId] [uniqueidentifier] NOT NULL,
   [FirstName] [nvarchar](50) NOT NULL,
   [LastName] [nvarchar](50) NOT NULL,
   [EmploymentNumber] [nvarchar](50) NOT NULL,
   [Note] [nvarchar](1024) NOT NULL,
   [TerminalDate] [datetime] NULL,
   [TeamId] [uniqueidentifier] NULL,
   [SiteId] [uniqueidentifier] NULL,
   [BusinessUnitId] [uniqueidentifier] NULL
)

--re-init @dynamicSQL
SELECT @dynamicSQL=''

-- Get searchString into temptable
INSERT INTO #SearchStrings
SELECT * FROM dbo.SplitStringString(@search_criterias)

--select * from #SearchStrings

DECLARE @searchString nvarchar(max)
DECLARE SearchStringCursor CURSOR FOR
SELECT RTRIM(LTRIM(SearchString)) FROM #SearchStrings ORDER BY SearchString;
OPEN SearchStringCursor;

FETCH NEXT FROM SearchStringCursor INTO @searchString

DECLARE @splitterIndex int
select @splitterIndex = -1

Declare @searchType nvarchar(200)
Declare @searchValue nvarchar(max)

WHILE @@FETCH_STATUS = 0
BEGIN
  --print 'SearchString: "' + @searchString + '"'
  Select @splitterIndex = CHARINDEX(':', @searchString)
  IF @splitterIndex > 0
  BEGIN
   select @searchType = LTRIM(RTRIM(SUBSTRING(@searchString, 0, @splitterIndex)))
   --print 'SearchType: "' + @searchType + '"'
   select @searchValue = LTRIM(RTRIM(SUBSTRING(@searchString, @splitterIndex + 1, LEN(@searchString) - @splitterIndex)))
   --print 'SearchValue: "' + @searchValue + '"'
   Insert Into #SearchCriteria Values(@searchType, @searchValue)
  END

  FETCH NEXT FROM SearchStringCursor INTO @searchString
END

--select * from #SearchCriteria
CLOSE SearchStringCursor
DEALLOCATE SearchStringCursor;

--count number of search criterias
SELECT @criteriaCount = COUNT(SearchType)
FROM #SearchCriteria

--convert @leave_after to ISO-format string
SELECT @leave_after_ISO = CONVERT(NVARCHAR(10), @leave_after,120)

------------
--cursor for adding "AND" or "OR" conditions for each search criteria
------------
DECLARE SearchCriteriaCursor CURSOR FOR
SELECT SearchType, SearchValue, ROW_NUMBER() OVER(ORDER BY SearchType DESC) as RowNum FROM #SearchCriteria;
OPEN SearchCriteriaCursor;

FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue, @cursorCount

WHILE @@FETCH_STATUS = 0
BEGIN
   DECLARE @valueClause nvarchar(max)

   DECLARE @valueSplitterIndex int
   SELECT @valueSplitterIndex = CHARINDEX(';', @searchValue)

   -- If the search value contains multiple keyword (combined with ';')
   -- Then generate search conditions for every search keyword combined with OR
   IF @valueSplitterIndex > 0
   BEGIN
      DECLARE @notProcessedSearchValue nvarchar(max)
      DECLARE @valuePart nvarchar(max)

      SELECT @notProcessedSearchValue = @searchValue

      IF RIGHT(@searchValue, 1) <> ';'
         SELECT @notProcessedSearchValue = @searchValue + ';'

      SELECT @valueClause = '('
      WHILE @valueSplitterIndex > 0
      BEGIN
         SELECT @valuePart = LTRIM(RTRIM(SUBSTRING(@notProcessedSearchValue, 0, @valueSplitterIndex)))
		 IF @valuePart <> ''
		 BEGIN
            IF @valueClause <> '('
               SELECT @valueClause = @valueClause + ' OR '
            SELECT @valueClause = @valueClause + 'SearchValue LIKE N''%' + @valuePart + '%'''
		 END
         SELECT @notProcessedSearchValue = LTRIM(RTRIM(SUBSTRING(@notProcessedSearchValue, @valueSplitterIndex + 1,
            LEN(@notProcessedSearchValue) - @valueSplitterIndex)))
         SELECT @valueSplitterIndex = CHARINDEX(';', @notProcessedSearchValue)
      END
      SELECT @valueClause = @valueClause + ')'
   END
   ELSE
   BEGIN
      SELECT @valueClause = 'SearchValue like N''%' + @searchValue + '%'''
   END

   PRINT @valueClause

   IF @valueClause <> '()'
   BEGIN
      SELECT @dynamicSQL = @dynamicSQL + 'SELECT PersonId FROM ReadModel.FindPerson WHERE '
         + 'ISNULL(TerminalDate, ''2100-01-01'') >= ''' + @leave_after_ISO + ''' AND ' + @valueClause

      --If 'All' set searchtype as a separate AND condition
      IF @searchType <> 'All'
         SELECT @dynamicSQL = @dynamicSQL + ' AND (SearchType = '''' OR SearchType = '''+ @searchType + ''')'

      --add INTERSECT between each result set
      IF @cursorCount <> @criteriaCount --But NOT on last condition, the syntax is different
         SELECT @dynamicSQL = @dynamicSQL + ' INTERSECT '
   END

   --fectch next
   FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue, @cursorCount
END

CLOSE SearchCriteriaCursor;
DEALLOCATE SearchCriteriaCursor;

--debug
SELECT @dynamicSQL

--insert into PersonId temp table
INSERT INTO #PersonId
EXEC sp_executesql @dynamicSQL

--prepare restult
INSERT INTO #result
SELECT DISTINCT a.PersonId, FirstName, LastName, EmploymentNumber, Note, TerminalDate, [TeamId], [SiteId], [BusinessUnitId]
FROM ReadModel.FindPerson a
INNER JOIN #PersonId b
ON a.PersonId = b.PersonId
ORDER BY LastName, FirstName

--get total count
DECLARE @total int
SELECT @total = COUNT(*) FROM #result

SELECT @dynamicSQL=''

SELECT @dynamicSQL='SELECT ' + cast(@total as nvarchar(10)) + ' AS TotalCount, *
    FROM (
    SELECT *, ROW_NUMBER() OVER(ORDER BY ' +
        CASE @order_by
            WHEN 0 THEN 'PC.FirstName collate ' + @collation
            WHEN 1 THEN 'PC.LastName collate ' + @collation
            WHEN 2 THEN 'PC.EmploymentNumber collate ' + @collation
            WHEN 3 THEN 'PC.Note collate ' + @collation
            ELSE 'CONVERT(varchar(50), PC.TerminalDate, 120) collate ' + @collation
        END + ' ' +
        CASE @sort_direction
            WHEN 1 THEN 'ASC) AS RowNumber'
            ELSE 'DESC) AS RowNumber'
        END + ' ' +
     ' FROM #result PC) #result WHERE RowNumber >= '+ cast(@start_row as nvarchar(10))
     + ' AND RowNumber < '+ cast(@end_row as nvarchar(10))

--debug
--SELECT @dynamicSQL

--return
EXEC sp_executesql @dynamicSQL
GO
