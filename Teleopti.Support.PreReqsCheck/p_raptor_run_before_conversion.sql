

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[p_raptor_run_before_conversion]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[p_raptor_run_before_conversion]
GO

/* 
=============================================
 Author:		<Author,,Name>
 Create date: <2008-05-07>
 Description:	This procedure is used to check for know errors in version 6 of Teleopti WFM.
				If we find an error in the conversion which is due to an error in version 6,
				we write code here to check for it. An error list is then presented to the 
				user and he/she need to fix it in the old database before converting.
 =============================================
 */
 CREATE PROCEDURE [dbo].[p_raptor_run_before_conversion] 

AS
BEGIN
/* 
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
*/
	SET NOCOUNT ON;

	CREATE TABLE #errors(error_number int,error_type varchar(100),error_msg varchar(500))
    DECLARE @version varchar(25)
	DECLARE @patch int
	DECLARE @error_number int
	SET @error_number = 0

	SELECT @version = system_setting_desc_value FROM system_settings where system_setting_id=41

/* 
  START Check version
*/
	IF @version <> '6.5/6.6'
	BEGIN
		SET @error_number = 1
		INSERT INTO #errors
			SELECT @error_number, 'Incorrect main version','You need to have version 6.6 to upgrade'
	END

/* 
	START Check patch
*/
	SELECT @patch = convert(int,round(convert(real,system_setting_desc_value),0)) FROM system_settings where system_setting_id=42
	IF @patch < 65038
	BEGIN
		SET @error_number = 2
		INSERT INTO #errors
			SELECT @error_number,'Incorrect patch','You need to have latest patch to upgrade'
	END



/* 
	START Check forecast connected to multiple skills
*/
	SET @error_number = 3

	CREATE TABLE #nom_skills (fc_id int, skill_count int)
	INSERT INTO #nom_skills
		SELECT forecast_id, count(skill_id) skill_count 
		FROM t_fc5_skill_forecasts sf
			INNER JOIN t_fc5_forecasts fc ON fc.id=sf.forecast_id
		GROUP BY forecast_id
		HAVING count(skill_id)>1

	INSERT INTO #errors
		SELECT @error_number,'Forecast is connected to multiple skills','Forecast: ' + CONVERT(varchar(4),forecast_id) + ' Forecast name: ' + f.name + ' connected to Skill: ' + CONVERT(varchar(4),skill_id) + ' Skill name: ' + s.name 
		FROM #nom_skills n
			INNER JOIN t_fc5_forecasts f ON f.id=n.fc_id
			INNER JOIN t_fc5_skill_forecasts sf ON sf.forecast_id=n.fc_id
			INNER JOIN t_fc5_skills s ON sf.skill_id= s.id
		ORDER BY forecast_id


/* 
	Start Check AbsenceActivity compability
*/
SET @error_number = 4

	create table #tmp_out(shift_class nvarchar(255))
	create table #shifts(shift_class nvarchar(255), shift_class_id int, abs_id int,abs_desc_nonuni nvarchar(255), activity_id int, activity_name_nonuni nvarchar(255))

	insert into #shifts
		select sc.shift_class_name, sc.shift_class_id,a.abs_id,a.abs_desc_nonuni, ac.activity_id, ac.activity_name_nonuni
		from shift_classes sc 
		inner join absences a on sc.activity_id = a.activity_id
		inner join activities ac on	a.activity_id = ac.activity_id
		

	insert into #shifts
		select sc.shift_class_name, sc.shift_class_id, a.abs_id,a.abs_desc_nonuni, ac.activity_id, ac.activity_name_nonuni
		from shift_classes sc 
		inner join shift_breakes sb on sc.shift_class_id = sb.shift_class_id
		inner join absences a on sb.activity_id = a.activity_id
		inner join activities ac on	a.activity_id = ac.activity_id
		
	insert into #shifts
		select sc.shift_class_name, sc.shift_class_id, a.abs_id,a.abs_desc_nonuni, ac.activity_id, ac.activity_name_nonuni
		from shift_classes sc 
		inner join shift_class_act_min_periods sb on sc.shift_class_id = sb.shift_class_id
		inner join absences a on sb.activity_id = a.activity_id
		inner join activities ac on a.activity_id = ac.activity_id

	IF (SELECT COUNT(*) FROM #shifts) > 0
		INSERT INTO #errors
		SELECT @error_number,'Number of Shift classes based on an absence activity or has absence activities connected:', COUNT(*)
		FROM #shifts


/* 
	Start Check multiple users with same user_name and domain
*/
SET @error_number = 5
	create table #tmp_users(user_name nvarchar(50), domain nvarchar(50))

	INSERT INTO #tmp_users
		select user_name,domain  
		from t_tmc_users 
		group by user_name,domain
		having count(*)>1
		
	INSERT INTO #errors
		select @error_number, 'Multiple users with same user_name and domain:', user_name + '\' + domain
		from #tmp_users

/* 
	Start Check skill levels to high/low
*/
SET @error_number = 6
	INSERT INTO #errors
		select @error_number,'The skill levels have to be between 25% and 200%',exp_desc
		from t_skill_exp where (exp_value <0.25 or exp_value>2)

/* 
	Start Check rotation name length
*/
SET @error_number = 7
	INSERT INTO #errors
		select @error_number, 'The rotation name should be max 50 characters',rotation_name
		from t_rotations where (len(rotation_name)>50)

/* 
	Check shift length
*/
SET @error_number = 8
	DECLARE @intervals_per_day int
	SELECT @intervals_per_day =  system_setting_value FROM system_settings where system_setting_id=1
	SELECT @intervals_per_day = @intervals_per_day + @intervals_per_day/2
	
	SELECT COUNT(*) as activities, s.shift_id, ass.emp_id, ass.sched_date INTO #shift_ids
	FROM assigned_shifts ass
	INNER JOIN shifts s on ass.shift_id = s.shift_id
	INNER JOIN shift_interval_activity sia ON s.shift_id = sia.shift_id
	GROUP BY s.shift_id, ass.emp_id, ass.sched_date
	HAVING COUNT(*) > @intervals_per_day

	IF (SELECT COUNT(*) FROM #shift_ids) > 0
	INSERT INTO #errors
	SELECT @error_number, 'Number of shifts longer than 36 hours:', COUNT(*)
	FROM #shift_ids
		


/* 
	START Check days off wiht activities
*/
SET @error_number = 9
		IF (SELECT COUNT(*) FROM absences WHERE apply_count_rules=1 and activity_id is not null) > 0
		BEGIN
			INSERT INTO #errors
			SELECT @error_number, 'You have absences with apply count rules and activity_id', COUNT(*)
			FROM absences WHERE apply_count_rules=1 and activity_id is not null
		END	

/*
	START Check workload name.length >0
*/
SET @error_number = 10

	IF (SELECT COUNT(*) FROM t_fc5_forecasts WHERE len(name) = 0) > 0
	INSERT INTO #errors
		SELECT 10, id, 'Forecast id ' + convert(varchar(10),id) + ' has to have a name' FROM t_fc5_forecasts WHERE len(name)=0
	
		


/* 
	START Check skill name.length >0
*/
SET @error_number = 11

	IF (SELECT COUNT(*) FROM t_fc5_skills WHERE len(name) = 0) > 0
	INSERT INTO #errors
		SELECT 11, id, 'Skill id ' + convert(varchar(10),id) + ' has to have a name' FROM t_fc5_skills WHERE len(name)=0
	
		


/* 
	START Check overtime activities
*/

SET @error_number = 12


	INSERT INTO t_raptor_overtime(overtime_id,overtime_name)
		SELECT distinct a.activity_id,COALESCE(activity_name_nonuni,activity_name) FROM activities a
			INNER JOIN t_fc5_skill_activities sa ON a.activity_id=sa.activity_id
		WHERE a.activity_id NOT IN (SELECT overtime_id FROM t_raptor_overtime)
		AND a.in_worktime = 0
	
/* 	
	--AF: Add the easy ones and perhaps there's noone left to fix manually?
*/
	select min(s.activity_id) as activity_id, o.overtime_id
	into #ota
	from t_raptor_overtime o
	inner join t_fc5_skill_activities sa on o.overtime_id = sa.activity_id
	inner join t_fc5_skills s on sa.skill_id = s.id
	group by o.overtime_id
	having min(s.activity_id) = max(s.activity_id)

	update t_raptor_overtime
	set activity_id = #ota.activity_id
	from t_raptor_overtime o
	inner join #ota on #ota.overtime_id = o.overtime_id


	IF (SELECT COUNT(*) FROM t_raptor_overtime WHERE activity_id IS NULL) > 0
	INSERT INTO #errors
		SELECT 12, overtime_id, 'The "' + overtime_name + '" has to be mapped to an activity in "raptor_overtime" table.' FROM t_raptor_overtime WHERE activity_id IS NULL

	IF (SELECT COUNT(*) FROM t_raptor_overtime WHERE activity_id = overtime_id) > 0
	INSERT INTO #errors
		SELECT 12, overtime_id, 'The "' + overtime_name + '" cannot be mapped to itself in "raptor_overtime" table.' FROM t_raptor_overtime WHERE activity_id = overtime_id
		


/* 
	START Inform about overtime activities
*/
SET @error_number = 13

	IF (SELECT count(*) FROM activities a 
			INNER JOIN t_fc5_skill_activities sa ON a.activity_id=sa.activity_id
		WHERE a.activity_id NOT IN (SELECT overtime_id FROM t_raptor_overtime)
		AND a.in_worktime = 1) > 0
		
	INSERT INTO #errors
		SELECT distinct 13, a.activity_id, 'The "' + COALESCE(activity_name_nonuni,activity_name) + '" will not be converted as overtime or a skill activity because it is "in worktime".' 
		FROM activities a
			INNER JOIN t_fc5_skill_activities sa ON a.activity_id=sa.activity_id
		WHERE a.in_worktime = 1


	INSERT INTO #errors
		SELECT distinct 13, a.activity_id, COALESCE(activity_name_nonuni,activity_name) + '" cannot be both skill activity and an overtime activity'  
		FROM activities a
			INNER JOIN t_fc5_skills s ON s.activity_id=a.activity_id
		WHERE s.activity_id in (SELECT overtime_id FROM t_raptor_overtime)




/* 
	START Inform about konfidential activities not being converterd
*/
SET @error_number = 14

	IF (SELECT COUNT(*) FROM activities a WHERE activity_id not in (SELECT activity_id FROM absences) AND private_desc <> '') > 0
		

	INSERT INTO #errors
		SELECT 14, a.activity_id, COALESCE(activity_name_nonuni,activity_name) + '" will not be converted as private. This should probably connected to an absence.'  
		FROM activities a
		WHERE a.activity_id not in (SELECT activity_id FROM absences WHERE activity_id is not null) AND len(private_desc)>0



/* 
	START Inform about skills missing activities
*/
SET @error_number = 15

	IF (SELECT COUNT(*) FROM t_fc5_skills s WHERE activity_id IS NULL) > 0
		
	INSERT INTO #errors
		SELECT 15, s.id, name + '" has not activity. You must select an activity for this skill'  
		FROM t_fc5_skills s
		WHERE activity_id IS NULL




/* 
	START Inform about too long names
*/
SET @error_number = 16

	--henrikl 100824, added +1 for space between unit and emp type names
	--henrikl 100824, added "or sc.unit_id=-1"
	IF (SELECT COUNT(*) FROM shift_classes sc 
			INNER JOIN employment_types et ON sc.employment_type = et.emp_type_id
			INNER JOIN units u ON sc.unit_id = u.unit_id or sc.unit_id=-1
		WHERE LEN(et.emp_type_desc) + LEN(COALESCE(u.unit_desc,u.unit_desc_nonuni))+1 > 50) > 0
		
	INSERT INTO #errors
		SELECT distinct 16, 'Too long emp type and unit names',et.emp_type_desc + '/' + COALESCE(u.unit_desc,u.unit_desc_nonuni) + ' Emp type + Unit name is ' + CONVERT(varchar(3),LEN(et.emp_type_desc) + LEN(COALESCE(u.unit_desc,u.unit_desc_nonuni))+1) + ' characters, max allowed 50'
		FROM shift_classes sc 
			INNER JOIN employment_types et ON sc.employment_type = et.emp_type_id
			INNER JOIN units u ON sc.unit_id = u.unit_id or sc.unit_id=-1
		WHERE LEN(et.emp_type_desc) + LEN(COALESCE(u.unit_desc,u.unit_desc_nonuni))+1 > 50 

/*
	START Check max length schedule note
*/

SET @error_number = 17

	IF (SELECT COUNT(*) FROM schedule_notes WHERE LEN([text]) > 255) > 0
		
	INSERT INTO #errors
		SELECT distinct 17, 'Too long schedule note for agent ', CONVERT(nvarchar(20), emp.emp_id) + ' - ' + COALESCE(emp.f_name, emp.f_name_nonuni) 
		+ ' ' + COALESCE(emp.l_name, emp.l_name_nonuni) 
		+ ' date ' + CONVERT(nvarchar(20), sn.date, 107)		
		+ ' Characters max allowed 255'
		FROM schedule_notes sn 
			INNER JOIN employees emp ON sn.emp_id = emp.emp_id
			INNER JOIN workspaces w ON sn.workspace_id = w.ws_id

/*
	START Check if skill activity is connected to absence
*/

SET @error_number = 18
	IF (select count(*) from t_fc5_skills s
		inner join absences a on a.activity_id = s.activity_id
		inner join activities ac on ac.activity_id =a.activity_id) >0
	
	INSERT INTO #errors	
	SELECT distinct 18, ac.activity_id, COALESCE(ac.activity_name_nonuni,ac.activity_name) + '" cannot be both skill activity and connected to an absence'  
	from t_fc5_skills s
		inner join absences a on a.activity_id = s.activity_id
		inner join activities ac on ac.activity_id =a.activity_id


/*
	START Check intervals smaller than 15 min
*/

SET @error_number = 19

	IF (SELECT system_setting_value FROM system_settings WHERE system_setting_id = 1 and system_setting_value > 96) > 0

	INSERT INTO #errors
		SELECT distinct 18,'Smaller intervals than 15 min','You have smaller intervals than 15 min. When converting you should convert to 15 or 30 min intervals.'
		
		
--henrikl
SET @error_number = 20
	
	--IF EXISTS (select 1 from t_employee_workrules where pref_from > pref_to)
	INSERT INTO #errors	
	SELECT @error_number, 
		'In work rules an agent cannot have a preference end date which is less than the start date.', 
		coalesce(e.f_name,e.f_name_nonuni)+' '+coalesce(e.l_name,e.l_name_nonuni)+' (emp_id: '+convert(varchar(10),e.emp_id)+') Start date: '+convert(varchar(10),a.pref_from,120)+', End date: '+convert(varchar(10),a.pref_to,120)
	from t_employee_workrules a
	join employees e on a.emp_id = e.emp_id
	where pref_from > pref_to

/*
	START Check that late_start is greter or equal to early start for breaks.
*/

SET @error_number = 21
	
	INSERT INTO #errors	
	SELECT @error_number, 'Early_Start is greater then Late_Start on breaks, on ClassID:',CONVERT(VARCHAR(10),shift_class_id) FROM shift_breakes 
	WHERE break_late_start < break_early_start
	
/*
	Find deleted activities connected to skills
*/

SET @error_number = 22	

	IF (SELECT COUNT(*) FROM t_fc5_skills s INNER JOIN activities a ON
		s.activity_id = a.activity_id
		WHERE a.deleted = 1
		AND s.deleted IS NULL) > 0
		
	INSERT INTO #errors
		SELECT @error_number, s.id, 'Skill "' + s.name + '" is using a deleted activity, "' + a.activity_name_nonuni + '" with activity_id = ' + CONVERT(VARCHAR(5),a.activity_id)
		FROM t_fc5_skills s INNER JOIN activities a ON
		s.activity_id = a.activity_id
		WHERE a.deleted = 1
		AND s.deleted IS NULL
		
	SELECT * FROM #errors
	
END

 GO
