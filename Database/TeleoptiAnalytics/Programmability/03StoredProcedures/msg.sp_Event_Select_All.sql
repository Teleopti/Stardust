IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Event_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Event_Select_All]
GO


CREATE PROC [msg].[sp_Event_Select_All]
AS
SELECT	EventId, 
		StartDate,
		EndDate,
		UserId,
		ProcessId,
		ModuleId,
		PackageSize,
		IsHeartbeat,
		ReferenceObjectId,
		ReferenceObjectType,
		DomainObjectId,
		DomainObjectType,
		DomainUpdateType,
		DomainObject,
		ChangedBy,
		ChangedDateTime
FROM	Msg.Event


GO

