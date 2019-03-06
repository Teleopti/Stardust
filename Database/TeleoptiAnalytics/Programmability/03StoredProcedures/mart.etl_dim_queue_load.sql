IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_queue_load_identity_on]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_queue_load_identity_on]
GO

CREATE PROCEDURE [mart].[etl_dim_queue_load_identity_on]
WITH EXECUTE AS OWNER
AS
BEGIN
	--------------
	-- Not Defined
	--------------
	SET IDENTITY_INSERT mart.dim_queue ON
	INSERT INTO mart.dim_queue
		(
		queue_id,
		datasource_id	
		)
	SELECT 
		queue_id			=-1,
		datasource_id		=-1
	WHERE NOT EXISTS (SELECT * FROM mart.dim_queue where queue_id = -1)
	SET IDENTITY_INSERT mart.dim_queue OFF
END
GO



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_queue_load]
GO

-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Description:	Loads queues from Teleopti CCC agg.

-- Change Log
-- Date			Author	Description
-- 2009-02-11	KJ		New mart schema
-- 2008-11-05	DJ		Use the column [display_desc] from Agg.mart.queues instead of [org_desc]
-- 2009-02-09	KJ		Stage moved to mart db, removed view
-- 2009-07-02	DJ		Re-factor of dim_queue
-- 2009-09-01	DJ		Exclude default queue from load
-- 2011-07-18	DJ		This SP becomes a wrapper for etl_dim_queue_load_internal + etl_dim_queue_load_external
-- 2011-10-20	DJ		No queues loaded due to typ0
-- =============================================
--Exec [mart].[etl_dim_queue_load] -2
CREATE PROCEDURE [mart].[etl_dim_queue_load]
@datasource_id smallint
AS
DECLARE @internal bit
DECLARE @sqlstring nvarchar(4000)
SET @sqlstring = ''

--------------------------------------------------------------------------
--If we get All = -2 loop existing log objects and call this SP in a cursor for each log object
--------------------------------------------------------------------------
IF @datasource_id = -2 --All
BEGIN
	DECLARE DataSouceCursor CURSOR FOR
	SELECT datasource_id FROM mart.sys_datasource WHERE datasource_id NOT IN (-1,1) AND time_zone_id IS NOT NULL AND inactive = 0 
	OPEN DataSouceCursor

	FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC [mart].[etl_dim_queue_load] @datasource_id
		FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END
ELSE  --Single datasource_id
BEGIN

	--init
	SELECT @internal = internal FROM mart.sys_datasource WHERE datasource_id = @datasource_id

	--default row
	EXEC [mart].[etl_dim_queue_load_identity_on]
	
	-----------------
	-- update changes
	-----------------
	--prepare
	SELECT @sqlstring = 'UPDATE mart.dim_queue
	SET 
		queue_original_id	= orig_queue_id, 
		queue_name			= orig_desc,
		queue_description	= display_desc,
		log_object_name		= sys.log_object_name, 
		update_date			= getdate()
	FROM
		'
		+ CASE @internal
			WHEN 0 THEN '	mart.v_queues agg'
			WHEN 1 THEN '	dbo.queues agg'
			ELSE NULL --Fail fast
		  END
		+ ' 
	INNER JOIN
		mart.sys_datasource sys
	ON
		agg.log_object_id = sys.log_object_id	AND
		sys.datasource_id = ' + CAST(@datasource_id as nvarchar(10)) + '
	WHERE 
		mart.dim_queue.queue_agg_id		= agg.queue			AND
		mart.dim_queue.datasource_id	= sys.datasource_id'
		
	---Exec
	EXEC sp_executesql @sqlstring

	-------------
	-- new queues
	-------------
	--prepare
	SELECT @sqlstring = 'INSERT INTO mart.dim_queue
		( 
		queue_agg_id,
		queue_original_id, 
		queue_name, 
		queue_description,
		log_object_name,
		datasource_id
		)
	SELECT 
		queue_agg_id			= agg.queue, 
		queue_original_id		= agg.orig_queue_id, 
		queue_name				= agg.orig_desc,
		queue_description		= agg.display_desc,
		log_object_name			= sys.log_object_name,
		datasource_id			= sys.datasource_id
	FROM '
		+ CASE @internal
			WHEN 0 THEN '	mart.v_queues agg'
			WHEN 1 THEN '	dbo.queues agg'
			ELSE NULL --Fail fast
		  END
		+ ' 
	INNER JOIN
		mart.sys_datasource sys
	ON
		agg.log_object_id = sys.log_object_id	AND
		sys.datasource_id = ' + CAST(@datasource_id as nvarchar(10)) + '
	WHERE --filter out Existing queues
		NOT EXISTS (SELECT queue_id FROM mart.dim_queue d 
						WHERE	d.queue_agg_id	= agg.queue
						AND		d.datasource_id = sys.datasource_id
					)
	AND  --filter out Exluded queues
		NOT EXISTS (SELECT queue_original_id FROM mart.dim_queue_excluded exl
						WHERE	exl.queue_original_id	= agg.orig_queue_id COLLATE DATABASE_DEFAULT
						AND		exl.datasource_id		= sys.datasource_id
					)
	AND  --exclude queues with null values in agg for orig_queue_id and orig_desc
		(
			agg.orig_queue_id IS NOT NULL
			AND
			agg.orig_desc IS NOT NULL
		)'

	---Exec
	EXEC sp_executesql @sqlstring

END

GO
