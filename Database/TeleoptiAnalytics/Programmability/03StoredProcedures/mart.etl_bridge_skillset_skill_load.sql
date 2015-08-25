IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_skillset_skill_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_skillset_skill_load]
GO
/****** Object:  StoredProcedure [mart].[etl_bridge_skillset_skill_load]    Script Date: 12/04/2008 16:08:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		KaJe
-- Update date: 2009-02-11
-- 2009-02-11 New Mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-04-27 Change dateformat on min/max date DJ
-- Update date: 2008-12-04 fixes for multi BU load KJ
-- Description:	Loads bridge, that connects skillsets with skills.
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_skillset_skill_load] 
@business_unit_code uniqueidentifier
	
AS

--Create maxdate
DECLARE @maxdate as smalldatetime
SELECT @maxdate=CAST('20591231' as smalldatetime)

/*Get business unit id*/
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

/*Deleting data*/
DELETE FROM mart.bridge_skillset_skill 
WHERE business_unit_id = @business_unit_id
	OR business_unit_id = -1


/*
DELETE FROM mart.bridge_skillset_skill 
WHERE business_unit_id = (SELECT DISTINCT business_unit_id FROM Stage.stg_agent_skillset)
	OR business_unit_id = -1
*/
--INSERT -1
INSERT INTO mart.bridge_skillset_skill
	(	skillset_id, 
		skill_id, 
		business_unit_id, 
		datasource_id, 
		insert_date, 
		update_date, 
		datasource_update_date
	)
SELECT DISTINCT
	skillset_id		= ds.skillset_id, 
	skill_id		= -1, 
	business_unit_id= ds.business_unit_id, 
	datasource_id	= ds.datasource_id, 
	insert_date		= getdate(), 
	update_date		= getdate(), 
	datasource_update_date = GETDATE()
FROM 
	mart.dim_skillset ds
WHERE skillset_id=-1


-- Insert 
INSERT INTO mart.bridge_skillset_skill
	(	skillset_id, 
		skill_id, 
		business_unit_id, 
		datasource_id, 
		insert_date, 
		update_date, 
		datasource_update_date
	)
SELECT DISTINCT
	skillset_id		= ds.skillset_id, 
	skill_id		= isnull(vas.skill_id,-1), 
	business_unit_id= isnull(vas.business_unit_id,-1), 
	datasource_id	= isnull(vas.datasource_id,-1), 
	insert_date		= getdate(), 
	update_date		= getdate(), 
	datasource_update_date = isnull(vas.datasource_update_date,@maxdate)
FROM
	mart.dim_skillset ds
INNER JOIN 
	Stage.stg_agent_skillset vas 
	ON
	ds.skillset_id = vas.skillset_id AND
	ds.business_unit_id=vas.business_unit_id
WHERE ds.business_unit_id = @business_unit_id




GO
