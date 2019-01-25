IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_sts_update_agent_logg]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_sts_update_agent_logg]
GO

CREATE PROCEDURE [dbo].[p_sts_update_agent_logg]
@log_object_id int,
@last_logg_date smalldatetime,
@last_logg_interval int

AS

set nocount on 

begin transaction


--l�gg till '-1' k� ifall den inte finns
insert into agent_logg(
	queue,
	date_from,
	interval,
	agent_id, 
	agent_name,
	avail_dur,		
	tot_work_dur,		
	talking_call_dur,	
	pause_dur,		
	wait_dur,		
	wrap_up_dur,		
	answ_call_cnt,
	direct_out_call_cnt,
	direct_out_call_dur,	
	direct_in_call_cnt,
	direct_in_call_dur,	
	transfer_out_call_cnt,
	admin_dur)
select	qs.queue,	-- queue
	ast.date_from,	-- date_from
	ast.interval,	-- interval
	ast.agent_id,	-- agent_id
	af.agent_name,	-- agent_name
	0, 		-- avail_dur,		
	0, 		-- tot_work_dur,		
	0, 		-- talking_call_dur,	
	0, 		-- pause_dur,		
	0, 		-- wait_dur,		
	0, 		-- wrap_up_dur,		
	0, 		-- answ_call_cnt,
	0, 		-- direct_out_call_cnt,
	0, 		-- direct_out_call_dur,	
	0, 		-- direct_in_call_cnt,
	0, 		-- direct_in_call_dur,	
	0, 		-- transfer_out_call_cnt,
	0 		-- admin_dur)
from agent_state_logg as ast
inner join queues as qs on qs.orig_queue_id = '-1'
inner join agent_info as af on af.agent_id = ast.agent_id
where not exists(select agent_id from agent_logg where queue = qs.queue
						and date_from = ast.date_from
						and interval = ast.interval
						and agent_id = ast.agent_id
						and qs.log_object_id = @log_object_id)
group by qs.queue,ast.date_from,ast.interval,ast.agent_id,af.agent_name

if @@error <> 0
begin
	select 'ERROR: Rollback issued!!!'
	rollback transaction 
	return
end


--uppdatera poster p� k� '-1' i agent_logg
update agent_logg set admin_dur = (select isnull(sum(ast.state_dur),0) from agent_state_logg as ast
					inner join agent_states as st on st.state_id = ast.state_id
					inner join queues as qs on qs.queue = al.queue
					where st.is_admin = 1 
					and st.is_active = 1
					and al.date_from = ast.date_from
					and qs.orig_queue_id = '-1'
					and al.agent_id = ast.agent_id
					and al.interval = ast.interval
					and qs.log_object_id = @log_object_id),
			avail_dur = (select isnull(ast.state_dur,0) from agent_state_logg as ast
					inner join agent_states as st on st.state_id = ast.state_id
					inner join queues as qs on qs.queue = al.queue
					where ast.state_id = (select state_id from agent_states where state_name = 'avail')
					and al.date_from = ast.date_from
					and qs.orig_queue_id = '-1'
					and al.agent_id = ast.agent_id
					and al.interval = ast.interval
					and qs.log_object_id = @log_object_id),
			pause_dur =  (select isnull(sum(ast.state_dur),0) from agent_state_logg as ast
					inner join agent_states as st on st.state_id = ast.state_id
					inner join queues as qs on qs.queue = al.queue
					where st.is_paus = 1 
					and st.is_active = 1
					and al.date_from = ast.date_from
					and qs.orig_queue_id = '-1'
					and al.agent_id = ast.agent_id
					and al.interval = ast.interval
					and qs.log_object_id = @log_object_id),
			wrap_up_dur =  (select isnull(sum(ast.state_dur),0) from agent_state_logg as ast
					inner join agent_states as st on st.state_id = ast.state_id
					inner join queues as qs on qs.queue = al.queue
					where st.is_wrap = 1 
					and st.is_active = 1
					and al.date_from = ast.date_from
					and qs.orig_queue_id = '-1'
					and al.agent_id = ast.agent_id
					and al.interval = ast.interval
					and qs.log_object_id = @log_object_id)
from agent_logg al
where al.date_from >= @last_logg_date
and al.interval >= (select case when al.date_from = @last_logg_date then @last_logg_interval else 0 end)
and al.queue = (select queue from queues where orig_queue_id = '-1')


if @@error <> 0
begin
	select 'ERROR: Rollback issued!!!'
	rollback transaction 
	return
end


commit transaction

GO

