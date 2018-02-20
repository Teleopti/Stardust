ALTER TABLE mart.etl_job_schedule ADD
	tenant_name nvarchar(255) NOT NULL CONSTRAINT DF_etl_job_schedule_tenant_name DEFAULT N'<All>'
GO
