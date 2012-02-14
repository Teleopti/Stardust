IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_scorecard_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_scorecard_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-18
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- Description:	Loads scorecards from stg_scorecard to dim_scorecard.

-- =============================================
CREATE PROCEDURE [mart].[etl_dim_scorecard_load] 
	
AS

--select * from dim_scorecard
--------------------------------------------------------------------------
-- Not Defined scorecard
SET IDENTITY_INSERT mart.dim_scorecard ON

INSERT INTO mart.dim_scorecard
	(
	scorecard_id,
	scorecard_name, 
	period,
	business_unit_id
	)
SELECT 
	scorecard_id			=-1, 
	scorecard_name		='Not Defined', 
	period		= -1,
	business_unit_id =-1
WHERE NOT EXISTS (SELECT * FROM mart.dim_scorecard where scorecard_id = -1)

SET IDENTITY_INSERT mart.dim_scorecard OFF


---------------------------------------------------------------------------
-- update changes on scorecard
UPDATE mart.dim_scorecard
SET 
	scorecard_name	= s.scorecard_name,
	period			= s.period
FROM
	Stage.stg_scorecard s
WHERE 
	s.scorecard_code = dim_scorecard.scorecard_code

-- Insert new scorecards
INSERT INTO mart.dim_scorecard
	(
	scorecard_code, 
	scorecard_name, 
	period,
	business_unit_id,
	datasource_id,
	datasource_update_date
	)
SELECT 
	scorecard_code		= s.scorecard_code, 
	scorecard_name		= s.scorecard_name, 
	period				= s.period,
	business_unit_id	= db.business_unit_id,
	datasource_id		= s.datasource_id,
	datasource_update_date =s.datasource_update_date
FROM
	Stage.stg_scorecard s
INNER JOIN 
	mart.dim_business_unit db
ON 
	db.business_unit_code=s.business_unit_code
WHERE 
	NOT EXISTS (SELECT scorecard_id FROM mart.dim_scorecard d WHERE d.scorecard_code = s.scorecard_code and d.datasource_id=s.datasource_id)



GO

