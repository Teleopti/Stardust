IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_callguide_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_callguide_agent]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_callguide_agent]
@main_id int,
@node_id int,
@agent_id int,
@agent_name nvarchar(200),
@event nvarchar(200),
@agent_status nvarchar(200),
@work_level nvarchar(200)

/*
Purpose: This procedure recieves data from the callguide log 
and inserts data in the callguide table
By: 2005-09-06 Peder
Parameters: 	@main_id = site id
		@node_id = TeleoptiLog node id
		@agent_id = Callguide's unique id for the agent 
		@agent_name = Name of the agent, from Callguide
		@event = The name of the event that fired
		@agent_status = Name of the status
		@work_level = The name of the worklevel
*/

AS
SET NOCOUNT ON

--DECLARE @workmode_code int
DECLARE @workmode nvarchar(100)

if @event = 'AgentLogin'
BEGIN
	SELECT @agent_status = 'paused'
	SELECT @work_level = 'private'
END

if @event = 'AgentLogout'
BEGIN
	SELECT @agent_status = 'logout'
	SELECT @work_level = ''

	--SELECT @workmode_code = 120
END

SELECT @workmode = @agent_status + ' ' + @work_level
SELECT @workmode = ltrim(rtrim(@workmode))

--SELECT @workmode_code = workmode_code FROM t_rta_states WHERE state_name = @workmode

IF NOT EXISTS (SELECT 1 FROM t_log_NET_data_callguide_agent WHERE main_id = @main_id AND agent_id = @agent_id)
BEGIN
	INSERT INTO t_log_NET_data_callguide_agent(main_id, node_id, agent_id, agent_name, event, agent_status, work_level, updated, workmode_code)
	VALUES (@main_id, @node_id, @agent_id, @agent_name, @event, @agent_status, @work_level, 1, @workmode)
END
ELSE
BEGIN
	UPDATE t_log_NET_data_callguide_agent
		SET agent_name = @agent_name
		, event = @event
		, agent_status = @agent_status
		, work_level = @work_level
		, updated = 1
		, workmode_code = @workmode
	WHERE main_id = @main_id
	AND node_id = @node_id
	AND agent_id = @agent_id
END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

