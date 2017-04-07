IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_update_stat]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_update_stat]
GO

CREATE PROCEDURE [dbo].[p_update_stat]
@log_object_id int,
@start_date smalldatetime = '1900-01-01',
@end_date smalldatetime = '1900-01-01',
@rel_start_date int =0,
@rel_start_int int=0
AS
declare @logdb_name varchar(50)
if exists (select 1 from log_object where log_object_id = log_object_id)
begin
	select @logdb_name = ltrim(rtrim(logdb_name))
	from log_object
	where log_object_id = @log_object_id
end
else
begin
	print 'ERROR: Log object with ID='+convert(varchar(5),@log_object_id)+' does not exist in system configuration.'
	return
end
if exists (select 1 from master.dbo.sysdatabases where name = @logdb_name)
begin

	exec p_insert_queue_logg @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int
	exec p_insert_agent_logg @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int
	exec p_insert_goal_results @log_object_id, @start_date, @end_date, @rel_start_date, @rel_start_int


end
else
begin
	print 'ERROR: Log DB with name ='+@logdb_name+' does not exist on the server.'
end

--#43670 if the ETL Nightly too slow (e.g. 5h with larger customers) the Queue and Agent statistics will blocked for that time.
--WORKAROUND: Make sure to update the Analytics DB name and un-commment the next line between version bythat the TLS will execute the "ETL" for you
--exec TeleoptiAnalytics.dbo.p_update_stat_etl @log_object_id

GO

