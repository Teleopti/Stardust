IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossDatabaseView_definition]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[sys_crossDatabaseView_definition]
GO
--select * from [mart].[sys_crossDatabaseView_definition]()
CREATE FUNCTION [mart].[sys_crossDatabaseView_definition]
(
)
RETURNS TABLE

AS
RETURN
(
	SELECT 'v_log_object' as 'view_name','SELECT * FROM [$$$target$$$].dbo.log_object' as 'view_definition',4 as 'target_id'
	UNION ALL
	SELECT 'v_agent_logg' as 'view_name','SELECT * FROM [$$$target$$$].dbo.agent_logg' as 'view_definition',4 as 'target_id'
	UNION ALL
	SELECT 'v_agent_info' as 'view_name','SELECT * FROM [$$$target$$$].dbo.agent_info' as 'view_definition',4 as 'target_id'
	UNION ALL
	SELECT 'v_queues' as 'view_name','SELECT * FROM [$$$target$$$].dbo.queues' as 'view_definition',4 as 'target_id'
	UNION ALL
	SELECT 'v_queue_logg' as 'view_name','SELECT * FROM [$$$target$$$].dbo.queue_logg' as 'view_definition',4 as 'target_id'
	UNION ALL
	SELECT 'v_ccc_system_info' as 'view_name','SELECT * FROM [$$$target$$$].dbo.ccc_system_info' as 'view_definition',4 as 'target_id'
	UNION ALL
	SELECT 'v_log_object_detail' as 'view_name','SELECT * FROM [$$$target$$$].dbo.log_object_detail' as 'view_definition',4 as 'target_id'
)
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossDatabaseView_Stubs]') AND TYPE = 'P')
DROP PROCEDURE [mart].[sys_crossDatabaseView_Stubs]
GO

-- =============================================
/*
Author:			DJ
Create date:	2012-05-07
Description:	Implements a stub for every cross database views based

*/

-- =============================================


CREATE PROCEDURE mart.sys_crossDatabaseView_Stubs
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
DECLARE @object_id		AS INT

--INIT
SET @view			= ''
SET @definition		= ''
SET @target			= ''
SET	@object_id		= NULL

--create cursor 
DECLARE ViewCursor CURSOR FOR
	SELECT
		ct.target_customName as target,
		cv.view_name,
		cv.[View_Definition],
		object_id('mart.'+cv.view_name)
	FROM [mart].[sys_crossDatabaseView_definition]() cv
	INNER JOIN mart.sys_crossDatabaseView_target ct
		ON cv.target_id = ct.target_id
	
OPEN ViewCursor 
FETCH NEXT FROM ViewCursor INTO @target,@view,@definition,@object_id
 
--looping through each record 
WHILE @@fetch_status = 0 
BEGIN 

    --Drop view if exist
	SELECT @sql = 'IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N''[mart].['+@view+']'')) DROP VIEW [mart].['+@view+']'
	IF @debug = 1 PRINT @sql 
	ELSE EXEC(@sql)


	--Replace target
	SELECT @definition = REPLACE(@definition,'[$$$target$$$].','')
	
	--debug
	IF @debug = 1 PRINT 'GO'

	--Re-create view
	SELECT @sql = 'CREATE VIEW mart.'+ @view+' as ' + @definition
	IF @debug = 1 PRINT @sql
	ELSE EXEC(@sql)

	--debug
	IF @debug = 1 PRINT 'GO'

FETCH NEXT FROM ViewCursor INTO @target,@view,@definition,@object_id

END


CLOSE ViewCursor
DEALLOCATE ViewCursor

RETURN(0)
GO
EXEC mart.sys_crossDatabaseView_Stubs
GO
