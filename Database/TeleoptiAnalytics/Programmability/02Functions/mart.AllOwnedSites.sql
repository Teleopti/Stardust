IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[AllOwnedSites]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[AllOwnedSites]
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================


CREATE FUNCTION [mart].[AllOwnedSites](@person_code uniqueidentifier,@report_id uniqueidentifier)
RETURNS @sites TABLE (id int NOT NULL)
AS
BEGIN	
/*declare @person_code uniqueidentifier
set @person_code='478E8CFB-5B92-4049-95FF-9ABE00250898'*/

--SELECT * FROM mart.permission_report_data_test WHERE person_code=@person_code

/*RETURN ALL TEAMS PERMITTED TO ME, EXCEPT MYSELF PERMISSION*/
INSERT INTO @sites(id)
SELECT DISTINCT ds.site_id 
FROM mart.permission_report perm
INNER JOIN mart.dim_team dt ON
	dt.team_id=perm.team_id
INNER JOIN mart.dim_site ds 
	ON ds.site_id=dt.site_id
WHERE person_code=@person_code
AND perm.ReportId=@report_id
AND perm.my_own=0 --not myown

--select * from dim_team
/*RETURN ALL TEAMS I HAVE BELONGED TO, LOOKING AT MYSELF PERMISSIONS*/
INSERT INTO @sites(id)
SELECT DISTINCT dt.site_id
FROM mart.permission_report perm
INNER JOIN mart.dim_person dp ON dp.person_code=perm.person_code AND dp.to_be_deleted = 0 --Only valid PersonPeriods
INNER JOIN mart.dim_team dt ON 
	dt.team_id=dp.team_id
INNER JOIN mart.dim_site ds 
	ON ds.site_id=dt.site_id
WHERE perm.my_own=1 --only myself permissions
AND perm.person_code=@person_code
AND perm.ReportId=@report_id
AND ds.site_id NOT IN (SELECT id FROM @sites)
RETURN
END
GO

