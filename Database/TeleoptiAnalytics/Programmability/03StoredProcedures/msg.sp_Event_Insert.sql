IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Event_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Event_Insert]
GO



----------------------------------------------------------------------------
-- Insert a single record into Event
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Event_Insert]
	@EventId uniqueidentifier OUTPUT,
	@StartDate datetime,
	@EndDate datetime,
	@UserId int,
	@ProcessId int,
	@ModuleId uniqueidentifier,
	@PackageSize int,
	@IsHeartbeat bit,
	@ReferenceObjectId uniqueidentifier,
	@ReferenceObjectType nvarchar(255),
	@DomainObjectId uniqueidentifier,
	@DomainObjectType nvarchar(255),
	@DomainUpdateType int,
	@DomainObject varbinary(1024) = NULL,
	@ChangedBy nvarchar(10),
	@ChangedDateTime datetime
AS
BEGIN
IF(@EventId = '00000000-0000-0000-0000-000000000000')
BEGIN
	SET @EventId = newid()
	INSERT Msg.Event(EventId, StartDate, EndDate, UserId, ProcessId, ModuleId, PackageSize, IsHeartbeat, ReferenceObjectId, ReferenceObjectType, DomainObjectId, DomainObjectType, DomainUpdateType, DomainObject, ChangedBy, ChangedDateTime)
	VALUES (@EventId, @StartDate, @EndDate, @UserId, @ProcessId, @ModuleId, @PackageSize, @IsHeartbeat, @ReferenceObjectId, @ReferenceObjectType, @DomainObjectId, @DomainObjectType, @DomainUpdateType, @DomainObject, @ChangedBy, @ChangedDateTime)
END
ELSE
   
	IF(EXISTS(SELECT 1 FROM Msg.[Event] WHERE EventId = @EventId))
		BEGIN
			UPDATE	Msg.Event
			SET	StartDate = @StartDate,
				EndDate = @EndDate,
				UserId = @UserId,
				ProcessId = @ProcessId, 
				ModuleId = @ModuleId,
				PackageSize = @PackageSize,
				IsHeartbeat = @IsHeartbeat, 
				ReferenceObjectId = @ReferenceObjectId,
				ReferenceObjectType = @ReferenceObjectType,
				DomainObjectId = @DomainObjectId,
				DomainObjectType = @DomainObjectType,
				DomainUpdateType = @DomainUpdateType, 
				DomainObject = @DomainObject,
				ChangedBy = @ChangedBy,
				ChangedDateTime = @ChangedDateTime
			WHERE 	EventId = @EventId				
		END
	ELSE
		BEGIN
				INSERT Msg.Event(EventId, StartDate, EndDate, UserId, ProcessId, ModuleId, PackageSize, IsHeartbeat, ReferenceObjectId, ReferenceObjectType, DomainObjectId, DomainObjectType, DomainUpdateType, DomainObject, ChangedBy, ChangedDateTime)
				VALUES (@EventId, @StartDate, @EndDate, @UserId, @ProcessId, @ModuleId, @PackageSize, @IsHeartbeat, @ReferenceObjectId, @ReferenceObjectType, @DomainObjectId, @DomainObjectType, @DomainUpdateType, @DomainObject, @ChangedBy, @ChangedDateTime)
		END
END




GO

