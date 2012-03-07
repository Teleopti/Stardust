IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_user_setting_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_user_setting_get]
GO


--select * from t_webx2_user_setting
-- p_webx2_get_user_setting 1,   1025,1

CREATE     PROCEDURE [mart].[report_user_setting_get]
/*
	Ola 2004-10-26
	Laddar sparade inställningar för en rapport
	Om den inte hämtar en sparad namngiven uppsättning
	skickas -1 in i @saved_name_id
	Det ska vid en installation alltid finnas en sådan default som inte egentligen visas.
	200090211 Added new mart schema KJ
	-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/

	@param_name varchar(50),
	@report_id uniqueidentifier,
	@person_code uniqueidentifier,
	@saved_name_id int

AS


SELECT control_setting
FROM mart.report_user_setting
WHERE ReportId = @report_id
AND param_name = @param_name 
AND person_code = @person_code
AND saved_name_id = @saved_name_id

GO

