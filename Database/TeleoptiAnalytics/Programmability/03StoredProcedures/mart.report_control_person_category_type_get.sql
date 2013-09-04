IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_person_category_type_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_person_category_type_get]
GO


CREATE PROC [mart].[report_control_person_category_type_get]
@report_id		uniqueidentifier,
@person_code	uniqueidentifier,
@language_id	int,
@bu_id			uniqueidentifier
as
/*
Created:20080619 KJ
Last modified:
20080910 Added parameter @bu_id KJ
20090211 Added new mart5 schema KJ
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
*/

-----------------------------------------------------------------
-- Get person_id and team_id for user
DECLARE 
	@user_person_id int,
	@user_team_id	int

SELECT   
	@user_person_id = person_id,
	@user_team_id	= team_id
FROM 
	mart.dim_person d 
WHERE 
	person_code= @person_code



SELECT person_category_type_id as id, person_category_type_name as name
FROM mart.dim_person_category_type
ORDER BY person_category_type_id

GO

