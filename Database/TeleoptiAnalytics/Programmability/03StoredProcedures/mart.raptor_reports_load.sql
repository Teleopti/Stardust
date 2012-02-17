IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_reports_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_reports_load]
GO

CREATE PROCEDURE [mart].[raptor_reports_load] 
                            
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id as ReportId,
        CASE 
			WHEN report_name_resource_key = '' THEN  report_name
			ELSE 'xx' + report_name_resource_key 
		END 
		as ReportName 
        , 
        url as ReportUrl,
        target as TargetFrame
    FROM mart.v_report
	WHERE visible = 1
END
GO

