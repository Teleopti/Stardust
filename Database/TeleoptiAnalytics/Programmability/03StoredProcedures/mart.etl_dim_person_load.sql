IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_person_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_person_load]
GO

-- =============================================
-- Author:		ChLu
-- Create date: 2008-01-30
-- Description:	Loads persons from stg_person to dim person.
--				Also loads user/login info from dim person to aspnet_Users and aspnet_Membership
-- =============================================
-- Change Log:
-- Date			By		Description
-- =============================================
-- 2008-08-06			Changed column names User_name-->application_logon_name, employment_type_code-->employment_type_id
-- 2008-08-13	DaJo	Added columns: employment_number, windows_logon_name, windows_domain_name, password, email, note, language_id, language_name, time_zone_id, ui_culture, contract_code, contract_name, parttime_code, parttime_percentage
-- 2008-08-15	DaJo	Added default values for not defined persons
-- 2008-08-19	KaJe	Added delete statements for non-active users in aspnet_users and aspnet_membership tables
-- 2008-08-20	KaJe	Added new column skillset_id from new table stg_agent_skillset
-- 2009-02-09	KaJe	Stage moved to mart db, removed view
-- 2009-02-11	KaJe	New mart schema
-- 2009-04-06	KaJe	New column person_period_code. New handling for person with new time_zones.
-- 2009-04-27	DaJo	Change min/maxdate format
-- 2009-09-21	DaJo	Added ToBeDeleted
-- 2010-02-25	DaJo	Sync IsDeleted per BU
-- 2010-07-07	AnFo	Deleted PPs was not removed properly in dim person
-- 2010-09-15	DaJo	temp-fix of #11390 - Adding 24 h to ending person periods
-- 2010-11-30	DaJo	#12550 - Refactor dim person_load to use Person_Period_Code as key
-- 2012-03-05	DaJo	#????? - Try to avoid IsDeleted to be set when multiple ETL processes run at the same time
-- 2012-04-25	DaJo	#19150 - Add Windows credentials to cube
-- 2013-11-17	DaJo	#25718 - reduce the number of functions on dim person
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_person_load] 
@current_business_unit_code uniqueidentifier
WITH EXECUTE AS OWNER
AS

--Create maxdate
DECLARE @eternityDate as smalldatetime
SELECT @eternityDate=CAST('20591231' as smalldatetime)

--Create mindate
DECLARE @mindate as smalldatetime
SELECT @mindate=CAST('19000101' as smalldatetime)

--Get current max date_id from dim_date
DECLARE @maxdateid as int
DECLARE @maxdate as smalldatetime
SELECT @maxdateid = max(date_id),@maxdate = max(date_date) FROM mart.dim_date
WHERE date_id >= 0 --exclude the special ones < 0

DECLARE @maxInterval int
SELECT @maxInterval=ISNULL(max(interval_id),-1) FROM mart.dim_interval

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


--If the current BU-code is not i stage, something is fishy, Abort!
--ETL Service and ETL Tool conflict
if not exists (select 1 from stage.stg_person where business_unit_code=@current_business_unit_code)
RETURN 0


--<Todo>
--This section should be removed when InitialLoad dynamicly loads dim_date from => max(max_date_of_all_objects_in_raptor)
	-----------------------
	-- fix stage data!
	-- We cannot handle person periods that expands longer than dim_date
	-----------------------
	CREATE TABLE #lastPersonPeriod (person_code uniqueidentifier, last_valid_to_date smalldatetime)
	
	--insert into temp table: save the max valid_to_date = the ending of the last person period 
	--(we need this to distinguish between a leaving date or enterity date)
	INSERT INTO #lastPersonPeriod 
	SELECT person_code, max(valid_to_date)
	FROM stage.stg_person
	WHERE business_unit_code = @current_business_unit_code
	GROUP BY person_code
	
	--update the temp table
	--If leaving date is set, set maxdate, else keep the eternityDate
	UPDATE #lastPersonPeriod 
	SET last_valid_to_date = @maxdate
	WHERE last_valid_to_date <> @eternityDate

	--clean up stage
	---If person periods is starting after maxdate => delete rows
	DELETE FROM stage.stg_person
	WHERE valid_from_date > @maxdate
	
	--clean up stage
	--If person period is still ending after maxdate => update from temp table
	UPDATE stage.stg_person
	SET valid_to_date = tmp.last_valid_to_date
	FROM #lastPersonPeriod tmp
	WHERE	stage.stg_person.person_code	= tmp.person_code
	AND		stage.stg_person.valid_to_date	> @maxdate
--</Todo>

-----------------------	
-- reset to-be-deleted flag for all person periods
-----------------------
UPDATE mart.dim_person
SET to_be_deleted=0
WHERE business_unit_code = @current_business_unit_code

-----------------------
-- delete persons
-- e.g. mark rows to be deleted when a PersonPeriod
-- has been deleted in app-DB (not present in stage any more)
-----------------------
UPDATE mart.dim_person
SET to_be_deleted=1
FROM stage.stg_person s
RIGHT OUTER JOIN
	mart.dim_person p
ON p.person_period_code = s.person_period_code
WHERE p.person_id<>-1  --Not the Default person
	AND s.person_period_code IS NULL
	and p.business_unit_code = @current_business_unit_code
	
-------------------------
-- update changes on person
-------------------------
UPDATE mart.dim_person
SET 
	valid_from_date			= s.valid_from_date,
	valid_to_date			= s.valid_to_date, 
	valid_from_date_id		= d1.date_id,
	valid_from_interval_id	= s.valid_from_interval_id,
	valid_to_date_id		= isnull(d2.date_id,@maxdateid),
	valid_to_interval_id	= s.valid_to_interval_id,
	person_period_code		= s.person_period_code,
	person_name				= s.person_name,
	first_name				= s.person_first_name, 
	last_name				= s.person_last_name,
	employment_number		= s.employment_number,
	employment_type_code	= null,--s.employment_type_code, missing in stg!
	employment_type_name	= s.employment_type, 
	team_id					= dt.team_id,
	team_code				= s.team_code, 
	team_name				= s.team_name, 
	site_id					= dt.site_id,
	site_code				= s.site_code, 
	site_name				= s.site_name, 
	business_unit_id		= dt.business_unit_id,
	business_unit_code		= s.business_unit_code, 
	business_unit_name		= s.business_unit_name, 
	skillset_id				= -1,
	email					= s.email,
	note					= s.note,
	employment_start_date	= s.employment_start_date, 
	employment_end_date		= s.employment_end_date, 
	time_zone_id			= dtz.time_zone_id,
	is_agent				= s.is_agent, 
	is_user					= s.is_user, 
	contract_code			= s.contract_code,
	contract_name			= s.contract_name,
	parttime_code			= s.parttime_code,
	parttime_percentage		= s.parttime_percentage,
	datasource_id			= 1, 
	update_date				= getdate(),
	datasource_update_date	= s.datasource_update_date,
	windows_domain			= case when (s.windows_domain is null or s.windows_domain = '') then 'Not Defined' else s.windows_domain end,
	windows_username		= case when (s.windows_username is null or s.windows_username = '') then 'Not Defined' else s.windows_username end,
	valid_to_date_id_maxDate	= CASE WHEN d2.date_id =-2 THEN @maxdateid-1 ELSE isnull(d2.date_id,@maxdateid-1) END, 
	valid_to_interval_id_maxDate= CASE WHEN d2.date_id =-2 THEN @maxInterval WHEN d2.date_id IS NULL THEN @maxInterval ELSE isnull(s.valid_to_interval_id,@maxInterval)  END, 
	valid_from_date_local	= s.valid_from_date_local,
	valid_from_date_id_local = d3.date_id,
	valid_to_date_local		= CASE WHEN s.valid_to_date_local=@eternityDate THEN @maxdate ELSE ISNULL(d4.date_date,@maxdate) END,
	valid_to_date_id_local = CASE WHEN d4.date_id=-2 THEN @maxdateid ELSE isnull(d4.date_id,@maxdateid)  END
FROM
	Stage.stg_person s	
LEFT JOIN
	mart.dim_team dt
ON
	 s.team_code	= dt.team_code
LEFT JOIN
	mart.dim_time_zone dtz
ON
	s.time_zone_code = dtz.time_zone_code
INNER JOIN
	mart.dim_date d1
ON
	CONVERT(smalldatetime, CONVERT(int,(CONVERT(float,s.valid_from_date)))) = d1.date_date -- Remove time part from date
LEFT JOIN
	mart.dim_date d2
ON
	CONVERT(smalldatetime, CONVERT(int,(CONVERT(float,s.valid_to_interval_start)))) = d2.date_date -- Remove time part from date
INNER JOIN mart.dim_date d3 --valid_from_date_local
	ON s.valid_from_date_local = d3.date_date
LEFT JOIN mart.dim_date d4 --valid_to_date_local
	ON s.valid_to_date_local = d4.date_date
WHERE 
	s.person_period_code		= mart.dim_person.person_period_code
	
-------------------------
-- Insert new persons
-------------------------
INSERT INTO mart.dim_person	
	(
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
	contract_code,
	contract_name,
	parttime_code,
	parttime_percentage,
	datasource_id, 
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
	person_code				= s.person_code,
	valid_from_date			= s.valid_from_date,
	valid_to_date			= s.valid_to_date, 
	valid_from_date_id		= d1.date_id,
	valid_from_interval_id	= s.valid_from_interval_id,
	valid_to_date_id		= isnull(d2.date_id,@maxdateid),
	valid_to_interval_id	= s.valid_to_interval_id,
	person_period_code		= s.person_period_code,
	person_name				= s.person_name, 
	first_name				= s.person_first_name, 
	last_name				= s.person_last_name,
	employment_number		= s.employment_number, 
	employment_type_id		= null,--s.employment_type_code, missing in stg!
	employment_type_name	= s.employment_type, --rename => s.employment_type_name
	team_id					= dt.team_id,
	team_code				= s.team_code, 
	team_name				= s.team_name, 
	site_id					= dt.site_id,
	site_code				= s.site_code, 
	site_name				= s.site_name, 
	business_unit_id		= dt.business_unit_id,
	business_unit_code		= s.business_unit_code, 
	business_unit_name		= s.business_unit_name, 
	skillset_id				= -1,
	email					= s.email,
	note					= s.note,
	employment_start_date	= s.employment_start_date, 
	employment_end_date		= s.employment_end_date,
	time_zone_id			= dtz.time_zone_id,
	is_agent				= s.is_agent, 
	is_user					= s.is_user,
	contract_code			= s.contract_code,
	contract_name			= s.contract_name,
	parttime_code			= s.parttime_code,
	parttime_percentage		= s.parttime_percentage,
	datasource_id			= 1, 
	datasource_update_date	= s.datasource_update_date,
	to_be_deleted			= 0,
	windows_domain			= s.windows_domain,
	windows_username		= s.windows_username,
	valid_to_date_id_maxDate	= CASE WHEN d2.date_id =-2 THEN @maxdateid-1 ELSE isnull(d2.date_id,@maxdateid-1) END, 
	valid_to_interval_id_maxDate= CASE WHEN d2.date_id =-2 THEN @maxInterval WHEN d2.date_id IS NULL THEN @maxInterval ELSE isnull(s.valid_to_interval_id, @maxInterval) END, 
	valid_from_date_id_local	= d3.date_id,
	valid_from_date_local	= s.valid_from_date_local,
	valid_to_date_id_local		= CASE WHEN d4.date_id=-2 THEN @maxdateid ELSE ISNULL(d4.date_id,@maxdateid)  END,
	valid_to_date_local	= CASE WHEN s.valid_to_date_local=@eternityDate THEN @maxdate ELSE ISNULL(d4.date_date,@maxdate)END
FROM
	Stage.stg_person s
LEFT JOIN
	mart.dim_team dt
ON
	 s.team_code	= dt.team_code
LEFT JOIN
	mart.dim_time_zone dtz
ON
	s.time_zone_code = dtz.time_zone_code
INNER JOIN
	mart.dim_date d1
ON
	CONVERT(smalldatetime, CONVERT(int,(CONVERT(float,s.valid_from_date)))) = d1.date_date -- Remove time part from date
LEFT JOIN
	mart.dim_date d2
ON
	CONVERT(smalldatetime, CONVERT(int,(CONVERT(float,s.valid_to_interval_start)))) = d2.date_date -- Remove time part from date
INNER JOIN mart.dim_date d3 --valid_from_date_local
	ON s.valid_from_date_local = d3.date_date
LEFT JOIN mart.dim_date d4 --valid_to_date_local
	ON s.valid_to_date_local = d4.date_date
WHERE 
	NOT EXISTS (SELECT person_id 
				FROM mart.dim_person p 
				WHERE
					s.person_period_code		= p.person_period_code
				)

--<ToDo>
--re-fac ETL and table to use person_period_code
	-------------------------
	-- Update persons skillset
	-------------------------
	UPDATE mart.dim_person
	SET
		skillset_id	= ds.skillset_id
	FROM 
		mart.dim_person dp
	INNER JOIN
		Stage.stg_agent_skillset sas
	ON
		sas.person_code	= dp.person_code	AND
		sas.date_from	= dp.valid_from_date
	INNER JOIN
		mart.dim_skillset ds
	ON
		ds.skillset_id = sas.skillset_id
	WHERE dp.business_unit_code = @current_business_unit_code
--</ToDo>


GO
