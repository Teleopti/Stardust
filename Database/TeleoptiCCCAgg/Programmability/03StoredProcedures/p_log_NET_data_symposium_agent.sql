IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_symposium_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_symposium_agent]
GO

CREATE  PROCEDURE [dbo].[p_log_NET_data_symposium_agent]

@main_id int,
@node_id int,
@Agent_Name nvarchar(100),
@Agent_ID int,
@State int,
@Supervisor_ID int,
@Time_In_State int,
@Answering_Skillset int,
@DN_In_Time_In_State int,
@DN_Out_Time_In_State int,
@Position_ID int



 AS



DECLARE @now DATETIME
SELECT @now = CONVERT(smalldatetime, getdate())

IF NOT exists (SELECT 1 FROM t_log_NET_data_symposium_agent WHERE main_id = @main_id  AND agent_id = @Agent_ID)
BEGIN
	

	INSERT INTO t_log_NET_data_symposium_agent(main_id, agent_id, state, supervisor_id, time_in_state, answering_skillset, dn_in_time_in_state, dn_out_time_in_state, position_id)
	SELECT
	@main_id ,
	@Agent_ID,
	@State,
	@Supervisor_ID,
	@Time_In_State ,
	@Answering_Skillset ,
	@DN_In_Time_In_State ,
	@DN_Out_Time_In_State ,
	--@Supervisor_User_ID 
	@Position_ID
END
ELSE
BEGIN
	UPDATE t_log_NET_data_symposium_agent
	
	SET 	state=@State,
		supervisor_id=@Supervisor_ID,
		time_in_state=@Time_In_State,
 		answering_skillset=@Answering_Skillset,
 		dn_in_time_in_state=@DN_In_Time_In_State,
		dn_out_time_in_state=@DN_Out_Time_In_State,
		position_id=@Position_ID,
		updated=1
	WHERE main_id=@main_id
	AND agent_id=@Agent_ID

END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

