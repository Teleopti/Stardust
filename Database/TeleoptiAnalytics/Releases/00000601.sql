CREATE TABLE mart.bug_47571 (
	person_code uniqueidentifier not null,
	datasource_update_date smalldatetime not null,
	isFixed tinyint not null
)
ALTER TABLE mart.bug_47571 ADD  CONSTRAINT [PK_bug_47571] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC
)


-- Get all agents
INSERT INTO mart.bug_47571
SELECT
	person_code = person_code,
	datasource_update_date = max(datasource_update_date),
	isFixed = 0 --default value => not fixed yet
FROM mart.dim_person p
WHERE p.person_id <>-1
--AND NOT EXISTS(SELECT 1 FROM mart.bug_47571 b WHERE b.person_code = p.person_code)
GROUP BY person_code


-- New ETL Job and job step
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (16, N'Upgrade Maintenance' )
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (99, N'Delayed Job')


DECLARE @schedule_id int
-- Save/Schedule new ETL job to run every other minute between 00:00-23:55
INSERT INTO Mart.[etl_job_schedule]
	([schedule_name]
	,[enabled]
	,[schedule_type]
	,[occurs_daily_at]
	,[occurs_every_minute]
	,[recurring_starttime]
	,[recurring_endtime]
	,[etl_job_name]
	,[etl_relative_period_start]
	,[etl_relative_period_end]
	,[etl_datasource_id]
	,[description])
VALUES
	('Temporary schedule for Upgrade Maintenance'
	,1
	,1
	,0
	,1
	,0
	,1430
	,'Upgrade Maintenance'
	,0
	,365
	,-1
	,'Occurs every day every 1 minute(s) between 00:00 and 23:55.')

SET @schedule_id = @@IDENTITY

-- Add a new row to mart.etl_job_delayed for first 10 agents
INSERT INTO mart.etl_job_delayed (stored_procedured, parameter_string)
SELECT 'mart.etl_correct_unlinked_personids_47571', '@schedule_id=' + CONVERT(nvarchar(5), @schedule_id)