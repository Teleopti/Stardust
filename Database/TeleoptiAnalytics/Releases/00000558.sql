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

--just in case
--let the mart.etl_dim_person_delete in Nightly handle the delete from dim_person, but we want to be able to add constraint on person_period_code so reset
UPDATE mart.dim_person
SET to_be_deleted= 1,
	person_code = newid(), 
	person_period_code=newid(),
	person_name = '<to be deleted>'
FROM mart.dim_person p
 INNER JOIN #dupl_persons AS dupl
ON p.person_id = dupl.remove_person_id

---drop the FKs to speed up delete
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_person]

ALTER TABLE [mart].[fact_schedule_deviation] DROP CONSTRAINT [FK_fact_schedule_deviation_dim_person]

ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_person]

ALTER TABLE [mart].[fact_schedule_preference] DROP CONSTRAINT [FK_fact_schedule_preference_dim_person]


-- Delete  duplicates from dim_person
DELETE mart.dim_person
FROM mart.dim_person p
INNER JOIN #dupl_persons AS dupl
ON p.person_id = dupl.remove_person_id

DROP TABLE #dupl_persons

--finally add constraint
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='AK_person_period_code')
BEGIN
	ALTER TABLE [mart].[dim_person] ADD CONSTRAINT AK_person_period_code UNIQUE (person_period_code)
END
--readd the FKs again
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])

ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_person]

ALTER TABLE [mart].[fact_schedule_deviation]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])

ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_person]

ALTER TABLE [mart].[fact_schedule_day_count]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])

ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_person]

ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])

ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_person]
