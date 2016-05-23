IF NOT EXISTS (SELECT * FROM [mart].[etl_jobstep] WHERE jobstep_id = 91)
	INSERT [mart].[etl_jobstep](jobstep_id, jobstep_name, insert_date, update_date)
	SELECT 91, 'dim_not_defined_rows',GETDATE(),GETDATE()
