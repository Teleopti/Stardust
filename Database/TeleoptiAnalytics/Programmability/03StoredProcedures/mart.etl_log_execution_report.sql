IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_log_execution_report]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_log_execution_report]
GO


-- =============================================
-- Author:		<KJ>
-- Create date: <2008-09-25>
-- Description:	<Execution Report>
-- =============================================
--exec etl_log_execution_report 'Main','stg_schedule','2008-09-01'
--exec etl_log_execution_report 'Main','All','2008-09-01'

CREATE PROCEDURE [mart].[etl_log_execution_report]
@job_name nvarchar(100),
@job_step_name nvarchar(100),
@start_date datetime
AS
BEGIN

SET NOCOUNT ON;
create table #jobs(id int,name nvarchar(100))
create table #jobsteps(id int,name nvarchar(100))

if @job_name='All'
begin
	insert #jobs
	SELECT distinct job_id, job_name
	from Mart.etl_job
END
ELSE
BEGIN
	insert #jobs
	SELECT distinct job_id,job_name from Mart.etl_job WHERE job_name=@job_name
END

if @job_step_name='All'
begin
	insert #jobsteps
	SELECT distinct jobstep_id,jobstep_name from Mart.etl_jobstep
END
ELSE
BEGIN
	insert #jobsteps
	SELECT distinct jobstep_id,jobstep_name from Mart.etl_jobstep WHERE jobstep_name=@job_step_name
END




select je.job_execution_id,j.job_name,je.job_start_time,je.job_end_time, 
		convert(varchar(5),Floor(je.duration_s / 3600)) + ':' + right('00' + convert(varchar(2),floor((je.duration_s % 3600) / 60)),2) + ':' + right('00' + convert(varchar(2),je.duration_s % 60),2) 
		 'total_duration',
		je.duration_s 'total_duration_s',
		je.affected_rows 'total_affected_rows',
		case when je.duration_s<=0 then 0
		else
		je.affected_rows/je.duration_s 
		end as	'total_rows_per_second',
	
		js.jobstep_name,
		convert(varchar(5),Floor(jse.duration_s / 3600)) + ':' + right('00' + convert(varchar(2),floor((jse.duration_s % 3600) / 60)),2) + ':' + right('00' + convert(varchar(2),jse.duration_s % 60),2) 'jobstep_duration',
		jse.rows_affected 'jobstep_rows_affected',
		case when jse.duration_s<=0 then 0
		else
		jse.rows_affected/jse.duration_s 
		end as 'jobstep_rows_per_second'
		
from 
Mart.etl_jobstep_execution jse
inner join Mart.etl_job_execution je on jse.job_execution_id=je.job_execution_id
inner join Mart.etl_job j ON je.job_id=j.job_id
inner join Mart.etl_jobstep js ON js.jobstep_id=jse.jobstep_id
WHERE j.job_id in (SELECT id FROM #jobs)
AND js.jobstep_id in (SELECT id FROM #jobsteps)
and je.job_start_time>@start_date
order by je.job_execution_id,j.job_name,js.jobstep_name

DROP TABLE #JOBS

END


GO

