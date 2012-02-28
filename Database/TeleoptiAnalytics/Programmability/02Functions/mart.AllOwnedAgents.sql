IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[AllOwnedAgents]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[AllOwnedAgents]
GO

-- =============================================
-- Author:		<KJ>
-- Create date: 2008-06-26
--				2012-01-09 Mattias E: Added BU
-- Description:	<Check permissions on Agents/Persons>
------------------------------------------------
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================


CREATE FUNCTION [mart].[AllOwnedAgents](@person_code uniqueidentifier,@report_id uniqueidentifier, @business_unit_code uniqueidentifier = '00000000-0000-0000-0000-000000000000')
RETURNS @agents TABLE (id int NOT NULL)
AS
BEGIN	


/*RETURN ALL PERSONS IN TEAMS PERMITTED TO ME, EXCEPT MYSELF PERMISSION*/
INSERT INTO @agents(id)
SELECT DISTINCT dp.person_id 
FROM mart.permission_report perm
INNER JOIN mart.dim_person dp ON dp.team_id=perm.team_id AND dp.to_be_deleted = 0 --Only valid PersonPeriods
WHERE perm.person_code=@person_code
AND perm.ReportId=@report_id
AND perm.my_own=0 --not myown
AND (dp.business_unit_code = @business_unit_code OR @business_unit_code = '00000000-0000-0000-0000-000000000000')


/*RETURN ALL PERSON_IDS I HAVE BEEN, LOOKING AT MYSELF PERMISSIONS*/
INSERT INTO @agents(id)
SELECT DISTINCT dp.person_id
FROM mart.permission_report perm
INNER JOIN mart.dim_person dp ON dp.person_code=perm.person_code AND dp.to_be_deleted = 0 --Only valid PersonPeriods
WHERE perm.my_own=1 --only myself permissions
AND perm.person_code=@person_code
AND perm.ReportId=@report_id
AND dp.person_id NOT IN (SELECT id FROM @agents)
AND (dp.business_unit_code = @business_unit_code OR @business_unit_code = '00000000-0000-0000-0000-000000000000')
RETURN
END
GO

