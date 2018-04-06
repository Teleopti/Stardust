IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_job_disable_schedule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[etl_job_disable_schedule]
GO