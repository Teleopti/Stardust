IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdatePersonScheduleDay]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdatePersonScheduleDay]
GO

CREATE PROCEDURE [ReadModel].[UpdatePersonScheduleDay]
@PersonId uniqueidentifier,
@TeamId uniqueidentifier,
@SiteId uniqueidentifier,
@BusinessUnitId uniqueidentifier,
@Start datetime,
@End datetime,
@BelongsToDate datetime,
@IsDayOff bit,
@Model nvarchar(max),
@ScheduleLoadedTime datetime,
@IsInitialLoad bit
WITH EXECUTE AS OWNER

-- =============================================
-- Author@		Yanyi, Jonas, Chundan
-- Create date@ 2016-03-10
-- Description@	Updates the read model for person schedule day
-- =============================================

AS
SET NOCOUNT ON
IF(@IsInitialLoad=1)
BEGIN
	INSERT INTO ReadModel.PersonScheduleDay (PersonId,TeamId,SiteId,BusinessUnitId,Start,[End],BelongsToDate,IsDayOff,Model,ScheduleLoadedTime) 
	VALUES (@PersonId,@TeamId,@SiteId,@BusinessUnitId,@Start,@End,@BelongsToDate,@IsDayOff,@Model,@ScheduleLoadedTime)
	RETURN
END

DECLARE @existingScheduleLoadedTime datetime

SELECT @existingScheduleLoadedTime = ScheduleLoadedTime
FROM ReadModel.PersonScheduleDay
WHERE PersonId = @PersonId
AND BelongsToDate = @BelongsToDate

IF (@existingScheduleLoadedTime IS NULL)
BEGIN
	INSERT INTO ReadModel.PersonScheduleDay (PersonId,TeamId,SiteId,BusinessUnitId,Start,[End],BelongsToDate,IsDayOff,Model,ScheduleLoadedTime) 
	VALUES (@PersonId,@TeamId,@SiteId,@BusinessUnitId,@Start,@End,@BelongsToDate,@IsDayOff,@Model,@ScheduleLoadedTime)
	RETURN
END

IF (@existingScheduleLoadedTime < @ScheduleLoadedTime)
BEGIN
	DELETE FROM ReadModel.PersonScheduleDay
	WHERE PersonId = @PersonId
	AND BelongsToDate = @BelongsToDate

	INSERT INTO ReadModel.PersonScheduleDay (PersonId,TeamId,SiteId,BusinessUnitId,Start,[End],BelongsToDate,IsDayOff,Model,ScheduleLoadedTime) 
	VALUES (@PersonId,@TeamId,@SiteId,@BusinessUnitId,@Start,@End,@BelongsToDate,@IsDayOff,@Model,@ScheduleLoadedTime)
END
GO