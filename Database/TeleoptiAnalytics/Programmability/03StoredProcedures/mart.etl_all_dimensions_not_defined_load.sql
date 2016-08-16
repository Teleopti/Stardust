IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_all_dimensions_not_defined_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_all_dimensions_not_defined_load]
GO
CREATE PROCEDURE [mart].[etl_all_dimensions_not_defined_load] 
WITH EXECUTE AS OWNER
AS
--Create maxdate
DECLARE @eternityDate as smalldatetime
SELECT @eternityDate=CAST('20591231' as smalldatetime)

--Create mindate
DECLARE @mindate as smalldatetime
SELECT @mindate=CAST('19000101' as smalldatetime)

-- Not Defined Absence
SET IDENTITY_INSERT mart.dim_absence ON
INSERT INTO mart.dim_absence
	(
	absence_id,
	absence_code,
	absence_name, 
	display_color,
	in_contract_time,
	in_contract_time_name,
	in_paid_time,
	in_paid_time_name,
	in_work_time,
	in_work_time_name,
	is_deleted,
	display_color_html,
	absence_shortname
	)
SELECT 
	absence_id				= -1, 
	absence_code			= '00000000-0000-0000-0000-000000000000',
	absence_name			= 'Not Defined', 
	display_color			= -1,
	in_contract_time		= 0,
	in_contract_time_name	= 'Not In Contract Time',
	in_paid_time			= 0,
	in_paid_time_name		= 'Not In Paid Time',
	in_work_time			= 0,
	in_work_time_name		= 'Not In Work Time',
	is_deleted				= 0,
	display_color_html		= '#FFFFFF',
	absence_shortname		= 'Not Defined'
WHERE NOT EXISTS (SELECT * FROM mart.dim_absence where absence_id = -1)
SET IDENTITY_INSERT mart.dim_absence OFF

-- Not Defined Acd_login
SET IDENTITY_INSERT mart.dim_acd_login ON
INSERT INTO mart.dim_acd_login
	(
	acd_login_id,
	acd_login_name,
	datasource_id	
	)
SELECT 
	acd_login_id		=-1,
	acd_login_name		='Not Defined',
	datasource_id		=-1
WHERE NOT EXISTS (SELECT * FROM mart.dim_acd_login where acd_login_id = -1)
SET IDENTITY_INSERT mart.dim_acd_login OFF


-- Not Defined Activity
SET IDENTITY_INSERT mart.dim_activity ON
INSERT INTO mart.dim_activity
	(
	activity_id,
	activity_code,
	activity_name, 
	display_color,
	in_ready_time,
	in_ready_time_name,
	in_contract_time,
	in_contract_time_name,
	in_paid_time,
	in_paid_time_name,
	in_work_time,
	in_work_time_name,
	is_deleted, 
	display_color_html
	)
SELECT 
	activity_id				= -1, 
	activity_code			= '00000000-0000-0000-0000-000000000000',
	activity_name			= 'Not Defined', 
	display_color			= -1,
	in_ready_time			= 0,
	in_ready_time_name		= 'Not In Ready Time',
	in_contract_time		= 0,
	in_contract_time_name	= 'Not In Contract Time',
	in_paid_time			= 0,
	in_paid_time_name		= 'Not In Paid Time',
	in_work_time			= 0,
	in_work_time_name		= 'Not In Work Time',
	is_deleted				= 0,
	display_color_html		= '#FFFFFF'
WHERE NOT EXISTS (SELECT * FROM mart.dim_activity where activity_id = -1)
SET IDENTITY_INSERT mart.dim_activity OFF

--------------------------------------------------------------------------
-- Not Defined Business_unit
SET IDENTITY_INSERT mart.dim_business_unit ON
INSERT INTO mart.dim_business_unit
	(
	business_unit_id,
	business_unit_name, 
	datasource_id
	)
SELECT 
	business_unit_id		= -1,
	business_unit_name		= 'Not Defined',
	datasource_id			= -1
WHERE
	NOT EXISTS (SELECT d.business_unit_id FROM mart.dim_business_unit d WHERE d.business_unit_id=-1)
SET IDENTITY_INSERT mart.dim_business_unit OFF

--Not Defined Day off
SET IDENTITY_INSERT [mart].[dim_day_off] ON
INSERT INTO [mart].[dim_day_off]
		([day_off_id]
		,[day_off_code]
		,[day_off_name]
		,[display_color]
		,[business_unit_id]
		,[datasource_id]
		,[insert_date]
		,[update_date]
		,[datasource_update_date]
		,[display_color_html]
		,[day_off_shortname])
	SELECT
		-1
		,'00000000-0000-0000-0000-000000000000'
		,'Not Defined'
		,-1
		,-1
		,-1
		,GETUTCDATE()
		,GETUTCDATE()
		,null
		,'#FFFFFF'
		,'Not Defined'
WHERE NOT EXISTS (SELECT * FROM mart.dim_day_off where day_off_id = -1)
SET IDENTITY_INSERT [mart].[dim_day_off] OFF

-- Not Defined overtime
SET IDENTITY_INSERT mart.dim_overtime ON
INSERT INTO mart.dim_overtime
	(
	overtime_id,
	overtime_code,
	overtime_name,
	business_unit_id,
	datasource_id
	)
SELECT
	overtime_id			= -1,
	overtime_code		= '00000000-0000-0000-0000-000000000000',
	overtime_name		= 'Not Defined',
	business_unit_id	= -1,
	datasource_id		= -1
WHERE NOT EXISTS (SELECT * FROM mart.dim_overtime where overtime_id = -1)
SET IDENTITY_INSERT mart.dim_overtime OFF

-- Not Defined skillset
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

-----------------------
-- Not Defined Person
-----------------------
SET IDENTITY_INSERT mart.dim_person ON
INSERT INTO mart.dim_person
	(
		person_id,
		person_code,
		valid_from_date,
		valid_to_date,
		valid_from_date_id,
		valid_from_interval_id,
		valid_to_date_id,
		valid_to_interval_id,
		person_period_code,
		person_name,
		first_name,
		last_name,
		employment_number,
		employment_type_code,
		employment_type_name,
		contract_code,
		contract_name,
		parttime_code,
		parttime_percentage,
		team_id,
		team_code,
		team_name,
		site_id,
		site_code,
		site_name,
		business_unit_id,
		business_unit_code,
		business_unit_name,
		skillset_id,
		email,
		note,
		employment_start_date,
		employment_end_date,
		time_zone_id,
		is_agent,
		is_user,
		datasource_id,
		insert_date,
		update_date,
		datasource_update_date,
		to_be_deleted,
		windows_domain,
		windows_username,
		valid_to_date_id_maxDate,
		valid_to_interval_id_maxDate,
		valid_from_date_id_local,
		valid_from_date_local,
		valid_to_date_id_local,
		valid_to_date_local
	)
SELECT
		person_id					= -1,
		person_code					= NULL,
		valid_from_date				= @mindate,
		valid_to_date				= @eternityDate,
		valid_from_date_id			= -1,
		valid_from_interval_id		= 0,
		valid_to_date_id			= -2,
		valid_to_interval_id		= 0,
		person_period_code			= NULL,
		person_name					= 'Not Defined',
		first_name					= 'Not Defined',
		last_name					= 'Not Defined',
		employment_number			= 'Not Defined',
		employment_type_code		= NULL,
		employment_type_name		= 'Not Defined',
		contract_code				= NULL,
		contract_name				= 'Not Defined',
		parttime_code				= NULL,
		parttime_percentage			= 'Not Defined',
		team_id						= -1,
		team_code					= NULL,
		team_name					= 'Not Defined',
		site_id						= -1,
		site_code					= NULL,
		site_name					= 'Not Defined',
		business_unit_id			= -1,
		business_unit_code			= NULL,
		business_unit_name			= 'Not Defined',
		skillset_id					= -1,
		email						= 'Not Defined',
		note						= 'Not Defined',
		employment_start_date		= @mindate,
		employment_end_date			= @eternityDate,
		time_zone_id				= -1,
		is_agent					= NULL,
		is_user						= NULL,
		datasource_id				= -1,
		insert_date					= @mindate,
		update_date					= @mindate,
		datasource_update_date		= @mindate,
		to_be_deleted				= 0,
		windows_domain				= 'Not Defined',
		windows_username			= 'Not Defined',
		valid_to_date_id_maxDate	= -1,
		valid_to_interval_id_maxDate= -1,
		valid_from_date_id_local	= -1,
		valid_from_date_local	= @mindate,
		valid_to_date_id_local		= -1,
		valid_to_date_local	= @mindate
WHERE
	NOT EXISTS (SELECT d.person_id FROM mart.dim_person d WHERE d.person_id=-1)
SET IDENTITY_INSERT mart.dim_person OFF

-- Not Defined Quality Quest
SET IDENTITY_INSERT mart.dim_quality_quest ON
INSERT INTO [mart].[dim_quality_quest]
(
	[quality_quest_id]
    ,[quality_quest_agg_id]
    ,[quality_quest_original_id]
    ,[quality_quest_score_weight]
    ,[quality_quest_name]
    ,[log_object_name]
    ,[datasource_id]
    ,[insert_date]
    ,[update_date]
    ,[quality_quest_type_name]
    )
SELECT
	[quality_quest_id]			= -1,
	[quality_quest_agg_id]		= -1,
	[quality_quest_original_id] = -1,
	[quality_quest_score_weight]= 0.0000,
	[quality_quest_name]		= 'Not Defined',
	[log_object_name]			= 'Not Defined',
	datasource_id				= -1,
	insert_date					= getdate(),
	update_date					= getdate(),
	quality_quest_type_name     = 'Not Defined'
WHERE
	NOT EXISTS (SELECT * FROM mart.dim_quality_quest d WHERE d.quality_quest_id=-1)
SET IDENTITY_INSERT mart.dim_quality_quest OFF

-- Not Defined Queue
SET IDENTITY_INSERT mart.dim_queue ON
INSERT INTO mart.dim_queue
	(
	queue_id,
	datasource_id	
	)
SELECT 
	queue_id			=-1,
	datasource_id		=-1
WHERE NOT EXISTS (SELECT * FROM mart.dim_queue where queue_id = -1)
SET IDENTITY_INSERT mart.dim_queue OFF

-- Not Defined Scenario
SET IDENTITY_INSERT mart.dim_scenario ON
INSERT INTO mart.dim_scenario
	(
	scenario_id, 
	scenario_name, 
	business_unit_id,
	business_unit_name, 
	datasource_id,
	is_deleted
	)
SELECT 
	scenario_id			= -1, 
	scenario_name		= 'Not Defined', 
	business_unit_id	= -1,
	business_unit_name	= 'Not Defined', 
	datasource_id		= -1,
	is_deleted			=  0
WHERE NOT EXISTS (SELECT * FROM mart.dim_scenario where scenario_id = -1)
SET IDENTITY_INSERT mart.dim_scenario OFF

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

-- Not Defined shift category
SET IDENTITY_INSERT mart.dim_shift_category ON
INSERT INTO mart.dim_shift_category
	(
	shift_category_id,
	shift_category_code,
	shift_category_name, 
	shift_category_shortname, 
	display_color, 
	business_unit_id,
	datasource_id,  
	datasource_update_date,
	is_deleted
	)
SELECT 
	shift_category_id			= -1, 
	shift_category_code			= '00000000-0000-0000-0000-000000000000', 
	shift_category_name			= 'Not Defined', 
	shift_category_shortname	= 'Not Defined', 
	business_unit_id			= -1,
	display_color				= -1, 
	datasource_id				= -1,  
	datasource_update_date		= @mindate,
	is_deleted					= 0
WHERE NOT EXISTS (SELECT * FROM mart.dim_shift_category where shift_category_id = -1)
SET IDENTITY_INSERT mart.dim_shift_category OFF

-- Not Defined shift length
SET IDENTITY_INSERT mart.dim_shift_length ON
INSERT INTO mart.dim_shift_length
	(
	shift_length_id, 
	shift_length_m, 
	shift_length_h, 
	datasource_id
	)
SELECT 
	shift_length_id				= -1, 
	shift_length_m				= -1, 
	shift_length_h				= -1,  
	datasource_id				= 1
WHERE NOT EXISTS (SELECT * FROM mart.dim_shift_length where shift_length_id = -1)
SET IDENTITY_INSERT mart.dim_shift_length OFF

-- Not Defined site
SET IDENTITY_INSERT mart.dim_site ON

INSERT INTO mart.dim_site
	(
	site_id,
	site_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	site_id		= -1,
	site_name		= 'Not Defined',
	business_unit_id		= -1,
	datasource_id			= -1
WHERE
	NOT EXISTS (SELECT d.site_id FROM mart.dim_site d WHERE d.site_id=-1)

-- insert all, used in reports 
INSERT INTO mart.dim_site
	(
	site_id,
	site_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	site_id		= -2,
	site_name		= 'All',
	business_unit_id	= -1,
	datasource_id			= -1
WHERE
	NOT EXISTS (SELECT d.site_id FROM mart.dim_site d WHERE d.site_id=-2)

SET IDENTITY_INSERT mart.dim_site OFF

-- Not Defined skill
SET IDENTITY_INSERT mart.dim_skill ON

INSERT INTO mart.dim_skill
	(
	skill_id, 
	skill_name, 
	time_zone_id, 
	forecast_method_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	skill_id			= -1, 
	skill_name			= 'Not Defined', 
	time_zone_id		= -1, 
	forecast_method_name= 'Not Defined', 
	business_unit_id	= -1,
	datasource_id		= -1	
WHERE NOT EXISTS (SELECT * FROM mart.dim_skill where skill_id = -1)

SET IDENTITY_INSERT mart.dim_skill OFF

-- Not Defined team
SET IDENTITY_INSERT mart.dim_team ON
INSERT INTO mart.dim_team
	(
	team_id,
	team_name, 
	scorecard_id,
	datasource_id
	)
SELECT 
	team_id			= -1,
	team_name		= 'Not Defined',
	scorecard_id	= -1,
	datasource_id	= -1
WHERE
	NOT EXISTS (SELECT d.team_id FROM mart.dim_team d WHERE d.team_id=-1)
SET IDENTITY_INSERT mart.dim_team OFF

-- Not Defined workload
SET IDENTITY_INSERT mart.dim_workload ON
INSERT INTO mart.dim_workload
	(
	workload_id,
	workload_name,
	skill_name, 
	time_zone_id, 
	forecast_method_name, 
	business_unit_id,
	datasource_id
	)
SELECT 
	workload_id			= -1,
	workload_name		= 'Not Defined',
	skill_name			= 'Not Defined', 
	time_zone_id		= -1, 
	forecast_method_name= 'Not Defined', 
	business_unit_id	= -1,
	datasource_id		= -1	
WHERE NOT EXISTS (SELECT * FROM mart.dim_workload where workload_id = -1)
SET IDENTITY_INSERT mart.dim_workload OFF
