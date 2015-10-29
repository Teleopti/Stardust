IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[agent_logg_view]'))
DROP VIEW [dbo].[agent_logg_view]

GO
CREATE VIEW [dbo].[agent_logg_view]
AS
SELECT     queue, date_from, interval, agent_id, agent_name COLLATE database_default as agent_name, avail_dur, tot_work_dur, talking_call_dur, pause_dur, wait_dur, wrap_up_dur, answ_call_cnt, 
                      direct_out_call_cnt, direct_out_call_dur, direct_in_call_cnt, direct_in_call_dur, transfer_out_call_cnt, admin_dur
FROM         dbo.agent_logg_intraday WITH (NOLOCK)
UNION
SELECT     queue, date_from, interval, agent_id, agent_name COLLATE database_default as agent_name, avail_dur, tot_work_dur, talking_call_dur, pause_dur, wait_dur, wrap_up_dur, answ_call_cnt, 
                      direct_out_call_cnt, direct_out_call_dur, direct_in_call_cnt, direct_in_call_dur, transfer_out_call_cnt, admin_dur
FROM         dbo.agent_logg WITH (NOLOCK)
GO
