IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_agent_schedule_web_adherence]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_agent_schedule_web_adherence]
GO

/* 
exec mart.report_data_agent_schedule_web_adherence @date_from='2013-02-05',
@adherence_id=2, @person_code=N'11610fe4-0130-4568-97de-9b5e015b2564', @business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA'

-- =============================================
-- Author:		Ola
-- Create date: 2014-02-24
-- Update date: 

-- Description:	Used by My Adherence Report in My Time.
-- A wrapper round mart.report_data_agent_schedule_adherence
-- =============================================
*/
CREATE proc [mart].[report_data_agent_schedule_web_adherence]
@date_from datetime,
@adherence_id int,
@person_code uniqueidentifier,
@business_unit_code uniqueidentifier
as
set nocount on

declare @time_zone_id int
select @time_zone_id = time_zone_id from mart.dim_person where person_code=@person_code


CREATE TABLE #webresult (
	date datetime,
	interval_id int,
	interval_name nvarchar(20),
	intervals_per_day int,
	site_id int,
	site_name nvarchar(100),
	team_id int,
	team_name nvarchar(100),
	person_id int,
	person_first_name nvarchar(30),
	person_last_name nvarchar(30),
	person_name	nvarchar(200),
	adherence decimal(18,3),
	adherence_tot decimal(18,3),
	deviation_m decimal(18,3),
	deviation_tot_m decimal(18,3),
	ready_time_m decimal(18,3),
	is_logged_in bit not null,
	activity_id int,
	absence_id int,
	display_color int,
	activity_absence_name nvarchar(100),
	team_adherence decimal(18,3),
	team_adherence_tot decimal(18,3),
	team_deviation_m decimal(18,3),
	team_deviation_tot_m decimal(18,3),
	adherence_type_selected nvarchar(100),
	hide_time_zone bit,
	shift_startdate datetime,
	date_interval_counter int
	)

	INSERT INTO #webresult
exec mart.report_data_agent_schedule_adherence 
@date_from=@date_from,
@date_to=@date_from,
@group_page_code=NULL,
@group_page_group_set=NULL,
@group_page_agent_code=NULL,
@site_id=-2,
@team_set=-2,
@agent_person_code=@person_code,
@adherence_id=@adherence_id,
@sort_by=6,
@time_zone_id=@time_zone_id,
@person_code=@person_code,
@report_id='D1ADE4AC-284C-4925-AEDD-A193676DBD2F',
@language_id=null,
@business_unit_code=@business_unit_code,
@from_matrix=0

select shift_startdate AS ShiftDate, 
intervals_per_day as IntervalsPerDay,
CASE 
	WHEN date != shift_startdate THEN  interval_id + intervals_per_day
	ELSE interval_id
END as IntervalId,
adherence Adherence, 
adherence_tot TotalAdherence, 
CAST(ROUND(deviation_m, 0) as int) AS Deviation, 
CASE (activity_id + absence_id)
	WHEN -2 THEN 16777215
	ELSE display_color
END AS DisplayColor,
activity_absence_name AS DisplayName, 
date_interval_counter AS IntervalCounter
from #webresult
GO


