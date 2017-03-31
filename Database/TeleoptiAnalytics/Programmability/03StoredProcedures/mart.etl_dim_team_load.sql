IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_team_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_team_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Description:	Loads teams from stg_person to dim_team.
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-04-01 Removed team -2 All KJ
-- 2010-07-07 Added scorecard to team level RK
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_team_load] 
WITH EXECUTE AS OWNER
AS


--------------------------------------------------------------------------
-- Not Defined team
SET IDENTITY_INSERT mart.dim_team ON

INSERT INTO mart.dim_team
	(
	team_id,
	team_code,
	team_name, 
	scorecard_id,
	datasource_id
	)
SELECT 
	team_id			= -1,
	team_code       = '00000000-0000-0000-0000-000000000000',
	team_name		= 'Not Defined',
	scorecard_id	= -1,
	datasource_id	= -1
WHERE
	NOT EXISTS (SELECT d.team_id FROM mart.dim_team d WHERE d.team_id=-1)

UPDATE [mart].[dim_team]
SET team_code = '00000000-0000-0000-0000-000000000000'
WHERE team_id = -1 AND team_code is null

-- insert all, used in reports 
/*INSERT INTO mart.dim_team
	(
	team_id,
	team_name, 
	datasource_id
	)
SELECT 
	team_id		= -2,
	team_name		= 'All',
	datasource_id			= -1
WHERE
	NOT EXISTS (SELECT d.team_id FROM mart.dim_team d WHERE d.team_id=-2)
*/
SET IDENTITY_INSERT mart.dim_team OFF

---------------------------------------------------------------------------
-- update changes on person
UPDATE mart.dim_team
SET 
	team_name			= sub.team_name,
	scorecard_id		= sub.scorecard_id,
	site_id				= sub.site_id,
	business_unit_id	= sub.business_unit_id
FROM
( 	
SELECT 
	s.team_code,
	s.datasource_id,
	business_unit_id	= isnull(ds.business_unit_id,-1),
	site_id				= isnull(site_id,-1),
	team_name			= max(s.team_name),
	scorecard_id		= isnull(sc.scorecard_id,-1)
FROM
	Stage.stg_person s
LEFT JOIN
	mart.dim_site ds
ON
	ds.site_code	= s.site_code
LEFT JOIN
	mart.dim_scorecard sc
ON
	s.scorecard_code = sc.scorecard_code
GROUP BY 
	s.datasource_id,
	s.team_code,
	isnull(ds.business_unit_id,-1),
	isnull(site_id,-1),
	isnull(sc.scorecard_id,-1)
)sub
WHERE 
	sub.team_code		= mart.dim_team.team_code	AND
	sub.datasource_id	= mart.dim_team.datasource_id

-- Insert new team
INSERT INTO mart.dim_team
	(
	team_code,
	team_name,
	scorecard_id,
	site_id,
	business_unit_id,
	datasource_id
	)
SELECT DISTINCT
	team_code			= s.team_code,
	team_name			= s.team_name,
	scorecard_id		= sc.scorecard_id,
	site_id				= isnull(ds.site_id,-1),
	business_unit_id	= isnull(ds.business_unit_id,-1),
	datasource_id		= s.datasource_id
FROM
	Stage.stg_person s
LEFT JOIN
	mart.dim_site ds
ON
	s.site_code = ds.site_code
LEFT JOIN
	mart.dim_scorecard sc
ON
	s.scorecard_code = sc.scorecard_code
WHERE 
	NOT EXISTS (SELECT team_code
				FROM mart.dim_team p 
				WHERE
						s.team_code	= p.team_code	AND
						s.datasource_id			= p.datasource_id
				)

--update historically rows in dim person
UPDATE mart.dim_person
SET team_name			= t.team_name
FROM mart.dim_team t
WHERE t.team_id = mart.dim_person.team_id

GO

