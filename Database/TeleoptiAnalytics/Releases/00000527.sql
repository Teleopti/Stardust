CREATE NONCLUSTERED INDEX [IX_etl_job_execution_job_start_endtime]
ON [mart].[etl_job_execution] 
([job_start_time],[job_end_time])
INCLUDE([schedule_id],[business_unit_code])
GO

