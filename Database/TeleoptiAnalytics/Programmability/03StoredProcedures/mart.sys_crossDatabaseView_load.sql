IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossDatabaseView_load]') AND TYPE = 'P')
DROP PROCEDURE [mart].[sys_crossDatabaseView_load]
GO

-- =============================================
/*
Author:			DJ
Create date:	2008-09-30
Description:	Implements all cross database views based on sys_crossDatabaseView_target and sys_crossDatabaseView

This procedure is used in order handle customers with custom names on the teleopti databases.

sys_crossDatabaseView_load
SELECT * FROM sys_crossDatabaseView
*/

-- =============================================


CREATE PROCEDURE mart.sys_crossDatabaseView_load
			@debug bit = 0
AS
SET NOCOUNT ON

--DECLARE
DECLARE @sql			AS VARCHAR(4000)
DECLARE @view			AS VARCHAR(100)
DECLARE @definition		AS VARCHAR(3800)
DECLARE @target			AS VARCHAR(100)
DECLARE @allInternal	AS BIT
DECLARE @fixedRows		AS INT

--INIT
SET @view			= ''
SET @definition		= ''
SET @target			= ''

--create cursor 
DECLARE ViewCursor CURSOR FOR
SELECT
a.target,
a.view_name,
a.view_definition
FROM (
	SELECT
		ct.target_customName as target,
		cv.view_name,
		cv.[View_Definition],
		0 as 'IsCustom'
	FROM mart.sys_crossDatabaseView cv
	INNER JOIN mart.sys_crossDatabaseView_target ct
		ON cv.target_id = ct.target_id
	WHERE ct.confirmed = 1

	UNION ALL

	SELECT
		ct.target_customName as target,
		cv.view_name,
		cv.[View_Definition],
		1 as 'IsCustom'
	FROM mart.sys_crossdatabaseview_custom cv
	INNER JOIN mart.sys_crossDatabaseView_target ct
		ON cv.target_id = ct.target_id
	WHERE ct.confirmed = 1
	) a
ORDER BY a.IsCustom --make sure custom views come _after_ standard. In that case custom will replace standard (given the same name)

OPEN ViewCursor 
FETCH NEXT FROM ViewCursor INTO @target,@view,@definition
 
--looping through each record 
WHILE @@fetch_status = 0 
BEGIN 

	--debug
	IF @debug = 1 PRINT 'GO'

    --Drop view if exist
	SELECT @sql = 'IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N''[mart].['+@view+']'')) DROP VIEW [mart].['+@view+']'
	IF @debug = 1 PRINT @sql 
	ELSE EXEC(@sql)

	--Replace target
	IF (dbo.IsAzureDB() = 1 OR mart.AllLogObjectsAreInternal() = 1)
		SELECT @definition = REPLACE(@definition,'[$$$target$$$].','')
	ELSE
		SELECT @definition = REPLACE(@definition,'$$$target$$$',@target)
	
	--debug
	IF @debug = 1 PRINT 'GO'

	--Re-create view
	SELECT @sql = 'CREATE VIEW mart.'+ @view+' as ' + @definition
	IF @debug = 1 PRINT @sql
	ELSE EXEC(@sql)

FETCH NEXT FROM ViewCursor INTO @target,@view,@definition
END


CLOSE ViewCursor
DEALLOCATE ViewCursor

RETURN(0)
