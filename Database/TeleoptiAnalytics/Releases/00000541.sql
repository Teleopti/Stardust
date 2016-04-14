IF NOT EXISTS (SELECT * FROM [mart].[etl_job] WHERE job_id = 15)
	INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (15, N'Reload datamart (old nightly)')
GO
