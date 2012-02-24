IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_absence_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_absence_get]
GO

--exec report_control_twolist_absence_get 15,'C04803E2-8D6F-4936-9A90-9B2000148778',1053,'4AD43E49-B233-4D03-A813-9B2000102EBE'
-- =============================================
-- Author:		unknonw
-- Create date: 2008-..-..
-- Description:	Loads absence into report control

-- Change Log
-- Date			Author	Description
-- 20100727		DJ		Added hard coded filter: is_deleted
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
CREATE PROC [mart].[report_control_twolist_absence_get]
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS

CREATE TABLE #absences (counter int identity(1,1),id int, name nvarchar(50)) 

INSERT #absences(id,name)
SELECT
	id		= absence_id,
	name	= absence_name
FROM
	mart.dim_absence d
INNER JOIN 
	mart.dim_business_unit b
ON 
	b.business_unit_id=d.business_unit_id
WHERE absence_id<>-1
AND b.business_unit_code=@bu_id --20080910 bara det bu som man använder just nu i raptor
AND d.is_deleted = 0 --2010-07-27 Show only active absences
ORDER BY absence_name

SELECT id,name
FROM #absences
ORDER BY counter


GO

