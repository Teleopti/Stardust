IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_log_object_detail]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_log_object_detail]
GO
-- =============================================
-- Author:		RobinK
-- Create date: 2014-10-06
-- Description:	Return the log object details from agg db
-- ---------------------------------------------
-- ChangeLog:
-- Date			Author	Description
-- ---------------------------------------------
-- =============================================
-- EXEC [mart].[raptor_log_object_detail]

CREATE PROCEDURE [mart].[raptor_log_object_detail] 
AS

SELECT
	l.log_object_desc,
	l.log_object_id,
	d.detail_desc,
	d.proc_name,
	DATEADD(mi,d.int_value*(1440/96),d.date_value) AS last_update
FROM mart.v_log_object l
INNER JOIN [mart].[v_log_object_detail] d 
	ON l.log_object_id=d.log_object_id
ORDER BY
	l.log_object_id,
	d.detail_id

GO