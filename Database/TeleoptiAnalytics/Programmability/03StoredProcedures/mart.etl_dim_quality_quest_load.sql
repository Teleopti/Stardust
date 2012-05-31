IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'mart.etl_dim_quality_quest_load') AND type in (N'P', N'PC'))
DROP PROCEDURE mart.etl_dim_quality_quest_load
GO

-- =============================================
-- Author:		DavidJ
-- Create date: 2012-05-31
-- Description:	Loads quality forms from QM plattform
-- =============================================
--Exec mart.etl_dim_quality_quest_load -2;select * from mart.dim_quality_quest
CREATE PROCEDURE mart.etl_dim_quality_quest_load
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
		EXEC mart.etl_dim_quality_quest_load @datasource_id
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
	SET IDENTITY_INSERT mart.dim_quality_quest ON
	INSERT INTO [mart].[dim_quality_quest]
	(
	   [quality_quest_id]
      ,[quality_quest_agg_id]
      ,[quality_quest_original_id]
      ,[quality_quest_score_weight]
      ,[quality_quest_name]
      ,[quality_quest_type_id]
      ,[log_object_name]
      ,[datasource_id]
      ,[insert_date]
      ,[update_date]
      )
	SELECT
		[quality_quest_id]			= -1,
		[quality_quest_agg_id]		= -1,
		[quality_quest_original_id] = -1,
		[quality_quest_score_weight]= 0.0000,
		[quality_quest_name]		= 'Not Defined',
		[quality_quest_type_id]		= -1,
		[log_object_name]			= 'Not Defined',
		datasource_id				= -1,
		insert_date					= getdate(),
		update_date					= getdate()
	WHERE
		NOT EXISTS (SELECT * FROM mart.dim_quality_quest d WHERE d.quality_quest_id=-1)

	SET IDENTITY_INSERT mart.dim_quality_quest OFF

	-------------
	-- new quality type
	-------------
	INSERT INTO mart.dim_quality_quest
		( 
      [quality_quest_agg_id]
      ,[quality_quest_original_id]
      ,[quality_quest_score_weight]
      ,[quality_quest_name]
      ,[quality_quest_type_id]
      ,[log_object_name]
      ,[datasource_id]
      ,[insert_date]
      ,[update_date]
		)
	SELECT 
		quality_quest_agg_id		= agg.quality_id,
		quality_quest_original_id	= agg.original_id,
		quality_quest_score_weight	= agg.score_weight,
		quality_quest_name			= agg.quality_name,
		quality_quest_type_id		= -1, --not found yet
		log_object_name				= s.log_object_name,
		datasource_id				= s.datasource_id,
		update_date					= GETDATE(),
		insert_date					= GETDATE()
	FROM dbo.quality_info agg --local agg table
	INNER JOIN mart.sys_datasource s
		ON	agg.log_object_id = s.log_object_id
		AND s.datasource_id = @datasource_id
	WHERE --filter out Existing quality_ids
		NOT EXISTS (SELECT quality_quest_id
						FROM mart.dim_quality_quest q
						WHERE	q.quality_quest_agg_id		= agg.quality_id
						AND		q.datasource_id				= @datasource_id
					)

	-----------------
	-- update existing quality quests
	-----------------
	UPDATE mart.dim_quality_quest
	SET 
		quality_quest_agg_id		= agg.quality_id,
		quality_quest_original_id	= agg.original_id,
		quality_quest_score_weight	= agg.score_weight,
		quality_quest_name			= agg.quality_name,
		quality_quest_type_id		= qt.quality_quest_type_id,
		log_object_name				= s.log_object_name,
		datasource_id				= s.datasource_id,
		update_date					= GETDATE()
	FROM mart.dim_quality_quest q
	INNER JOIN dbo.quality_info agg --local agg table
		ON q.quality_quest_agg_id	= agg.quality_id
	INNER JOIN mart.sys_datasource s
		ON	agg.log_object_id = s.log_object_id
		AND s.datasource_id = 4
	INNER JOIN mart.dim_quality_quest_type qt
		ON qt.quality_quest_type_name = agg.quality_type

			
END

GO
