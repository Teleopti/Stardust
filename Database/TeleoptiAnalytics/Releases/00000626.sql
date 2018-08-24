ALTER TABLE [mart].[etl_job_execution]
ADD [tenant_name] nvarchar(255) null

GO

UPDATE history
   SET history.tenant_name = schedule.tenant_name
  FROM [mart].[etl_job_execution] history
  JOIN [mart].[etl_job_schedule] schedule
    ON history.schedule_id = schedule.schedule_id
 WHERE history.schedule_id > 0

GO
