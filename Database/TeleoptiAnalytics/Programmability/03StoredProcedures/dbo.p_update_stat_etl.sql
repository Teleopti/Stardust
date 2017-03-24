IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_update_stat_etl]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_update_stat_etl]
GO
/*
To be executed either:
1) from p_update_stat
This would be custom workaround that, add this to the very end of p_update_stat:
exec [TeleoptiAnalytics].[dbo].[p_update_stat_etl] @log_object_id 
note: this workaround will be removed every time WFM is patched

or
2) from ToptiLogServer
Still code changes needed for this to happen.
But think right after the p_update_stat have been been executed.

*/
--exec [dbo].[p_update_stat_etl] @log_object_id=2

CREATE PROCEDURE [dbo].[p_update_stat_etl]
@log_object_id int
as

declare @datasource_id int
select @datasource_id = datasource_id from mart.sys_datasource where log_object_id=@log_object_id and inactive=0
if @datasource_id is not null
begin
	exec mart.etl_fact_queue_load_intraday @datasource_id = @datasource_id
	exec mart.etl_fact_agent_load_intraday @datasource_id = @datasource_id
	exec mart.etl_fact_agent_queue_load_intraday @datasource_id = @datasource_id
end
GO