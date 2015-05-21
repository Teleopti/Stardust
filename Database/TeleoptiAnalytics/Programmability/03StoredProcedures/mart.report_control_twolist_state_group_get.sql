IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_twolist_state_group_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_twolist_state_group_get]
GO


--[mart].[report_control_twolist_state_group_get] NULL,NULL,NULL,'928DD0BC-BF40-412E-B970-9B5E015AADEA'

-- =============================================
-- Author:		KJ
-- Create date: 2013-11-07
-- Description:	Loads state groups into report control
-- Change Log
-- Date			Author	Description
-- =============================================
CREATE PROCEDURE [mart].[report_control_twolist_state_group_get]
@report_id uniqueidentifier,
@person_code uniqueidentifier, -- user 
@language_id int,	-- t ex.  1053 = SV-SE
@bu_id uniqueidentifier
AS

CREATE TABLE #state_groups (counter int identity(1,1),id int, name nvarchar(50)) 

INSERT #state_groups(id,name)
SELECT
	id		= state_group_id,
	name	=state_group_name
FROM
	mart.dim_state_group d
INNER JOIN 
	mart.dim_business_unit b
ON 
	b.business_unit_id=d.business_unit_id
WHERE state_group_id<>-1
AND b.business_unit_code=@bu_id 
ORDER BY state_group_name

SELECT id,name
FROM #state_groups
ORDER BY counter

