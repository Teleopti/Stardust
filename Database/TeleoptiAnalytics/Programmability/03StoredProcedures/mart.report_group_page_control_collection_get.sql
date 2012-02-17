IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_group_page_control_collection_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_group_page_control_collection_get]
GO


-- =============================================
-- Author:		Jonas N
-- Create date: 2010-09-04
-- Description:	Get a control_collection_id for the given report.
-- =============================================
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- EXEC mart.report_group_page_control_collection_get 'A9718D69-77A9-4D1D-9D44-DBA7EA7E92F5'
CREATE PROCEDURE [mart].[report_group_page_control_collection_get]
@report_id uniqueidentifier
AS
SET NOCOUNT ON;

SELECT
	cc.control_collection_id
FROM 
	mart.v_report_control_collection cc
INNER JOIN
	mart.report r
ON
	cc.CollectionId = r.ControlCollectionId
WHERE
	r.Id = @report_id
	AND cc.ControlId = 'A9718D69-77A9-4D1D-9D44-DBA7EA7E92F5' --29

GO


