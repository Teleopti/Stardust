:SETVAR TELEOPTIAGG main_DemoSales_TeleoptiCCCAgg
:SETVAR TELEOPTIANALYTICS DJ
:SETVAR TELEOPTIAPP main_DemoSales_TeleoptiCCC7

declare @SQL2008R2 nvarchar(5)
declare @productversion nvarchar(5)
set @SQL2008R2 = '10.50'
set @productversion = substring(cast(SERVERPROPERTY('productversion') as nvarchar(10)),1,5)

IF @productversion<>@SQL2008R2
Raiserror ('You must fix and backup your demo database on SQL 2008 R2 in order for others to restore databases on that version',20,1) WITH LOG

--delete all fact data
truncate table $(TELEOPTIANALYTICS).mart.fact_agent
truncate table $(TELEOPTIANALYTICS).mart.fact_queue
truncate table $(TELEOPTIANALYTICS).mart.fact_agent_queue
truncate table $(TELEOPTIANALYTICS).mart.fact_agent_skill
truncate table $(TELEOPTIANALYTICS).mart.fact_forecast_workload
truncate table $(TELEOPTIANALYTICS).mart.fact_hourly_availability
truncate table $(TELEOPTIANALYTICS).mart.fact_kpi_targets_team
truncate table $(TELEOPTIANALYTICS).mart.fact_quality
truncate table $(TELEOPTIANALYTICS).mart.fact_request
truncate table $(TELEOPTIANALYTICS).mart.fact_requested_days
truncate table $(TELEOPTIANALYTICS).mart.fact_schedule
truncate table $(TELEOPTIANALYTICS).mart.fact_schedule_day_count
--truncate table $(TELEOPTIANALYTICS).mart.fact_schedule_deviation
truncate table $(TELEOPTIANALYTICS).mart.fact_schedule_forecast_skill
truncate table $(TELEOPTIANALYTICS).mart.fact_schedule_preference
truncate table $(TELEOPTIANALYTICS).mart.bridge_time_zone

--clean out unwanted data
declare @TemplateEndDate datetime
declare @TemplateStartDate datetime
set @TemplateEndDate = '2013-03-03'
set @TemplateStartDate = '2013-02-04'

delete from f
FROM $(TELEOPTIANALYTICS).mart.fact_schedule_deviation f
INNER JOIN $(TELEOPTIANALYTICS).mart.dim_date d
	ON d.date_id = f.date_id
 WHERE d.date_date > @TemplateEndDate --unwanted days after template period
delete from $(TELEOPTIAGG).dbo.agent_logg where date_from > @TemplateEndDate --unwanted days after template period
delete from $(TELEOPTIAGG).dbo.queue_logg where date_from > @TemplateEndDate --unwanted days after template period

update $(TELEOPTIANALYTICS).dbo.aspnet_users set LoweredUserName=''
update $(TELEOPTIANALYTICS).dbo.aspnet_users set UserName=''

delete b
from $(TELEOPTIANALYTICS).mart.dim_date d
inner join $(TELEOPTIANALYTICS).mart.bridge_time_zone b
on d.date_id = b.date_id
where d.date_date > '2015-05-20 00:00:00'

delete from $(TELEOPTIAPP).dbo.WindowsAuthenticationInfo
delete $(TELEOPTIAPP).dbo.license
delete $(TELEOPTIAPP).dbo.licensestatus

truncate table $(TELEOPTIANALYTICS).stage.stg_request
truncate table $(TELEOPTIANALYTICS).stage.stg_skill
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule_changed
truncate table $(TELEOPTIANALYTICS).stage.stg_acd_login_person
truncate table $(TELEOPTIANALYTICS).stage.stg_person
truncate table $(TELEOPTIANALYTICS).stage.stg_overtime
truncate table $(TELEOPTIANALYTICS).stage.stg_queue
truncate table $(TELEOPTIANALYTICS).stage.stg_agent_skill
truncate table $(TELEOPTIANALYTICS).stage.stg_agent_skillset
truncate table $(TELEOPTIANALYTICS).stage.stg_business_unit
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule
truncate table $(TELEOPTIANALYTICS).stage.stg_kpi
truncate table $(TELEOPTIANALYTICS).stage.stg_kpi_targets_team
truncate table $(TELEOPTIANALYTICS).stage.stg_user
truncate table $(TELEOPTIANALYTICS).stage.stg_scenario
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule_updated_personLocal
truncate table $(TELEOPTIANALYTICS).stage.stg_scorecard
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule_updated_ShiftStartDateUTC
truncate table $(TELEOPTIANALYTICS).stage.stg_shift_category
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule_forecast_skill
truncate table $(TELEOPTIANALYTICS).stage.stg_time_zone
truncate table $(TELEOPTIANALYTICS).stage.stg_time_zone_bridge
truncate table $(TELEOPTIANALYTICS).stage.stg_permission_report
truncate table $(TELEOPTIANALYTICS).stage.stg_day_off
truncate table $(TELEOPTIANALYTICS).stage.stg_workload
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule_day_off_count
truncate table $(TELEOPTIANALYTICS).stage.stg_forecast_workload
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule_day_absence_count
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule_preference
truncate table $(TELEOPTIANALYTICS).stage.stg_queue_workload
truncate table $(TELEOPTIANALYTICS).stage.stg_absence
truncate table $(TELEOPTIANALYTICS).stage.stg_activity
truncate table $(TELEOPTIANALYTICS).stage.stg_date
truncate table $(TELEOPTIANALYTICS).stage.stg_scorecard_kpi
truncate table $(TELEOPTIANALYTICS).stage.stg_group_page_person
truncate table $(TELEOPTIANALYTICS).stage.stg_time_zone_bridge
truncate table $(TELEOPTIANALYTICS).stage.stg_schedule_forecast_skill
truncate table $(TELEOPTIANALYTICS).stage.stg_person
GO

--shrink databases
USE $(TELEOPTIANALYTICS)
GO
dbcc shrinkfile(TeleoptiAnalytics_Primary,1)
GO
dbcc shrinkfile(TeleoptiAnalytics_Stage,1)
GO
dbcc shrinkfile(TeleoptiAnalytics_Msg,1)
GO
dbcc shrinkfile(TeleoptiAnalytics_Rta,1)
GO
dbcc shrinkfile(TeleoptiAnalytics_Mart,1)
GO
dbcc shrinkfile(TeleoptiAnalytics_Log,1)
GO

sp_helpfile
GO

USE $(TELEOPTIAPP)
GO
dbcc shrinkfile(TeleoptiCCC7_Data,1)
GO
dbcc shrinkfile(TeleoptiCCC7_Log,1)
GO
sp_helpfile
GO

USE $(TELEOPTIAGG)
GO
dbcc shrinkfile(TeleoptiCCCAgg_Data,1)
GO
dbcc shrinkfile(TeleoptiCCCAgg_Log,1)
GO
sp_helpfile
GO

--backup
backup database $(TELEOPTIAGG) TO disk='c:\TeleoptiCCC7Agg_Demo.bak' WITH INIT, stats=5
backup database $(TELEOPTIAPP) TO disk='c:\TeleoptiCCC7_Demo.bak' WITH INIT, stats=5
backup database $(TELEOPTIANALYTICS) TO disk='c:\TeleoptiAnalytics_Demo.bak' WITH INIT, stats=5
backup database $(TELEOPTIAGG) TO disk='c:\DemoSales_TeleoptiCCCAgg.bak' WITH INIT, stats=5
backup database $(TELEOPTIAPP) TO disk='c:\DemoSales_TeleoptiCCC7.bak' WITH INIT, stats=5
backup database $(TELEOPTIANALYTICS) TO disk='c:\DemoSales_TeleoptiAnalytics.bak' WITH INIT, stats=5