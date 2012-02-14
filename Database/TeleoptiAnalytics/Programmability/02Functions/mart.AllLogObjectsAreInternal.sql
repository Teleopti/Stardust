IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[AllLogObjectsAreInternal]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[AllLogObjectsAreInternal]
GO

-- =============================================
-- Author:		David Jonsson
-- Create date: 2011-07-20
-- Description:	This function returns True if all Log Objects Are Internal.
-- Change log:
-- Date			Who		Why
-- 2011-10-07	DavidJ	Because we assumed Analytics by default (initial load), we don't want that just yet
-- =============================================

--SELECT [mart].[AllLogObjectsAreInternal]()
CREATE FUNCTION [mart].[AllLogObjectsAreInternal]()
returns bit
AS
BEGIN
	DECLARE @fixedRows		AS INT
	SET @fixedRows		= 2 --Number of rows in mart.sys_datasource that are not true log ojects => datasource_id not in(1,-1)

	--We have no LogObjects at All (before initial load), then assume there are log objetcs in Agg-database
	IF (select count(*) FROM mart.sys_datasource) = @fixedRows
		RETURN 0
		
	--We have dataSources and all of them are internal
	IF (select count(*) FROM mart.sys_datasource) = (select count(*) + @fixedRows FROM mart.sys_datasource where datasource_id not in(1,-1) and internal = 1)
		RETURN 1
		
	--Else, go for the classic way (assume there are log objetcs in Agg-database)
	RETURN 0
END

GO