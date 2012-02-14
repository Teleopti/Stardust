IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Msg].[sp_Filter_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Msg].[sp_Filter_Insert]
GO
----------------------------------------------------------------------------
-- Insert a single record into Filter
----------------------------------------------------------------------------
CREATE PROC [msg].[sp_Filter_Insert]
	@FilterId uniqueidentifier OUTPUT,
	@SubscriberId uniqueidentifier,
	@ReferenceObjectId nvarchar(36),
	@ReferenceObjectType nvarchar(255),
	@DomainObjectId nvarchar(36),
	@DomainObjectType nvarchar(255),
	@EventStartDate datetime,
	@EventEndDate datetime,
	@ChangedBy nvarchar(10),
	@ChangedDateTime datetime
AS
BEGIN
	IF(@FilterId = '00000000-0000-0000-0000-000000000000')
		BEGIN
			SET @FilterId = newid()
			INSERT Msg.Filter(FilterId, SubscriberId, ReferenceObjectId, ReferenceObjectType, DomainObjectId, DomainObjectType, EventStartDate, EventEndDate, ChangedBy, ChangedDateTime)
			VALUES (@FilterId, @SubscriberId, @ReferenceObjectId, @ReferenceObjectType, @DomainObjectId, @DomainObjectType, @EventStartDate, @EventEndDate, @ChangedBy, @ChangedDateTime)
		END
	ELSE
		IF(EXISTS(SELECT 1 FROM Msg.Filter WHERE  FilterId = @FilterId))
			BEGIN
				UPDATE	Msg.Filter
				SET	SubscriberId = @SubscriberId,
					ReferenceObjectId = @ReferenceObjectId,
					ReferenceObjectType = @ReferenceObjectType,
					DomainObjectId = @DomainObjectId,
					DomainObjectType = @DomainObjectType,
					EventStartDate = @EventStartDate,
					EventEndDate = @EventEndDate,
					ChangedBy = @ChangedBy,
					ChangedDateTime = @ChangedDateTime
				WHERE 	FilterId = @FilterId
			END
		ELSE
			BEGIN
				INSERT Msg.Filter(FilterId, SubscriberId, ReferenceObjectId, ReferenceObjectType, DomainObjectId, DomainObjectType, EventStartDate, EventEndDate, ChangedBy, ChangedDateTime)
				VALUES (@FilterId, @SubscriberId, @ReferenceObjectId, @ReferenceObjectType, @DomainObjectId, @DomainObjectType, @EventStartDate, @EventEndDate, @ChangedBy, @ChangedDateTime)
			END
END
GO

