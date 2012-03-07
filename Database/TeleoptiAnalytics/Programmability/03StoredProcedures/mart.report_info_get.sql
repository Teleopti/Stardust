IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_info_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_info_get]
GO


CREATE PROCEDURE [mart].[report_info_get] 
/*
	Skapad 2004-05-03
	Av Ola
	Används: Vid skapande av rapport hämtas först info om vilken eller vilka stored
	procedures som ska köras och om det finns subrapporter etc.

	Ola 2008-05-13 Added url to output
20090211 Added new mart schema KJ
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
exec mart.report_info_get '482D0EE3-1D55-44D5-828E-15DEED5230CE'
*/
@report_id uniqueidentifier

AS

SELECT rpt_file_name, url, proc_name, sub1_name, sub1_proc_name, sub2_name, sub2_proc_name, report_name, report_name_resource_key, help_key
FROM mart.v_report
WHERE Id = @report_id

GO

