IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Heartbeat_Delete_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Heartbeat_Delete_All]
GO




----------------------------------------------------------------------------
-- Delete a single record from Heartbeat
----------------------------------------------------------------------------
CREATE PROC [Msg].[sp_Heartbeat_Delete_All]
AS

DELETE FROM Msg.Heartbeat



GO

