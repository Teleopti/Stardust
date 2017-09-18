IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_time_zone_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_time_zone_get]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		JN
-- Create date: 2009-12-03
-- Description:	Get all time zones
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_time_zone_get] 
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		time_zone_id,
		time_zone_code,
		time_zone_name,
		default_zone,
		utc_conversion,
		utc_conversion_dst,
		utc_in_use
	FROM mart.dim_time_zone

END

GO


