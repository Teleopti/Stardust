INSERT INTO mart.etl_job_delayed(stored_procedured,parameter_string)
SELECT 'mart.etl_delayed_dim_group_page_remove_duplicates','@delete_all=1'
	
	
