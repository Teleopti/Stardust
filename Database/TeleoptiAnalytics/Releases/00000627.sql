delete jsp from mart.etl_job_schedule_period jsp 
inner join mart.etl_job_schedule js on jsp.schedule_id = js.schedule_id
where js.etl_job_name = 'Nightly' and jsp.job_id = 4

