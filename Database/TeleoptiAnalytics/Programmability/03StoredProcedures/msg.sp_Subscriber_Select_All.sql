/****** Object:  StoredProcedure [msg].[sp_Subscriber_Select_All]    Script Date: 04/07/2009 10:43:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Subscriber_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Subscriber_Select_All]
GO

---------------------------------------------------------------------------
-- Select a single record from Subscriber
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Subscriber_Select_All]
AS
SELECT	SubscriberId,
	UserId,
	ProcessId,
	IPAddress,
	Port,
	ChangedBy,
	ChangedDateTime
FROM	Msg.Subscriber

GO

