IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_genesys_ctia_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_genesys_ctia_agent]
GO

CREATE  PROCEDURE [dbo].[p_log_NET_data_genesys_ctia_agent]

@main_id int,
@node_id int,
@agent_id nvarchar(200),
@agent_status  nvarchar(200)

/*
Purpose: This procedure recieves data from the Genesys CTIA bridge log
and inserts data in the t_log_NET_data_genesys_ctia_agent table
By: 2006-12-12 Ma�g
Parameters: 	
		@main_id		= site id
		@agent_id		= Genesys unique id for the agent 
		@agent_status	= Agent status		
*/
AS

IF NOT exists (SELECT 1 FROM t_log_NET_data_genesys_ctia_agent WHERE main_id = @main_id  AND agent_id = @agent_id)
BEGIN
	INSERT INTO t_log_NET_data_genesys_ctia_agent(main_id, agent_id, agent_status, updated)
	VALUES (@main_id, @agent_id, @agent_status, 1)
END
ELSE
BEGIN
	UPDATE t_log_NET_data_genesys_ctia_agent
	
	SET agent_status=@agent_status,
		updated=1
	WHERE main_id=@main_id
	AND agent_id=@agent_id

END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

