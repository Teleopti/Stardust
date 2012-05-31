IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'mart.etl_dim_quality_quest_type_load') AND type in (N'P', N'PC'))
DROP PROCEDURE mart.etl_dim_quality_quest_type_load
GO

-- =============================================
-- Author:		DavidJ
-- Create date: 2012-05-31
-- Description:	Loads quality type from QM plattform
-- =============================================
--Exec mart.etl_dim_quality_quest_type_load -2
CREATE PROCEDURE mart.etl_dim_quality_quest_type_load
@datasource_id smallint
	
AS

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
		EXEC [mart].[etl_dim_quality_quest_type_load] @datasource_id
		FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END
ELSE  --Single datasource_id
BEGIN

	--------------
	-- Not Defined
	--------------
	SET IDENTITY_INSERT mart.dim_quality_quest_type ON
	INSERT INTO [mart].[dim_quality_quest_type]
	(
		quality_quest_type_id,
		quality_quest_type_name,
		datasource_id,
		insert_date,
		update_date
	)
	SELECT DISTINCT
		quality_quest_type_id		= -1,
		quality_quest_type_name		= 'Not Defined',
		datasource_id				= -1,
		insert_date					= getdate(),
		update_date					= getdate()
	FROM [mart].[dim_quality_quest_type]
	WHERE
		NOT EXISTS (SELECT d.quality_quest_type_id FROM mart.dim_quality_quest_type d WHERE d.quality_quest_type_id=-1)

	SET IDENTITY_INSERT mart.dim_quality_quest_type OFF

	-----------------
	-- update existing quality type
	-----------------
	UPDATE mart.dim_quality_quest_type
	SET 
		quality_quest_type_name	= agg.quality_type,
		update_date				= GETDATE()
	FROM dbo.quality_info agg --local agg table
	INNER JOIN
		mart.sys_datasource sys
	ON
		agg.log_object_id = sys.log_object_id	AND
		sys.datasource_id = @datasource_id
	WHERE 
		mart.dim_quality_quest_type.quality_quest_type_name	= agg.quality_type	AND
		mart.dim_quality_quest_type.datasource_id	= sys.datasource_id

	-------------
	-- new quality type
	-------------
	INSERT INTO mart.dim_quality_quest_type
		( 
		quality_quest_type_name,
		insert_date,
		update_date,
		datasource_id
		)
	SELECT 
		quality_quest_type_name	= agg.quality_type,
		insert_date				= GETDATE(),
		update_date				= GETDATE(),
		datasource_id			= sys.datasource_id
	FROM dbo.quality_info agg --local agg table
	INNER JOIN
		mart.sys_datasource sys
	ON
		agg.log_object_id = sys.log_object_id	AND
		sys.datasource_id = @datasource_id
	WHERE --filter out Existing types
		NOT EXISTS (SELECT quality_quest_type_id FROM mart.dim_quality_quest_type d 
						WHERE	d.quality_quest_type_name	= agg.quality_type
						AND		d.datasource_id = sys.datasource_id
					)

END

GO

