IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_permission_data_check_test]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_permission_data_check_test]
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
--EXEC report_permission_data_check_test '2000-01-01','2003-01-10',21,'0,8,10,51,123,754,755','37D3CD63-90D8-4E89-94B9-9ABE00250819',12

--SELECT * FROM DIM_PERSON WHERE TEAM_ID=21
CREATE PROCEDURE [mart].[report_permission_data_check_test]
@person_code uniqueidentifier,
@report_id uniqueidentifier
AS
	
SET NOCOUNT ON
/*declare @person_code uniqueidentifier
set @person_code='478E8CFB-5B92-4049-95FF-9ABE00250898'*/


--SELECT * FROM mart.permission_report_data_test WHERE person_code=@person_code
CREATE TABLE #teams(id int)
/*RETURN ALL TEAMS PERMITTED TO ME, EXCEPT MYSELF PERMISSION*/
INSERT #teams(id)
SELECT DISTINCT team_id 
FROM mart.permission_report_data_test perm
INNER JOIN mart.dim_team dt ON
	dt.team_id=perm.data_id
WHERE person_code=@person_code
AND data_id<>-2 --not where team_id is missing


/*RETURN ALL TEAMS I HAVE BELONGED TO, LOOKING AT MYSELF PERMISSIONS*/
INSERT #teams(id)
SELECT DISTINCT dt.team_id
FROM permission_report_data_test perm
INNER JOIN mart.dim_person dp ON dp.person_code=perm.person_code
INNER JOIN mart.dim_team dt ON dt.team_id=dp.team_id
WHERE perm.myself=1 --only myself permissions
AND perm.person_code=@person_code


/*RETURN ALL*/
SELECT DISTINCT id from
#teams

GO

