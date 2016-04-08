IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[AllOwnedSites]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[AllOwnedSites]
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- 2013-02-07 #22206 - Implement faster persmission load (view) - David
-- 2013-05-06 Bck to the "old" way - Ola
-- =============================================


CREATE FUNCTION [mart].[AllOwnedSites](@person_code uniqueidentifier,@report_id uniqueidentifier)
RETURNS @sites TABLE (id int NOT NULL)
AS
BEGIN	

/*RETURN ALL TEAMS PERMITTED TO ME, EXCEPT MYSELF PERMISSION*/
INSERT INTO @sites(id)
SELECT DISTINCT ds.site_id 
FROM mart.permission_report perm WITH (NOLOCK)
INNER JOIN mart.dim_team dt WITH (NOLOCK)
	ON dt.team_id=perm.team_id
INNER JOIN mart.dim_site ds WITH (NOLOCK)
	ON ds.site_id=dt.site_id
WHERE person_code=@person_code
AND perm.ReportId=@report_id
AND perm.my_own=0 --not myown

--select * from dim_team
/*RETURN ALL TEAMS I HAVE BELONGED TO, LOOKING AT MYSELF PERMISSIONS*/
INSERT INTO @sites(id)
SELECT DISTINCT dt.site_id
FROM mart.permission_report perm WITH (NOLOCK)
INNER JOIN mart.dim_person dp WITH (NOLOCK)
	ON dp.person_code=perm.person_code 
	AND dp.to_be_deleted = 0 --Only valid PersonPeriods
INNER JOIN mart.dim_team dt WITH (NOLOCK)
	ON dt.team_id=dp.team_id
INNER JOIN mart.dim_site ds WITH (NOLOCK)
	ON ds.site_id=dt.site_id
WHERE perm.my_own=1 --only myself permissions
AND perm.person_code=@person_code
AND perm.ReportId=@report_id
AND ds.site_id NOT IN (SELECT id FROM @sites)
RETURN
END
GO

