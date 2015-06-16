IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[remove_old_tables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[remove_old_tables]
GO

--exec [mart].[remove_old_tables]
CREATE PROCEDURE [mart].[remove_old_tables]
AS
SET NOCOUNT OFF
--====================
--Drop tables left behind
--====================
DECLARE  @tables_removed TABLE(removed_rows int)

--fact_agent_old
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'mart' 
                 AND  TABLE_NAME = 'fact_agent_old'))
AND (NOT EXISTS (
		SELECT * FROM mart.etl_job_delayed
		)
	)
BEGIN
    DROP TABLE mart.fact_agent_old
	INSERT INTO @tables_removed SELECT 1
END

--fact_queue_old
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'mart' 
                 AND  TABLE_NAME = 'fact_queue_old'))
AND (NOT EXISTS (
		SELECT * FROM mart.etl_job_delayed
		)
	)
BEGIN
    DROP TABLE mart.fact_queue_old
	INSERT INTO @tables_removed SELECT 1
END


--fact_schedule_day_count_old
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'mart' 
                 AND  TABLE_NAME = 'fact_schedule_day_count_old'))
AND (NOT EXISTS (
		SELECT * FROM mart.etl_job_delayed
		)
	)
BEGIN
    DROP TABLE mart.fact_schedule_day_count_old
	INSERT INTO @tables_removed SELECT 1
END


--fact_schedule_deviation_old
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'mart' 
                 AND  TABLE_NAME = 'fact_schedule_deviation_old'))
AND (NOT EXISTS (
		SELECT * FROM mart.etl_job_delayed
		)
	)
BEGIN
    DROP TABLE mart.fact_schedule_deviation_old
	INSERT INTO @tables_removed SELECT 1
END

--fact_agent_queue_old
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'mart' 
                 AND  TABLE_NAME = 'fact_agent_queue_old'))
AND (NOT EXISTS (
		SELECT * FROM mart.etl_job_delayed
		)
	)
BEGIN
    DROP TABLE mart.fact_agent_queue_old
	INSERT INTO @tables_removed SELECT 1
END


--fact_schedule_old
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'mart' 
                 AND  TABLE_NAME = 'fact_schedule_old'))
AND (NOT EXISTS (
		SELECT * FROM mart.etl_job_delayed
		)
	)
BEGIN
    DROP TABLE mart.fact_schedule_old
	INSERT INTO @tables_removed SELECT 1 
END
SELECT  * FROM @tables_removed
GO