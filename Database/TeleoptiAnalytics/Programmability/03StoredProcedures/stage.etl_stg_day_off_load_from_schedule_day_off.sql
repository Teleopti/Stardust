IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[etl_stg_day_off_load_from_schedule_day_off]') AND type in (N'P', N'PC'))
DROP PROCEDURE [stage].[etl_stg_day_off_load_from_schedule_day_off]
GO

-- =============================================
-- Author:		Jonas N
-- Create date: 2012-11-29
-- Description:	Load stage.stg_day_off from scheduled day offs data.
-- =============================================
CREATE PROCEDURE [stage].[etl_stg_day_off_load_from_schedule_day_off]
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE [stage].[stg_day_off]

	INSERT INTO [stage].[stg_day_off]
		(
		[day_off_name]
		,[day_off_shortname]
		,[business_unit_code]
		,[day_off_code]
		,[display_color]
		,[display_color_html]
		,[datasource_id]
		,[insert_date]
		,[update_date]
		,[datasource_update_date]
		)
	SELECT 
		[day_off_name]			= sdo.day_off_name,
		[day_off_shortname]		= MAX(sdo.day_off_shortname),
		[business_unit_code]	= sdo.business_unit_code,
		[day_off_code]			= NULL,
		[display_color]			= -8355712,
		[display_color_html]	= '#808080',
		[datasource_id]			= MIN(sdo.datasource_id),
		[insert_date]			= GETDATE(),
		[update_date]			= GETDATE(),
		[datasource_update_date]= MAX(sdo.datasource_update_date)
	FROM stage.stg_schedule_day_off_count sdo
	GROUP BY sdo.day_off_name, sdo.business_unit_code
END

GO


