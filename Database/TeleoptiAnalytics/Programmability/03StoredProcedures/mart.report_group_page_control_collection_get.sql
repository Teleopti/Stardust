IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_group_page_control_collection_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_group_page_control_collection_get]
GO


-- =============================================
-- Author:		Jonas N
-- Create date: 2010-09-04
-- Description:	Get a control_collection_id for the given report.
-- =============================================
-- EXEC mart.report_group_page_control_collection_get 1
CREATE PROCEDURE [mart].[report_group_page_control_collection_get]
@report_id int
AS
SET NOCOUNT ON;

SELECT
	cc.control_collection_id
FROM 
	mart.report_control_collection cc
INNER JOIN
	mart.report r
ON
	cc.collection_id = r.control_collection_id
WHERE
	r.report_id = @report_id
	AND cc.control_id = 29

GO


