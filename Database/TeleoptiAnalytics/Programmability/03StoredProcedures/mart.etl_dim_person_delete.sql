IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_person_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_person_delete]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-09-xx
-- Description:	Deletes fact-tables dependent on mart.dim_person
--				
-- =============================================
-- Change Log:
-- Date			By		Description
-- =============================================
-- 2010-10-14	KJ		Clean up bridge_group_page_person
-- 2009-09-24	DJ		Clean up of deleted tables
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_person_delete]
	
AS

create table #top (person_id int)

insert into #top
select top(10) person_id from mart.dim_person where to_be_deleted = 1

--------------------------------------------------------------------------
-- Delete all fact for ToBeDeleted dimensions
-------------------------------------------------------------------------

-- fact_schedule
DELETE FROM mart.fact_schedule 
FROM mart.fact_schedule AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

-- fact_schedule_preference
DELETE FROM mart.fact_schedule_preference
FROM mart.fact_schedule_preference AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

-- fact_schedule_day_count
DELETE FROM mart.fact_schedule_day_count
FROM mart.fact_schedule_day_count AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

-- fact_schedule_deviation
DELETE FROM mart.fact_schedule_deviation
FROM mart.fact_schedule_deviation AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

-- bridge_acd_login_person
DELETE FROM mart.bridge_acd_login_person
FROM mart.bridge_acd_login_person AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

--bridge_group_page_person
DELETE FROM mart.bridge_group_page_person
FROM mart.bridge_group_page_person AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

DELETE FROM mart.fact_requested_days
FROM mart.fact_requested_days AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

DELETE FROM mart.fact_request
FROM mart.fact_request AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

DELETE FROM mart.fact_agent_skill
FROM mart.fact_agent_skill AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id


DELETE FROM mart.fact_hourly_availability
FROM mart.fact_hourly_availability AS fact
    INNER JOIN #top AS dim
    ON fact.person_id = dim.person_id

-- dim_person
DELETE mart.dim_person
FROM mart.dim_person p
INNER JOIN #top t
ON p.person_id = t.person_id

GO