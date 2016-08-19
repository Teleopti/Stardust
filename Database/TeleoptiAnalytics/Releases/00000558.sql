--FETCH DUPLICATES
SELECT dp.person_period_code, person_id AS remove_person_id, keep_person_id , valid_from_date_id_local
INTO #dupl_persons
FROM [mart].[dim_person] dp
JOIN (SELECT person_period_code, MIN(person_id) AS keep_person_id
  FROM [mart].[dim_person]
  GROUP BY person_period_code HAVING COUNT(person_period_code) > 1) err ON dp.person_period_code = err.person_period_code
WHERE person_id <> keep_person_id

DELETE FROM mart.fact_schedule 
FROM mart.fact_schedule AS fact
    INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id
WHERE fact.shift_startdate_local_id>=(SELECT valid_from_date_id_local FROM #dupl_persons WHERE dupl.remove_person_id=#dupl_persons.remove_person_id)


-- fact_schedule_preference
DELETE FROM mart.fact_schedule_preference
FROM mart.fact_schedule_preference AS fact
  INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id
WHERE fact.date_id>=(SELECT valid_from_date_id_local-1 FROM #dupl_persons WHERE dupl.remove_person_id=#dupl_persons.remove_person_id)

-- fact_schedule_day_count
DELETE FROM mart.fact_schedule_day_count
FROM mart.fact_schedule_day_count AS fact
  INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id

-- fact_schedule_deviation
DELETE FROM mart.fact_schedule_deviation
FROM mart.fact_schedule_deviation AS fact
    INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id
WHERE fact.shift_startdate_local_id>=(SELECT valid_from_date_id_local FROM #dupl_persons WHERE dupl.remove_person_id=#dupl_persons.remove_person_id)

-- bridge_acd_login_person
DELETE FROM mart.bridge_acd_login_person
FROM mart.bridge_acd_login_person AS fact
  INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id

--bridge_group_page_person
DELETE FROM mart.bridge_group_page_person
FROM mart.bridge_group_page_person AS fact
  INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id

DELETE FROM mart.fact_requested_days
FROM mart.fact_requested_days AS fact
  INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id
WHERE fact.request_date_id>=(SELECT valid_from_date_id_local-1 FROM #dupl_persons WHERE dupl.remove_person_id=#dupl_persons.remove_person_id)

DELETE FROM mart.fact_request
FROM mart.fact_request AS fact
  INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id
WHERE fact.request_start_date_id>=(SELECT valid_from_date_id_local-1 FROM #dupl_persons WHERE dupl.remove_person_id=#dupl_persons.remove_person_id)

DELETE FROM mart.fact_agent_skill
FROM mart.fact_agent_skill AS fact
  INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id

DELETE FROM mart.fact_hourly_availability
FROM mart.fact_hourly_availability AS fact
  INNER JOIN #dupl_persons AS dupl
    ON fact.person_id = dupl.remove_person_id

--let the mart.etl_dim_person_delete in Nightly handle the delete from dim_person, but we want to be able to add constraint on person_period_code so reset
UPDATE mart.dim_person
SET to_be_deleted= 1,
	person_code = newid(), 
	person_period_code=newid(),
	person_name = '<to be deleted>'
FROM mart.dim_person p
 INNER JOIN #dupl_persons AS dupl
ON p.person_id = dupl.remove_person_id

DROP TABLE #dupl_persons
--finally add constraint
ALTER TABLE [mart].[dim_person] ADD CONSTRAINT AK_person_period_code UNIQUE (person_period_code)

