IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[PersonFinderWithCriteria]')
   AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[PersonFinderWithCriteria]
GO

-- EXEC [ReadModel].[PersonFinderWithCriteria] 'FirstName : ashley; pierre, Organization: london', '2001-01-01', 1, 100, '1:1,2:0', 1053, '928DD0BC-BF40-412E-B970-9B5E015AADEA', '2016-10-10'
-- =============================================
-- Author:      Xinfeng
-- Create date: 2005-05-09
-- Description: Gets the person(s) match search criteria specified in the ReadModel (AND-search), Based on ReadModel.PersonFinder
-- Change Log
------------------------------------------------
-- When         Who       What
-- 2015-05-08   Chundan   Add mutual search for type 'All' and other specific searchTypes, and make searching in 'All' match all values
-- 2015-05-11   Xinfeng   Optimize search value parsing
-- 2015-06-16   Xinfeng   Add multiple-orderby support
-- =============================================
CREATE PROCEDURE [ReadModel].[PersonFinderWithCriteria]
@search_criterias nvarchar(max),
@leave_after datetime,
@start_row int,
@end_row int,
@order_by nvarchar(30),
@culture int,
@business_unit_id uniqueidentifier,
@belongs_to_date datetime
AS
SET NOCOUNT ON

--if empty input, then RETURN
IF @search_criterias = '' RETURN

--declare
DECLARE @leave_after_ISO nvarchar(10)
DECLARE @belongs_to_date_ISO nvarchar(10)
DECLARE @dynamicSQL nvarchar(max)
DECLARE @eternityDate datetime
DECLARE @totalRows int
DECLARE @countWords int
DECLARE @isSearchInAll bit
DECLARE @collation nvarchar(50)

DECLARE @directDynamicSQL nvarchar(max)
DECLARE @allQuery nvarchar(max)
DECLARE @stringholder nvarchar(max)
DECLARE @stringholder2 nvarchar(max)

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
-- Temp table for vaules with type 'All'
CREATE TABLE #SearchStringsInAll (SearchWord nvarchar(200) COLLATE database_default  NOT NULL  )
-- Temp table for splitted criteria
CREATE TABLE #SearchCriteria (SearchType nvarchar(200) NOT NULL, SearchValue nvarchar(max) NULL)
CREATE TABLE #PersonId (PersonId uniqueidentifier)
CREATE TABLE #IntermediatePersonId (PersonId uniqueidentifier)
CREATE TABLE #DirectSearchCriteria (SearchType nvarchar(200) NOT NULL, SearchValue nvarchar(max) NULL)

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
SELECT @search_criterias = REPLACE(@search_criterias, '%', '[%]') --make '%' valuable search value
INSERT INTO #SearchCriteria SELECT LTRIM(RTRIM(SUBSTRING(c.string, 0, CHARINDEX(':',c.string)))),g.string FROM dbo.SplitStringString(@search_criterias) as c CROSS APPLY dbo.SplitStringByChar(LTRIM(RTRIM(SUBSTRING(c.string, CHARINDEX(':',c.string) + 1, LEN(c.string) - CHARINDEX(':',c.string)))), ';') as g

if EXISTS (SELECT * FROM #SearchCriteria WHERE SearchValue = '')
	BEGIN
		SELECT * FROM #result
		RETURN
	END

IF EXISTS (SELECT * FROM #SearchCriteria WHERE SearchType = 'ALL')
	begin
		SELECT @isSearchInAll = 1
		INSERT INTO #DirectSearchCriteria SELECT * FROM #SearchCriteria
	end
ELSE
	begin
		SELECT @isSearchInAll = 0
		INSERT INTO #DirectSearchCriteria SELECT * FROM #SearchCriteria WHERE SearchType IN ('FirstName', 'LastName', 'EmploymentNumber', 'Note')
		DELETE FROM #SearchCriteria WHERE SearchType IN ('FirstName', 'LastName', 'EmploymentNumber', 'Note')
	end


DECLARE @splitterIndex int
select @splitterIndex = -1

Declare @searchType nvarchar(200)
Declare @searchValue nvarchar(max)

Declare @keywordSplitterIndex int

--debug
--SELECT * from #SearchCriteria
--SELECT * from #DirectSearchCriteria

--convert @leave_after to ISO-format string
SELECT @leave_after_ISO = CONVERT(NVARCHAR(10), @leave_after,120)

--convert @belongs_to_date to ISO-format string
SELECT @belongs_to_date_ISO = CONVERT(NVARCHAR(10), @belongs_to_date,120)

IF @isSearchInAll = 1
BEGIN	
	SELECT @allQuery = ''
	SELECT @directDynamicSQL = 'SELECT p.Id AS PersonId FROM dbo.Person p with (nolock) WHERE ' 
							+ 'ISNULL(p.TerminalDate, ''2100-01-01'') >= ''' + @leave_after_ISO + ''' '

	SELECT @dynamicSQL = 'SELECT PersonId FROM ReadModel.FindPerson with (nolock) WHERE '
			+ 'ISNULL(TerminalDate, ''2100-01-01'') >= ''' + @leave_after_ISO + ''' ' 
			+ ' AND ( (StartDateTime IS NULL OR StartDateTime <=  ''' + @belongs_to_date_ISO + ''' ) AND ( EndDateTime IS NULL OR EndDateTime >= ''' + @belongs_to_date_ISO + ''' ))'
			
	SET @stringholder = ''
	SET @stringholder2 = ''

	DECLARE SearchCriteriaCursor CURSOR FOR
	SELECT SearchType, SearchValue FROM #SearchCriteria;
	OPEN SearchCriteriaCursor;

	FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue

	WHILE @@FETCH_STATUS = 0
	BEGIN						
		IF @searchValue = ''
		BEGIN			
			FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue
			CONTINUE
		END
		SET @searchValue = REPLACE(@searchValue, '''', '''''')	
		IF @allQuery <> ''
			SELECT @allQuery = @allQuery + ' INTERSECT '
			
		SET @stringholder = ' AND (FirstName LIKE N''%' + @searchValue   + '%'' ' 
							+ ' OR LastName LIKE N''%' + @searchValue   + '%'' '
							+ ' OR EmploymentNumber LIKE N''%' + @searchValue   + '%'' '
							+ ' OR Note LIKE N''%' + @searchValue   + '%'' )'

		SET @stringholder2 = ' AND SearchValue LIKE N''%' + @searchValue   + '%'' ' 


		SELECT @allQuery = @allQuery + ' ( ' + @directDynamicSQL + @stringholder + ' UNION ' + @dynamicSQL + @stringholder2 + ' ) '

		FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue
	END

	CLOSE SearchCriteriaCursor;
	DEALLOCATE SearchCriteriaCursor;	
		
	SELECT @allQuery = 'WITH TMP_P (PersonId) AS (' + @allQuery + ' ) ' + ' INSERT INTO #IntermediatePersonId SELECT PersonId FROM TMP_P '
    EXEC sp_executesql @allQuery
	SELECT @dynamicSQL = 'SELECT PersonId FROM #IntermediatePersonId'
END
ELSE
BEGIN
	IF EXISTS (SELECT * FROM #DirectSearchCriteria)
	BEGIN
		DECLARE DirectSearchCriteriaCursor CURSOR FOR
		SELECT DISTINCT SearchType FROM #DirectSearchCriteria;
		OPEN DirectSearchCriteriaCursor;

		SELECT @directDynamicSQL = 'SELECT p.Id FROM dbo.Person p with (nolock) WHERE ' 
								 + 'ISNULL(p.TerminalDate, ''2100-01-01'') >= ''' + @leave_after_ISO + ''' '

		FETCH NEXT FROM DirectSearchCriteriaCursor INTO @searchType

		WHILE @@FETCH_STATUS = 0
		BEGIN			
			SET @stringholder = ''
			SELECT @stringholder= @stringholder+ 'OR (' + @searchType + ' LIKE N''%' + REPLACE(SearchValue,'''', '''''') + '%'' ) ' FROM #DirectSearchCriteria WHERE SearchType = @SearchType
	
			IF @stringholder <> ''
			BEGIN
				SET @stringholder= SUBSTRING(@stringholder, 4, LEN(@stringholder) - 3)
				SET @directDynamicSQL = @directDynamicSQL + ' AND ( ' + @stringholder + ') ' 
			END
	
			FETCH NEXT FROM DirectSearchCriteriaCursor INTO @searchType
		END

		CLOSE DirectSearchCriteriaCursor;
		DEALLOCATE DirectSearchCriteriaCursor;
	
		SELECT @directDynamicSQL = 'INSERT INTO #IntermediatePersonId ' + @directDynamicSQL
	--print @directDynamicSQL
		EXEC sp_executesql @directDynamicSQL
		SELECT @dynamicSQL = 'SELECT fp.PersonId FROM #IntermediatePersonId t INNER JOIN ReadModel.FindPerson fp with (nolock) ON t.PersonId = fp.PersonId WHERE '					
	END
	ELSE
	BEGIN
		SELECT @dynamicSQL = 'SELECT fp.PersonId FROM ReadModel.FindPerson fp with (nolock) WHERE ISNULL(fp.TerminalDate, ''2100-01-01'') >= ''' + @leave_after_ISO + ''' AND ' 
	END

	IF EXISTS (SELECT * FROM #SearchCriteria)
	BEGIN
		SET @stringholder2 = ''
		SET @dynamicSQL = @dynamicSQL
						+ ' ( (fp.StartDateTime IS NULL OR fp.StartDateTime <=  ''' + @belongs_to_date_ISO + ''' ) AND ( fp.EndDateTime IS NULL OR fp.EndDateTime >= ''' + @belongs_to_date_ISO + ''' ))'

		DECLARE SearchCriteriaCursor CURSOR FOR
		SELECT SearchType, SearchValue FROM #SearchCriteria;
		OPEN SearchCriteriaCursor;

		FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue
						
		WHILE @@FETCH_STATUS = 0
		BEGIN					
			IF @stringholder2 <> ''
				SET @stringholder2 = @stringholder2 + ' INTERSECT '

			SET @stringholder = ''			
			SELECT @stringholder= @stringholder+ 'OR fp.SearchValue LIKE N''%' + [string]   + '%''  ' FROM SplitStringByChar(REPLACE(@searchValue, '''', ''''''), ';')
				
			IF @stringholder <> ''
			BEGIN
				SET @stringholder= SUBSTRING(@stringholder, 4, LEN(@stringholder) - 3)
				SET @stringholder2 = @stringholder2 + '(' + @dynamicSQL + ' AND ( ' + @stringholder + ')  AND fp.SearchType = ''' + @searchType + ''' ) ' 
			END
	
			FETCH NEXT FROM SearchCriteriaCursor INTO @searchType, @searchValue
		END

		SET @dynamicSQL = @stringholder2
		CLOSE SearchCriteriaCursor;
		DEALLOCATE SearchCriteriaCursor;	
	END
	ELSE
		SELECT @dynamicSQL = 'SELECT PersonId FROM #IntermediatePersonId'
END
--print @dynamicSQL
--insert into PersonId temp table
INSERT INTO #PersonId
EXEC sp_executesql @dynamicSQL

--prepare restult
INSERT INTO #result
SELECT DISTINCT a.PersonId, FirstName, LastName, EmploymentNumber, Note, TerminalDate, [TeamId], [SiteId], [BusinessUnitId]
FROM ReadModel.FindPerson a
INNER JOIN #PersonId b
ON a.PersonId = b.PersonId
WHERE BusinessUnitId = @business_unit_id
ORDER BY LastName, FirstName

--get total count
DECLARE @total int
SELECT @total = COUNT(*) FROM #result

-- Combine order by string
CREATE TABLE #OrderByStrings(
	OrderByString nvarchar(10),
)

INSERT INTO #OrderByStrings
SELECT * FROM dbo.SplitStringString(@order_by)

DECLARE @colId INT;
DECLARE @direction INT;

DECLARE @fullOrderBy NVARCHAR(500)
SELECT @fullOrderBy = ''

DECLARE @orderByString NVARCHAR(10)
DECLARE OrderByStringCursor CURSOR FOR
SELECT RTRIM(LTRIM(OrderByString)) FROM #OrderByStrings;
OPEN OrderByStringCursor;

FETCH NEXT FROM OrderByStringCursor INTO @orderByString

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @splitterIndex = CHARINDEX(':', @orderByString)
	IF @splitterIndex > 0
	BEGIN
		SELECT @colId = CONVERT(INT, LTRIM(RTRIM(SUBSTRING(@orderByString, 0, @splitterIndex))))
		SELECT @direction = CONVERT(INT, LTRIM(RTRIM(SUBSTRING(@orderByString, @splitterIndex + 1, LEN(@orderByString) - @splitterIndex))))

		SELECT @fullOrderBy = @fullOrderBy + CASE @colId
            WHEN 0 THEN 'PC.FirstName collate ' + @collation
            WHEN 1 THEN 'PC.LastName collate ' + @collation
            WHEN 2 THEN 'PC.EmploymentNumber collate ' + @collation
            WHEN 3 THEN 'PC.Note collate ' + @collation
            ELSE 'CONVERT(varchar(50), PC.TerminalDate, 120) collate ' + @collation
        END + ' ' +
        CASE @direction
            WHEN 1 THEN 'ASC'
            ELSE 'DESC'
        END + ','
	END
	FETCH NEXT FROM OrderByStringCursor INTO @orderByString
END

IF RIGHT(@fullOrderBy, 1) = ','
	SELECT @fullOrderBy = SUBSTRING(@fullOrderBy, 1, LEN(@fullOrderBy) - 1)

--debug
--SELECT @fullOrderBy

CLOSE OrderByStringCursor
DEALLOCATE OrderByStringCursor;
Drop Table #OrderByStrings

SELECT @dynamicSQL='SELECT ' + cast(@total as nvarchar(10)) + ' AS TotalCount, *
    FROM (
    SELECT *, ROW_NUMBER() OVER(ORDER BY ' + @fullOrderBy + ') AS RowNumber
      FROM #result PC) #result WHERE RowNumber >= '+ cast(@start_row as nvarchar(10))
     + ' AND RowNumber < '+ cast(@end_row as nvarchar(10))

--debug
--print @dynamicSQL

--return
EXEC sp_executesql @dynamicSQL

GO