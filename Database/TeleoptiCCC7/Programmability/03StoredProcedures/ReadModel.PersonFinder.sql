IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[PersonFinder]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[PersonFinder]
GO

-- exec [ReadModel].PersonFinder 'b', 'All', '2001-01-01', 1, 100, 1, 1, 1053
-- exec sp_executesql N'exec [ReadModel].PersonFinder @search_string=@p0, @search_type=@p1, @leave_after=@p2, @start_row =@p3, @end_row=@p4, @order_by=@p5, @sort_direction=@p6, @culture=@p7',N'@p0 nvarchar(4000),@p1 nvarchar(4000),@p2 datetime,@p3 int,@p4 int,@p5 int,@p6 int,@p7 int',@p0=N'2011',@p1=N'All',@p2='2015-07-01 00:00:00',@p3=1,@p4=11,@p5=0,@p6=0,@p7=2057
-- =============================================
-- Author:		Ola
-- Create date: 2011-12-19
-- Description:	Gets the person(s) having all search word specified in the ReadModel (AND-search)
-- Change Log
------------------------------------------------
-- When			Who		What
-- 2012-01-10	David	Re-factored some to get the AND-search
-- =============================================
CREATE PROCEDURE [ReadModel].[PersonFinder]
@search_string nvarchar(max),
@search_type nvarchar(200),
@leave_after datetime,
@start_row int,
@end_row int, 
@order_by int, 
@sort_direction int,
@culture int
AS
SET NOCOUNT ON

--if empty input, then RETURN
IF @search_string = '' RETURN

--declare
DECLARE @leave_after_ISO nvarchar(10)
DECLARE @dynamicSQL nvarchar(4000)
DECLARE @eternityDate datetime
DECLARE @cursorString nvarchar(50)
DECLARE @cursorCount int
DECLARE @countWords int
DECLARE @totalRows int
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
CREATE TABLE #PersonId (PersonId  uniqueidentifier)

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

--re-init @dynamicSQL
SELECT @dynamicSQL=''

-- Get searchString into temptable
SELECT @search_string = REPLACE(@search_string,' ', ',') --separated by space from client
SELECT @search_string = REPLACE(@search_string,'%', '[%]') --make '%' valuable search value
INSERT INTO #strings
SELECT * FROM dbo.SplitStringString(@search_string)

--count number of SearchWord
SELECT @countWords = COUNT(SearchWord)
FROM #strings

--convert @leave_after to ISO-format string
SELECT @leave_after_ISO   = CONVERT(NVARCHAR(10), @leave_after,120)

------------
--cursor for adding "AND" or "OR" conditions for each search string
------------
DECLARE SearchWordCursor CURSOR FOR
SELECT SearchWord, ROW_NUMBER() OVER(ORDER BY SearchWord DESC) as RowNum FROM #strings;
OPEN SearchWordCursor;

FETCH NEXT FROM SearchWordCursor INTO @cursorString, @cursorCount

WHILE @@FETCH_STATUS = 0
 BEGIN
	
	SELECT @dynamicSQL = @dynamicSQL + 'SELECT DISTINCT PersonId, FirstName, LastName, EmploymentNumber, Note, TerminalDate, TeamId, SiteId, BusinessUnitId FROM ReadModel.FindPerson WHERE ISNULL(TerminalDate, ''2100-01-01'') >= '''+ @leave_after_ISO + ''' AND SearchValue like N''%' + @cursorString + '%'''

	--If 'All' set searchtype as a separate AND condition
	IF @search_type <> 'All'
	SELECT @dynamicSQL = @dynamicSQL + ' AND (SearchType = '''' OR SearchType = '''+ @search_type + ''')'

	--add INTERSECT between each result set
	IF @cursorCount <> @countWords --But NOT on last condition, the syntax is different
	SELECT @dynamicSQL = @dynamicSQL + ' INTERSECT '
      
	--fectch next
	FETCH NEXT FROM SearchWordCursor INTO @cursorString, @cursorCount
 END
 
CLOSE SearchWordCursor;
DEALLOCATE SearchWordCursor;

--debug
--print @dynamicSQL

--insert into #Result directly instead of having a second query that opens up for duplicate results and deadlocks
INSERT INTO #Result
EXEC sp_executesql @dynamicSQL

--get total count
DECLARE @total int 
SELECT @total = COUNT(*) FROM #result

SELECT @dynamicSQL=''

SELECT @dynamicSQL='SELECT ' + cast(@total as nvarchar(10)) + ' AS TotalCount, *
    FROM (
    SELECT    *, ROW_NUMBER() OVER(
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
        ' FROM    #result PC) #result WHERE  RowNumber >= '+ cast(@start_row as nvarchar(10)) +' AND RowNumber < '+ cast(@end_row as nvarchar(10))

--debug
--print @dynamicSQL

--return
EXEC sp_executesql @dynamicSQL   
GO
