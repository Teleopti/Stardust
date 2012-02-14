IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_datasource_set_raptor_time_zone]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_datasource_set_raptor_time_zone]
GO



SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		JN
-- Create date: 2009-12-04
-- Description:	Set time zone id for 'Raptor' data source.
-- =============================================
CREATE PROCEDURE [mart].[sys_datasource_set_raptor_time_zone]
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @time_zone_id int
	SET @time_zone_id = NULL

	SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = 'UTC'
	IF (SELECT @time_zone_id) IS NOT NULL
	BEGIN
			   UPDATE mart.sys_datasource 
			   SET time_zone_id = @time_zone_id,
					update_date = GETDATE()
			   WHERE datasource_id = 1 --Hardcode value for "Raptor Default"
	END

END

GO


