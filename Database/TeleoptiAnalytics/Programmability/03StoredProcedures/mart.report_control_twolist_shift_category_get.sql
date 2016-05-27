IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_shift_category_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_shift_category_get]
GO
--[mart].[report_control_twolist_shift_category_get] NULL, NULL, NULL,'928DD0BC-BF40-412E-B970-9B5E015AADEA'

-- =============================================
-- Author:		unknown
-- Create date: 2008-..-..
-- Description:	Loads absence into report control

-- Change Log
-- Date			Author	Description
-- 20100727		DJ		Added hard coded filter: is_deleted
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
CREATE PROCEDURE [mart].[report_control_twolist_shift_category_get]
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS

CREATE TABLE #categories (counter int identity(1,1),id int, name nvarchar(50)) 

INSERT #categories(id,name)
SELECT
	id		= shift_category_id,
	name	= shift_category_name
FROM
	mart.dim_shift_category d WITH(NOLOCK)
INNER JOIN 
	mart.dim_business_unit b WITH(NOLOCK)
ON 
	b.business_unit_id=d.business_unit_id
WHERE shift_category_id<>-1
AND b.business_unit_code=@bu_id
AND d.is_deleted = 0 --2010-07-27 Only Active shift Categories
ORDER BY shift_category_name

SELECT id,name
FROM #categories
ORDER BY counter

