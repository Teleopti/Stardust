IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_skillset_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_skillset_load]
GO


-- =============================================
-- Author:		KaJe
-- Create date: 2008-08-20
-- Description:	Loads skillsets from stg_agent_skill to dim_skillset
-- Update date: 2009-02-09
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_skillset_load] 
	
AS
SET NOCOUNT ON 
--------------------------------------------------------------------------
-- Not Defined skill
SET IDENTITY_INSERT mart.dim_skillset ON

INSERT INTO mart.dim_skillset
	(
	skillset_id, 
	skillset_code,
	skillset_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	skillset_id			= -1, 
	skillset_code		= 'Not Defined', 
	skillset_name		= 'Not Defined', 
	business_unit_id	= -1,
	datasource_id		= -1	
WHERE NOT EXISTS (SELECT * FROM mart.dim_skillset where skillset_id = -1)

SET IDENTITY_INSERT mart.dim_skillset OFF
--SELECT * FROM dim_skillset
---------------------------------------------------------------------------
--drop table #person_skills
/*SELECT DISTINCT person_code,skill_code,date_from,date_to,update_date
INTO #person_skills
FROM
	v_stg_agent_skill
ORDER BY person_code,skill_code,date_from,date_to,update_date

--drop table #skills
CREATE TABLE #skills(	person_code uniqueidentifier,
						skill_id int,
						skill_name nvarchar(100),
						date_from datetime,
						date_to	datetime,
						business_unit_id int,
						update_date	datetime,
						skill_sum_code nvarchar(4000),
						skill_sum_name nvarchar(4000),
						skillset_id int
					)*/
--EMPTY TABLE
DELETE FROM  Stage.stg_agent_skillset

INSERT Stage.stg_agent_skillset(person_code, 
							skill_id, 
							date_from, 
							skillset_id, 
							date_to, 
							skill_name, 
							skill_sum_code, 
							skill_sum_name, 
							business_unit_id, 
							datasource_id, 
							insert_date, 
							update_date, 
							datasource_update_date)
SELECT DISTINCT	person_code= vas.person_code,
		skill_id		=  ds.skill_id,
		date_from		= vas.date_from,
		skillset_id		= -1,
		date_to			= vas.date_to,
		skill_name		= ds.skill_name,
		skill_sum_code	= '',
		skill_sum_name	= '',
		business_unit_id= ds.business_unit_id, 
		datasource_id	= 1,
		insert_date		= getdate(),
		update_date		= getdate(),
		datasource_update_date	= vas.update_date	
FROM 
	Stage.stg_agent_skill vas
INNER JOIN Mart.dim_skill ds 
	ON ds.skill_code= vas.skill_code
ORDER BY vas.person_code,ds.skill_id,vas.date_from,vas.date_to

--select * from #skills

DECLARE @tmp_datefrom datetime
DECLARE @tmp_dateto  datetime
DECLARE @tmp_skill_name nvarchar(100)
DECLARE @tmp_skill int
DECLARE @tmp_person uniqueidentifier

DECLARE skill_cursor CURSOR FOR
SELECT DISTINCT person_code, skill_id,skill_name,date_from,date_to
FROM Stage.stg_agent_skillset
ORDER BY person_code, skill_id,skill_name,date_from,date_to
OPEN skill_cursor
FETCH NEXT FROM skill_cursor INTO @tmp_person,@tmp_skill,@tmp_skill_name,@tmp_datefrom,@tmp_dateto
WHILE @@FETCH_STATUS = 0
BEGIN
		UPDATE Stage.stg_agent_skillset
		SET skill_sum_name =skill_sum_name + @tmp_skill_name + ','
		,skill_sum_code= skill_sum_code + convert(varchar(5),@tmp_skill) +','
		WHERE person_code=@tmp_person AND date_from=@tmp_datefrom AND date_to=@tmp_dateto

	FETCH NEXT FROM skill_cursor INTO @tmp_person,@tmp_skill,@tmp_skill_name,@tmp_datefrom,@tmp_dateto
END
CLOSE skill_cursor
DEALLOCATE skill_cursor

UPDATE Stage.stg_agent_skillset
SET skill_sum_code=LEFT(skill_sum_code, len(skill_sum_code) -1)
WHERE right(skill_sum_code, 1)=','

UPDATE Stage.stg_agent_skillset
SET skill_sum_name=LEFT(skill_sum_name, len(skill_sum_name) -1)
WHERE right(skill_sum_name, 1)=','



--UPDATE OLD SKILLSETS where skills have new names
UPDATE Mart.dim_skillset
SET skillset_name = s.skill_sum_name,
	update_date = getdate()
FROM 
	Stage.stg_agent_skillset s
WHERE
	Mart.dim_skillset.skillset_code = s.skill_sum_code
	AND Mart.dim_skillset.skillset_name <> s.skill_sum_name

-- Insert new skills
INSERT INTO Mart.dim_skillset
	(
	skillset_code, 
	skillset_name, 
	business_unit_id,
	datasource_id, 
	datasource_update_date
	)
SELECT DISTINCT 
	skillset_code				= s.skill_sum_code, 
	skillset_name				= s.skill_sum_name,  
	business_unit_id			= s.business_unit_id,
	datasource_id				= 1, 
	datasource_update_date		= max(s.datasource_update_date)
FROM
	Stage.stg_agent_skillset s
WHERE 
	NOT EXISTS (SELECT skillset_code FROM Mart.dim_skillset d WHERE d.skillset_code = s.skill_sum_code and d.datasource_id=1)
GROUP BY 
	skill_sum_code, 
	skill_sum_name, 
	s.business_unit_id
ORDER BY skill_sum_code, 
	skill_sum_name, 
	s.business_unit_id
--SELECT * FROM DIM_SKILLSET
SET NOCOUNT ON
UPDATE Stage.stg_agent_skillset
SET skillset_id=dim_skillset.skillset_id
FROM Mart.dim_skillset
WHERE Stage.stg_agent_skillset.skill_sum_code=dim_skillset.skillset_code
--select * from #skills


GO

