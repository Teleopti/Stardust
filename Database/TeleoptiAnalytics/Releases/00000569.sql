PRINT 'Adding new indexes for schedule tables. Please be patient...'
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_deviation]') AND name = N'IX_fact_schedule_deviation_person_shiftstart_local')
BEGIN
	DROP INDEX [IX_fact_schedule_deviation_person_shiftstart_local] 
	ON [mart].[fact_schedule_deviation]
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_deviation]') AND name = N'IX_fact_schedule_deviation_person_id_shift_startdate_local_id')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_fact_schedule_deviation_person_id_shift_startdate_local_id] 
	ON [mart].[fact_schedule_deviation]
	(
		[person_id] ASC,
		[shift_startdate_local_id] ASC
	)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]') AND name = N'IX_fact_schedule_day_count_person_id_shift_startdate_local_id')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_fact_schedule_day_count_person_id_shift_startdate_local_id] 
	ON [mart].[fact_schedule_day_count]
	(
		[person_id] ASC
	)
	INCLUDE ([shift_startdate_local_id])
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_person_shift_startdate_local_id')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_fact_schedule_person_shift_startdate_local_id] 
	ON [mart].[fact_schedule]
	(
		[person_id] ASC,
		[shift_startdate_local_id] ASC
	)
END
GO