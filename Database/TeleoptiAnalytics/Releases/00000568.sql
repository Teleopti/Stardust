IF NOT EXISTS (SELECT * FROM [mart].[etl_jobstep] WHERE jobstep_id = 98)
	INSERT [mart].[etl_jobstep](jobstep_id, jobstep_name, insert_date, update_date)
	SELECT 98, 'dim_person update max date',GETDATE(),GETDATE()
