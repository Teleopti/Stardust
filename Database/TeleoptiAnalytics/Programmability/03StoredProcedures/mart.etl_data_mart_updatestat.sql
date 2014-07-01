IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_data_mart_updatestat]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_data_mart_updatestat]
GO

-- =============================================
-- Author:		David J
-- Create date: 2014-07-01
-- Description:	Try cherry pick some tables that need update_stats more often then SQL Server predicts
-- =============================================
--[mart].[etl_data_mart_updatestat] @debug=1
CREATE PROCEDURE [mart].[etl_data_mart_updatestat]
@debug bit = 0
WITH EXECUTE AS OWNER
AS

/* Example for how to add a table to the jobstep
INSERT INTO [mart].[sys_updatestat_tables](table_schema,table_name,options)
SELECT 'mart','fact_schedule',NULL
WHERE NOT EXISTS (select * FROM [mart].[sys_updatestat_tables] WHERE table_schema = 'mart' AND table_name='fact_schedule')
*/

BEGIN
	SET NOCOUNT ON

	DECLARE updatestats CURSOR FOR
	SELECT upds.table_schema, upds.table_name, ISNULL(upds.options,'SAMPLE 25 PERCENT')
	FROM information_schema.tables t
	INNER JOIN [mart].[sys_updatestat_tables] upds
		ON upds.table_schema = t.table_schema
		AND upds.table_name	 = t.table_name
	WHERE t.TABLE_TYPE = 'BASE TABLE'

	OPEN updatestats

	DECLARE @tableSchema NVARCHAR(128)
	DECLARE @tableName NVARCHAR(128)
	DECLARE @Statement NVARCHAR(600)
	DECLARE @options NVARCHAR(200)

	FETCH NEXT FROM updatestats INTO @tableSchema, @tableName, @options

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @Statement = 'UPDATE STATISTICS '  + '[' + @tableSchema + ']' + '.' + '[' + @tableName + ']' + ' WITH ' + @options
		IF @debug=1
			PRINT @Statement
		ELSE
			EXEC sp_executesql @Statement
		FETCH NEXT FROM updatestats INTO @tableSchema, @tableName, @options
	END

	CLOSE updatestats
	DEALLOCATE updatestats

	SET NOCOUNT OFF

END

GO

