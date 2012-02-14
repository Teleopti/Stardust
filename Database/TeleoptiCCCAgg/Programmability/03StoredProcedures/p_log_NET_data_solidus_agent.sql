IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[p_log_NET_data_solidus_agent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[p_log_NET_data_solidus_agent]
GO

CREATE   PROCEDURE [dbo].[p_log_NET_data_solidus_agent]
@main_id int,
@node_id int,
@event_string nvarchar(4000) = ''

/*
Syfte: Tar emot och splittar en str�ng fr�n Solidusloggen samt
l�ser in datat i t_log_solidus_agent_status
Av: Zo� 2005-03-16
Anv�nds av: 
Parametrar:	@event_string = Str�ngen fr�n loggen


exec p_log_solidus_insert_realtime 1, 'EVENTID=4;RECID=40;'

*/

AS

DECLARE @event_type_id int
DECLARE @agent_id int
DECLARE @voice int
DECLARE @email int
DECLARE @media int
DECLARE @loggedon int

IF @event_string = 'CONNECTED' /*New connection*/
BEGIN
	DELETE FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id
	RETURN
END

IF (LEN(@event_string)>12)
BEGIN
	
	SELECT @event_string = left(REPLACE(@event_string,';',','),len(@event_string))
	
	CREATE TABLE #tmpParams (params nvarchar(200))
	EXEC p_log_NET_functions_split_string @event_string, #tmpParams

	CREATE TABLE #params(param_name nvarchar(50),value nvarchar(200))
	
	INSERT INTO #params
	SELECT LEFT(params,CHARINDEX('=',params)-1),RIGHT(params,LEN(params)-CHARINDEX('=',params)) 
	FROM #tmpParams
	
	SELECT @event_type_id = CONVERT(int,value) FROM #params WHERE param_name = 'EVENTID'
	SELECT @agent_id = CONVERT(int,value) FROM #params WHERE param_name = 'RECID'
	
	/* Set startvalues for the parameters */
	SELECT @voice = -1, @email = -1, @media = -1, @loggedon = -1
	
	IF @event_type_id = 1 /*BROADCAST_LOGON*/
	BEGIN
		SELECT @loggedon = 1
		SELECT @voice = CONVERT(int,value) FROM #params WHERE param_name = 'VoiceReady'
		SELECT @email = CONVERT(int,value) FROM #params WHERE param_name = 'EmailReady'
		SELECT @media = CONVERT(int,value) FROM #params WHERE param_name = 'MediaReady'
	END

	IF @event_type_id = 2 /*BROADCAST_LOGOFF*/
	BEGIN
		SELECT @loggedon = 0
		SELECT @voice = 0
		SELECT @email = 0
		SELECT @media = 0
	END
	
	IF @event_type_id = 3 /*BROADCAST_READY*/
	BEGIN
		SELECT @voice = 1
	END

	IF @event_type_id = 4 /*BROADCAST_NOTREADY*/
	BEGIN
		SELECT @voice = 0
	END	
	
	IF @event_type_id = 5 /*BROADCAST_EMAILREADY*/
	BEGIN
		SELECT @email = 1
	END
	
	IF @event_type_id = 6 /*BROADCAST_EMAILNOTREADY*/
	BEGIN
		SELECT @email = 0
	END
	
	IF @event_type_id = 7 /*BROADCAST_ORIGINATED*/
	BEGIN
		SELECT @voice = 2
	END
	
	IF @event_type_id = 8 /*BROADCAST_DELIVERED*/
	BEGIN
		SELECT @voice = 2
	END	
	
	IF @event_type_id = 9 /*BROADCAST_ESTABLISHED*/
	BEGIN
		SELECT @voice = 2
	END	
	
	IF @event_type_id = 10 /*BROADCAST_HELD*/
	BEGIN
		SELECT @voice = 2
	END	
	
	IF @event_type_id = 11 /*BROADCAST_RETRIEVED*/
	BEGIN
		SELECT @voice = 2
	END
		
	IF @event_type_id = 12 /*BROADCAST_TRANSFERED*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 13 /*BROADCAST_CONFERENCED*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 14 /*BROADCAST_CONNECTIONCLEARED*/
	BEGIN
		SELECT @voice = 1
		IF (SELECT value FROM #params WHERE param_name = 'ClericalFlag' )= '1' /*Busy or Clerical*/
			SELECT @voice = 2

	END

	IF @event_type_id = 15 /*BROADCAST_CALENDED*/
	BEGIN
		DECLARE @tmp_event_id INT
		SELECT @tmp_event_id = event_id FROM t_log_NET_data_solidus_agent 
		WHERE agent_id = @agent_id AND main_id = @main_id
		
		IF @tmp_event_id <> 4
			SELECT @voice = 1
	
	END
	
	IF @event_type_id = 16 /*BROADCAST_CALLREJECTED*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END
	
	IF @event_type_id = 17 /*BROADCAST_CALLBACKACCEPTED*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END

	IF @event_type_id = 18 /*BROADCAST_CALLBACKREJECTED*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END
	
	IF @event_type_id = 19 /*BROADCAST_CALLBACKSTATUS*/
	BEGIN
		SELECT @voice = -1 /*Do nothiing*/
	END

	IF @event_type_id = 20 /*BROADCAST_EMAILREJECT*/
	BEGIN
		SELECT @email = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END
	
	IF @event_type_id = 21 /*BROADCAST_EMAILDELETE*/
	BEGIN
		SELECT @voice = -1 /*Do nothiing*/
	END

	IF @event_type_id = 22 /*BROADCAST_EMAILREPLY*/
	BEGIN
		SELECT @email = 2
	END

	IF @event_type_id = 23 /*BROADCAST_EMAILINFO*/
	BEGIN
		SELECT @email = 2
	END

	IF @event_type_id = 24 /*BROADCAST_CALLINFORMATION*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CALLTYPE' )= '4'
			SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CALLTYPE' )= '5'
			SELECT @email = 2
	END

	IF @event_type_id = 25 /*BROADCAST_EMAILREJECT*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 26 /*BROADCAST_MEDIASTARTED*/
	BEGIN
		SELECT @media = 2
	END

	IF @event_type_id = 27 /*BROADCAST_MEDIASTOPPED*/
	BEGIN
		SELECT @media = 1
	END

	IF @event_type_id = 28 /*BROADCAST_CSRSTATUS*/
	BEGIN
		IF (SELECT value FROM #params WHERE param_name = 'STATUS' )= 'Ready'
			SELECT @media = 1
		IF (SELECT value FROM #params WHERE param_name = 'STATUS' )= 'Not Ready'
			SELECT @media = 0
		IF (SELECT value FROM #params WHERE param_name = 'STATUS' )= 'Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END

	IF @event_type_id = 29 /*BROADCAST_CAMPAIGNACCEPT*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 30 /*BROADCAST_CAMPAIGREJECTED*/
	BEGIN
		SELECT @voice = 2
		IF (SELECT value FROM #params WHERE param_name = 'CAUSE' )= 'Agent Logged Off'
		BEGIN
			SELECT @loggedon = 0
			SELECT @voice = 0
			SELECT @email = 0
			SELECT @media = 0
		END
	END
	
	IF @event_type_id = 31 /*BROADCAST_CAMPAIGNSTATUS*/
	BEGIN
		SELECT @voice = 1
	END

	IF @event_type_id = 32 /*BROADCAST_CAMPAIGNINFO*/
	BEGIN
		SELECT @voice = 2
	END

	IF @event_type_id = 33 /*BROADCAST_SERVICEGROUP_ADDED*/
	BEGIN
		SELECT @voice = -1 /* Do nothing*/
	END

	IF @event_type_id = 34 /*BROADCAST_AGENT_SERVICEGROUPS_CHANGED*/
	BEGIN
		SELECT @voice = -1 /* Do nothing*/
	END

	IF @event_type_id = 35 /*BROADCAST_DATA_COMPLETE*/
	BEGIN
		SELECT @voice = -1 /* Do nothing*/
	END

	IF @event_type_id = 36 /*BROADCAST_DISCONECT*/
	BEGIN
		SELECT @voice = -1 /* Do nothing OR should we delete everything?*/
	END

	IF @event_type_id = 37 /*BROADCAST_XFERC_ALLINFORMATION*/
	BEGIN
		SELECT @voice = 2
	END

	
	IF NOT EXISTS (SELECT 1 FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id AND agent_id = @agent_id)
	BEGIN
		SELECT @loggedon = 1 /*If an event on agent the agent must be loggedon*/

		IF @voice = -1
			SELECT @voice = 0	
		IF @email = -1
			SELECT @email = 0
		IF @media = -1
			SELECT @media = 0	
		
		INSERT INTO t_log_NET_data_solidus_agent(main_id, agent_id, loggedon, voice, email, media, updated, event_id)
		SELECT @main_id, @agent_id, @loggedon, @voice, @email, @media, 1, @event_type_id
	END
	ELSE
	BEGIN
		IF @loggedon <> -1
		BEGIN
			UPDATE t_log_NET_data_solidus_agent SET loggedon = @loggedon, updated = 1, event_id = @event_type_id
			WHERE agent_id = @agent_id 
			AND main_id = @main_id
		END		

		IF @voice <> -1
		BEGIN
			UPDATE t_log_NET_data_solidus_agent SET voice = @voice, updated = 1, event_id = @event_type_id
			WHERE agent_id = @agent_id 
			AND main_id = @main_id
		END

		IF @email <> -1
		BEGIN
			UPDATE t_log_NET_data_solidus_agent SET email = @email, updated = 1, event_id = @event_type_id
			WHERE agent_id = @agent_id 
			AND main_id = @main_id
		END

		IF @media <> -1
		BEGIN
			UPDATE t_log_NET_data_solidus_agent SET media = @media, updated = 1, event_id = @event_type_id 
			WHERE agent_id = @agent_id 
			AND main_id = @main_id
		END
		
		DECLARE @curvoice int, @curemail int, @curmedia int
		SELECT @curvoice = voice FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id AND agent_id = @agent_id
		SELECT @curemail = email FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id AND agent_id = @agent_id
		SELECT @curmedia = media FROM t_log_NET_data_solidus_agent WHERE main_id = @main_id AND agent_id = @agent_id

		IF (@curvoice <> 0 OR @curemail <> 0 OR @curmedia <> 0)
		BEGIN
			UPDATE t_log_NET_data_solidus_agent
			SET loggedon = 1
			WHERE main_id = @main_id
			AND agent_id = @agent_id
		END
	END

	/* Write data to logg table, just for testing. Delete old data */
	IF (SELECT datediff(day,MIN(time),getdate()) FROM t_log_NET_data_solidus_logg) > 0
	BEGIN
		TRUNCATE TABLE t_log_NET_data_solidus_logg
	END
	
	INSERT INTO t_log_NET_data_solidus_logg(main_id, agent_id, event_type_id, parameter, value)
	SELECT @main_id, @agent_id,@event_type_id,param_name, value
	FROM #params

END

-- Added 070320 by Ma�g to support Teleopti PRO alarm features
-- Removed due to multidb support, 070425
--EXEC	[dbo].[p_log_NET_update_lastupdated] @main_id, @node_id

GO

