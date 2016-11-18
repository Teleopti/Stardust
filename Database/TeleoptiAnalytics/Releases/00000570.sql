PRINT 'Adding some new indexes for fact tables. Please be patient...'
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]') AND name = N'IX_fact_schedule_preference_person_id_date_id')
BEGIN
 CREATE NONCLUSTERED INDEX [IX_fact_schedule_preference_person_id_date_id] 
 ON [mart].[fact_schedule_preference]
 (
  [person_id] ASC,
  [date_id] ASC
 )
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_request]') AND name = N'IX_fact_request_person_id_request_start_date_id')
BEGIN
 CREATE NONCLUSTERED INDEX [IX_fact_request_person_id_request_start_date_id] 
 ON [mart].[fact_request]
 (
  [person_id] ASC,
  [request_start_date_id] ASC
 )
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_requested_days]') AND name = N'IX_fact_requested_days_person_id_request_date_id')
BEGIN
 CREATE NONCLUSTERED INDEX [IX_fact_requested_days_person_id_request_date_id] 
 ON [mart].[fact_requested_days]
 (
  [person_id] ASC,
  [request_date_id] ASC
 )
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_hourly_availability]') AND name = N'IX_fact_hourly_availability_person_id_date_id')
BEGIN
 CREATE NONCLUSTERED INDEX [IX_fact_hourly_availability_person_id_date_id] 
 ON [mart].[fact_hourly_availability]
 (
  [person_id] ASC,
  [date_id] ASC
 )
END
GO