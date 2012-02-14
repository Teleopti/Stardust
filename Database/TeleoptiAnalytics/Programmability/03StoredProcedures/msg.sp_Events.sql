IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Events]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Events]
GO




CREATE PROCEDURE [msg].[sp_Events]
AS
SELECT EventId, StartDate, EndDate, UserId, ProcessId, ModuleId, PackageSize, IsHeartbeat, ReferenceObjectId, ReferenceObjectType, DomainObjectId, DomainObjectType, DomainUpdateType, DomainObject, ChangedBy, ChangedDateTime 
FROM Msg.EVENT 
ORDER BY ChangedDateTime Desc

GO

