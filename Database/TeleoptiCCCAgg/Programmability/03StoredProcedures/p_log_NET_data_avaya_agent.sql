IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_avaya_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_avaya_agent]
GO

CREATE PROCEDURE [dbo].[p_log_NET_data_avaya_agent] 

@main_id int,
@node_id int,
@TS int,
@EXTENSION	int,		-- EXTENSION
@WORKMODE_DIRECTION int,	-- WORKMODE DIRECTION(AGSTATE)
@AGDURATION int,		-- AGDURATION
@AUXREASON	 int,		-- AUXREASON
@DA_INQUEUE int,		-- DA_INQUEUE
@WORKSKILL	int,		-- WORKSKILL
@ACDONHOLD	 int,		-- ACDONHOLD
@ACD	int,			-- ACD
@LOGID int			--LOGID


AS

IF NOT exists (SELECT 1 FROM t_log_NET_data_avaya_agent WHERE main_id = @main_id  AND logid = @LOGID)
BEGIN
	INSERT INTO t_log_NET_data_avaya_agent(main_id, node_id, time_stamp, extension, workmode_direction, ag_duration, auxreason, da_inqueue, workskill, acdonhold, acd, logid)
	SELECT
	@main_id ,
	@node_id,
	@TS ,
	@EXTENSION	,		
	@WORKMODE_DIRECTION ,	
	@AGDURATION ,		
	@AUXREASON	 ,		
	@DA_INQUEUE ,		
	@WORKSKILL	,		
	@ACDONHOLD	 ,		
	@ACD	,			
	@LOGID 			
END
ELSE
BEGIN
	UPDATE t_log_NET_data_avaya_agent
	
	SET time_stamp=@TS, 
		workmode_direction=@WORKMODE_DIRECTION, 
		ag_duration=@AGDURATION, 
		auxreason=@AUXREASON, 
		da_inqueue=@DA_INQUEUE, 
		workskill=@WORKSKILL, 
		acdonhold=@ACDONHOLD, 
		acd=@ACD,
		extension=@EXTENSION,
		updated=1
	WHERE main_id=@main_id
	AND logid=@LOGID
	AND node_id = @node_id
	
END

GO

