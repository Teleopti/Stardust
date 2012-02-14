IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_overtime_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_overtime_get]
GO

--exec report_control_twolist_overtime_get 15,'C04803E2-8D6F-4936-9A90-9B2000148778',1053,'4AD43E49-B233-4D03-A813-9B2000102EBE'
-- =============================================
-- Author:		DJ
-- Create date: 2011-02-28
-- Description:	Loads overtime into report control

-- Change Log
-- Date			Author	Description
-- 20110228		DJ		Intial version
-- =============================================
CREATE PROC [mart].[report_control_twolist_overtime_get]
@report_id int,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS

CREATE TABLE #overtime (counter int identity(1,1),id int, name nvarchar(50)) 

INSERT #overtime(id,name)
SELECT
	id		= overtime_id,
	name	= overtime_name
FROM
	mart.dim_overtime d
INNER JOIN 
	mart.dim_business_unit b
ON 
	b.business_unit_id=d.business_unit_id
WHERE overtime_id<>-1
AND b.business_unit_code=@bu_id
AND d.is_deleted = 0
ORDER BY overtime_name

SELECT id,name
FROM #overtime
ORDER BY counter


GO

