IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_activity_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_activity_get]
GO

--[mart].[report_control_twolist_activity_get] NULL,NULL,NULL,'928DD0BC-BF40-412E-B970-9B5E015AADEA'

-- =============================================
-- Author:		unknown
-- Create date: 2008-..-..
-- Description:	Loads absence into report control

-- Change Log
-- Date			Author	Description
-- 20100727		DJ		Added hard coded filter: is_deleted
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
CREATE PROC [mart].[report_control_twolist_activity_get]
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS

CREATE TABLE #activities (counter int identity(1,1),id int, name nvarchar(50)) 

INSERT #activities(id,name)
SELECT
	id		= activity_id,
	name	=activity_name
FROM
	mart.dim_activity d
INNER JOIN 
	mart.dim_business_unit b
ON 
	b.business_unit_id=d.business_unit_id
WHERE activity_id<>-1
AND b.business_unit_code=@bu_id --20080910 bara det bu som man använder just nu i raptor
AND d.is_deleted = 0 --2010-07-27 Only active activities
ORDER BY activity_name

SELECT id,name
FROM #activities
ORDER BY counter

GO

