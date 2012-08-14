IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_acd_login_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_acd_login_load]
GO

-- =============================================
-- Author:		KJ
-- Create date: 20080408
-- Description:	Loads agents from Teleopti CCC Agg
-- Update date: 2009-02-11
-- 2009-02-11 New Mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 20080414 KJ trim agent_name and add agent_name=Not Defined for agent_id -1
-- 20090708 KJ Added UPDATE for ace_login_name.
-- 2011-07-18 Dual agg databases DJ
-- =============================================
--mart.etl_dim_acd_login_load 1
CREATE PROCEDURE [mart].[etl_dim_acd_login_load]
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
		EXEC [mart].[etl_dim_acd_login_load] @datasource_id
		FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END

ELSE  --We have a single data source id
BEGIN

	--is this an internal or external datasource?
	SELECT @internal = internal FROM mart.sys_datasource WHERE datasource_id = @datasource_id

	--------------
	-- Not Defined 
	--------------
	SET IDENTITY_INSERT mart.dim_acd_login ON
	INSERT INTO mart.dim_acd_login
		(
		acd_login_id,
		acd_login_name,
		datasource_id	
		)
	SELECT 
		acd_login_id		=-1,
		acd_login_name		='Not Defined',
		datasource_id		=-1
	WHERE NOT EXISTS (SELECT * FROM mart.dim_acd_login where acd_login_id = -1)
	SET IDENTITY_INSERT mart.dim_acd_login OFF

	--------------
	-- update changes
	--------------
	--prepare
	SELECT @sqlstring = 'UPDATE mart.dim_acd_login
	SET acd_login_name = ltrim(rtrim(agg.agent_name)),
		update_date = getdate()
	FROM'
	+ CASE @internal
		WHEN 0 THEN '	mart.v_agent_info agg'
		WHEN 1 THEN '	dbo.agent_info agg'
		ELSE NULL --Fail fast
	  END
	+ ' 
	INNER JOIN mart.sys_datasource sys
	ON
		agg.log_object_id = sys.log_object_id	AND
		sys.datasource_id = ' + CAST(@datasource_id as nvarchar(10)) + '
	WHERE 
	agg.agent_id	= mart.dim_acd_login.acd_login_agg_id AND
	ltrim(rtrim(agg.orig_agent_id))	= mart.dim_acd_login.acd_login_original_id collate database_default AND
	sys.datasource_id =  mart.dim_acd_login.datasource_id AND
	acd_login_name <> ltrim(rtrim(agg.agent_name)) collate database_default'
			
	--Exec
	EXEC sp_executesql @sqlstring
	
	-------------
	-- Insert new
	-------------
	--prepare
		SELECT @sqlstring = 'INSERT INTO mart.dim_acd_login
		( 
		acd_login_agg_id,
		acd_login_original_id, 
		acd_login_name, 
		is_active,
		datasource_id,
		time_zone_id
		)
	SELECT 
		acd_login_agg_id		= agg.agent_id, 
		acd_login_original_id	= ltrim(rtrim(agg.orig_agent_id)), 
		acd_login_name			= ltrim(rtrim(agg.agent_name)),
		is_active				= agg.is_active,
		datasource_id			= sys.datasource_id,
		time_zone_id			= sys.time_zone_id
	FROM
		'
		+ CASE @internal
			WHEN 0 THEN '	mart.v_agent_info agg'
			WHEN 1 THEN '	dbo.agent_info agg'
			ELSE NULL --Fail fast
		  END
		+ ' 
		INNER JOIN
		mart.sys_datasource sys
	ON
		agg.log_object_id = sys.log_object_id	AND
		sys.datasource_id = ' + CAST(@datasource_id as nvarchar(10)) + '
	WHERE 
		NOT EXISTS (SELECT acd_login_id FROM mart.dim_acd_login d 
						WHERE	d.acd_login_agg_id= agg.agent_id 	AND
								d.datasource_id =sys.datasource_id
					)'

	--Exec
	EXEC sp_executesql @sqlstring

END
GO