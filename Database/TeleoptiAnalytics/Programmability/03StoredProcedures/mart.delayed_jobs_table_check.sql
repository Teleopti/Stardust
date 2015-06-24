IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[delayed_jobs_table_check]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[delayed_jobs_table_check]
GO

--exec [mart].[delayed_jobs_table_check]
CREATE PROCEDURE [mart].[delayed_jobs_table_check]
AS
SET NOCOUNT OFF
--====================
--Drop tables left behind
--====================
DECLARE  @tables_exists TABLE(table_count int)


--fact_agent_old
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'mart' AND  TABLE_NAME = 'fact_agent_old'))
INSERT INTO @tables_exists SELECT 1


--fact_queue_old
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'mart' AND  TABLE_NAME = 'fact_queue_old'))
INSERT INTO @tables_exists SELECT 1


--fact_schedule_day_count_old
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'mart' AND  TABLE_NAME = 'fact_schedule_day_count_old'))
INSERT INTO @tables_exists SELECT 1


--fact_schedule_deviation_old
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'mart' AND  TABLE_NAME = 'fact_schedule_deviation_old'))
INSERT INTO @tables_exists SELECT 1

--fact_agent_queue_old
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'mart' AND  TABLE_NAME = 'fact_agent_queue_old'))
INSERT INTO @tables_exists SELECT 1


--fact_schedule_old
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'mart' AND  TABLE_NAME = 'fact_schedule_old'))
INSERT INTO @tables_exists SELECT 1
SELECT  * FROM @tables_exists
GO