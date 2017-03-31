IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[AllOwnedTeams]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[AllOwnedTeams]
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- 2013-02-07 #22206 - Implement faster persmission load (view) - David
-- 2013-05-06 Bck to the "old" way - Ola
-- =============================================
--EXEC report_permission_data_check_test '478E8CFB-5B92-4049-95FF-9ABE00250898',12

CREATE FUNCTION [mart].[AllOwnedTeams](@person_code uniqueidentifier,@report_id uniqueidentifier)
RETURNS @teams TABLE (id int NOT NULL)
AS
BEGIN	

/*RETURN ALL TEAMS PERMITTED TO ME, EXCEPT MYSELF PERMISSION*/
INSERT INTO @teams(id)
SELECT DISTINCT dt.team_id 
FROM mart.permission_report perm WITH (NOLOCK)
INNER JOIN mart.dim_team dt WITH (NOLOCK) 
	ON dt.team_id=perm.team_id
WHERE person_code=@person_code
AND perm.ReportId=@report_id
AND perm.my_own=0 --not myown



/*RETURN ALL TEAMS I HAVE BELONGED TO, LOOKING AT MYSELF PERMISSIONS*/
INSERT INTO @teams(id)
SELECT DISTINCT dt.team_id
FROM mart.permission_report perm WITH (NOLOCK)
INNER JOIN mart.dim_person dp WITH (NOLOCK)
	ON dp.person_code=perm.person_code 
	AND dp.to_be_deleted = 0 --Only valid PersonPeriods
INNER JOIN mart.dim_team dt WITH (NOLOCK)
	ON dt.team_id=dp.team_id
WHERE perm.my_own=1 --only myself permissions
AND perm.person_code=@person_code
AND perm.ReportId=@report_id
AND dt.team_id NOT IN (SELECT id FROM @teams)
RETURN
END
GO

