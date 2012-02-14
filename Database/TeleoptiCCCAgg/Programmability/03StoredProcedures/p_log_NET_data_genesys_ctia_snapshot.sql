IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_genesys_ctia_snapshot]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_genesys_ctia_snapshot]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_genesys_ctia_snapshot]

@main_id int,
@node_id int,
@cleartable int,
@agent_id_array ntext,
@agent_status_array ntext,
@agent_id_remove_array ntext

/*
Purpose: This procedure recieves data from the Genesys CTIA bridge log
and inserts data in the t_log_NET_data_genesys_ctia_agent table.
The supplied parameters can either be single string values or commaseparated string values.
By: 2006-12-13 Ma�g
Parameters: 	
		@main_id		       = site id
		@cleartable		       = 1 Existing records will be deleted before inserting new ones, 
		                         0 Only supplied agents records will be inserted or updated
		@agent_id_array	       = Commaseparated strings with Genesys unique agent id's to update/insert
		@agent_status_array	   = Commaseparated strings with Genesys agent statuses (corresponding 
		                         to the supplied agent id's)
		@agent_id_remove_array = Commaseparated strings with Genesys unique agent id's that should
		                         be removed from table (i.e. logged out)
*/
AS

-- Table variable to hold the info from the comma separated arrays
DECLARE @tbl_agentrecords TABLE (listpos     int IDENTITY(1, 1) NOT NULL,
                                 agentid     nvarchar(1024),
                                 agentstatus nvarchar(1024))

-- Table variable to hold the info from the comma separated arrays
DECLARE @tbl_agentstoremove TABLE (listpos int IDENTITY(1, 1) NOT NULL,
                                   agentid nvarchar(1024))

-- "Unpack" the two comma separated arrays into our table variable
INSERT @tbl_agentrecords
	SELECT a.nstr, s.nstr
	FROM f_log_NET_charlist_to_table(@agent_id_array, DEFAULT) a
	JOIN f_log_NET_charlist_to_table(@agent_status_array, DEFAULT) s ON a.listpos = s.listpos
	WHERE a.nstr <> ''

-- "Unpack" the comma separated array with agent id's to remove into the table variable
INSERT @tbl_agentstoremove
	SELECT a.nstr
	FROM f_log_NET_charlist_to_table(@agent_id_remove_array, DEFAULT) a
	WHERE a.nstr <> ''

-- Clear the whole table before inserting records ?
IF @cleartable = 1
BEGIN
	BEGIN TRAN genesys_ctia_snapshot
	
	DELETE FROM t_log_NET_data_genesys_ctia_agent
	WHERE main_id=@main_id
	
	INSERT t_log_NET_data_genesys_ctia_agent
	SELECT @main_id, a.agentid, a.agentstatus, 1
	FROM @tbl_agentrecords a
	
	COMMIT TRAN genesys_ctia_snapshot
END
ELSE
-- Just update the supplied agent's statuses (delete/insert or insert/update ?)
BEGIN
	BEGIN TRAN genesys_ctia_snapshot

	-- Remove all loggedout agents
	DELETE FROM t_log_NET_data_genesys_ctia_agent
	WHERE main_id=@main_id AND
	agent_id IN (SELECT convert(int, agentid) FROM @tbl_agentstoremove)
	
	-- Delete/insert all agents 
	DELETE FROM t_log_NET_data_genesys_ctia_agent
	WHERE main_id=@main_id AND
	agent_id IN (SELECT convert(int, agentid) FROM @tbl_agentrecords)
	
	INSERT t_log_NET_data_genesys_ctia_agent
	SELECT @main_id, a.agentid, a.agentstatus, 1
	FROM @tbl_agentrecords a

	COMMIT TRAN genesys_ctia_snapshot
END

GO

