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
SELECT * FROM mart.sys_crossDatabaseView
*/

-- =============================================
CREATE PROCEDURE mart.sys_crossDatabaseView_load
			@debug bit = 0
AS
SET NOCOUNT ON

--DECLARE
DECLARE @sql			AS VARCHAR(4000)
DECLARE @view			AS VARCHAR(100)
DECLARE @definition		AS VARCHAR(1000)
DECLARE @target			AS VARCHAR(100)
DECLARE @allInternal	AS BIT
DECLARE @fixedRows		AS INT

--INIT
SET @view			= ''
SET @definition		= ''
SET @target			= ''

DECLARE @crossDatabaseView TABLE (view_name varchar(100) not null, view_definition varchar(4000) not null,target_id int)
INSERT INTO @crossDatabaseView
SELECT 'v_log_object','SELECT * FROM [$$$target$$$].dbo.log_object',4
UNION ALL
SELECT 'v_agent_logg','SELECT * FROM [$$$target$$$].dbo.agent_logg WITH (NOLOCK)',4
UNION ALL
SELECT 'v_agent_info','SELECT * FROM [$$$target$$$].dbo.agent_info',4
UNION ALL
SELECT 'v_queues','SELECT * FROM [$$$target$$$].dbo.queues',4
UNION ALL
SELECT 'v_queue_logg','SELECT * FROM [$$$target$$$].dbo.queue_logg  WITH (NOLOCK)',4
UNION ALL
SELECT 'v_ccc_system_info','SELECT * FROM [$$$target$$$].dbo.ccc_system_info',4
UNION ALL
SELECT 'v_log_object_detail','SELECT * FROM [$$$target$$$].dbo.log_object_detail',4

--create cursor 
DECLARE ViewCursor CURSOR FOR
	SELECT
		ct.target_customName as target,
		cv.view_name,
		cv.[View_Definition]
	FROM @crossDatabaseView cv
	INNER JOIN mart.sys_crossDatabaseView_target ct
		ON cv.target_id = ct.target_id
	WHERE ct.confirmed = 1
	
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
GO


