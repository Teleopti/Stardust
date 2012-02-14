IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_symposium_snapshot]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_symposium_snapshot]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_symposium_snapshot]

@main_id int,
@node_id int,
@cleartable int,
@agentrecord_array ntext

/*
@Agent_Name nvarchar(100),
@Agent_ID int,
@State int,
@Supervisor_ID int,
@Time_In_State int,
@Answering_Skillset int,
@DN_In_Time_In_State int,
@DN_Out_Time_In_State int,
@Position_ID int
*/

/*
Purpose: This procedure recieves data from the Symposium log
and inserts data in the t_log_NET_data_symposium_agent table.
By: 2006-12-19 Ma�g
Parameters: 	
		@main_id		       = site id
		@cleartable		       = 1 Existing records will be deleted before inserting new ones, 
		                                 0 Only supplied agents records will be inserted or updated
		@agentrecord_array	       = Commaseparated strings with Symposium specific info to 
                                                 update/insert. Note that the actual values have to be numeric.
*/

AS



-- Table variable to hold the info from the comma separated array
DECLARE @tbl_agentrecords TABLE (listpos int IDENTITY(1, 1) NOT NULL,
                              Agent_ID int,
                              State int,
                              Supervisor_ID int,
                              Time_In_State int,
                              Answering_Skillset int,
                              DN_In_Time_In_State int,
                              DN_Out_Time_In_State int,
                              Position_ID int)


-- "Unpack" the comma separated array into our table variable
INSERT @tbl_agentrecords
	SELECT a.int1, a.int2, a.int3, a.int4, a.int5, a.int6, a.int7, a.int8
	FROM f_log_NET_charlist_to_multicolumn_table_int(@agentrecord_array, 8, ',') a

-- Clear the whole table before inserting records ?
IF @cleartable = 1
BEGIN
    BEGIN TRAN symp_snapshot

    DELETE FROM t_log_NET_data_symposium_agent
    WHERE main_id=@main_id
	
    INSERT t_log_NET_data_symposium_agent
        SELECT @main_id, 
               a.Agent_ID,
	       a.State,
	       a.Supervisor_ID,
	       a.Time_In_State ,
	       a.Answering_Skillset ,
	       a.DN_In_Time_In_State ,
	       a.DN_Out_Time_In_State ,
	       a.Position_ID,
               1
        FROM @tbl_agentrecords a
	
    COMMIT TRAN symp_snapshot
END
ELSE
-- Just update the supplied agent's statuses (delete/insert or insert/update ?)
BEGIN
    BEGIN TRAN symp_snapshot

    -- Delete/insert all agents 
    DELETE FROM t_log_NET_data_symposium_agent
        WHERE main_id=@main_id AND
              agent_id IN (SELECT Agent_ID FROM @tbl_agentrecords)
	
    INSERT t_log_NET_data_symposium_agent
        SELECT @main_id, 
               a.Agent_ID,
	       a.State,
	       a.Supervisor_ID,
	       a.Time_In_State ,
	       a.Answering_Skillset ,
	       a.DN_In_Time_In_State ,
	       a.DN_Out_Time_In_State ,
	       a.Position_ID,
               1
        FROM @tbl_agentrecords a

    COMMIT TRAN symp_snapshot
END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

