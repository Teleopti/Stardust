IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_avaya_agent_init]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_avaya_agent_init]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_avaya_agent_init]
@main_id int,
@node_id int

AS

BEGIN TRAN avaya_agent_init_tran
DELETE FROM t_log_NET_data_avaya_agent
WHERE main_id=@main_id AND node_id = @node_id
COMMIT TRAN avaya_agent_init_tran

GO

