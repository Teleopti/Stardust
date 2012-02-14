IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_get_timezones_used_by_datasource]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_get_timezones_used_by_datasource]
GO


-- =============================================
-- Author:		JN
-- Create date: 2011-08-23
-- Description:	Returns all time zones used by datasources except for Null values.
--
-- ChangeLog
-- Date			By	Description
---==============================================
-- 20xx-xx-xx	xx	Changed yadayadayada...
-- =============================================
CREATE PROCEDURE [mart].[sys_get_timezones_used_by_datasource]
AS
BEGIN
	
	SET NOCOUNT ON;	

	SELECT
		t.time_zone_code
	FROM 
		mart.sys_datasource d
	INNER JOIN
		mart.dim_time_zone t
	ON
		d.time_zone_id = t.time_zone_id
	WHERE
		d.time_zone_id IS NOT NULL
END


GO

