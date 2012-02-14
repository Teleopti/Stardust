/****** Object:  StoredProcedure [msg].[sp_Heartbeat_Select_All]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Select_All]
GO

----------------------------------------------------------------------------
-- Select a single record from Heartbeat
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Heartbeat_Select_All]
AS

SELECT	HeartbeatId,
	SubscriberId,
	ProcessId,
	ChangedBy,
	ChangedDateTime
FROM	Msg.Heartbeat
GO
