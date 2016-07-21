IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdatePersonScheduleDay]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdatePersonScheduleDay]
GO

CREATE PROCEDURE [ReadModel].[UpdatePersonScheduleDay]
@PersonId uniqueidentifier,
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
	IF NOT EXISTS (
		SELECT * FROM ReadModel.PersonScheduleDay 
		WHERE PersonId = @PersonId
			AND BelongsToDate = @BelongsToDate)
	BEGIN
		INSERT INTO ReadModel.PersonScheduleDay (PersonId, Start,[End],BelongsToDate,IsDayOff,Model,ScheduleLoadedTime) 
		VALUES (@PersonId, @Start,@End,@BelongsToDate,@IsDayOff,@Model,@ScheduleLoadedTime)
	END
	SELECT 1 -- number of records changed
	RETURN
END

DECLARE @existingScheduleLoadedTime datetime

SELECT @existingScheduleLoadedTime = ScheduleLoadedTime
FROM ReadModel.PersonScheduleDay
WHERE PersonId = @PersonId
AND BelongsToDate = @BelongsToDate

IF (@existingScheduleLoadedTime IS NULL)
BEGIN
	INSERT INTO ReadModel.PersonScheduleDay (PersonId,Start,[End],BelongsToDate,IsDayOff,Model,ScheduleLoadedTime) 
	VALUES (@PersonId,@Start,@End,@BelongsToDate,@IsDayOff,@Model,@ScheduleLoadedTime)
	SELECT 1 -- number of records changed
	RETURN
END

IF (@existingScheduleLoadedTime <= @ScheduleLoadedTime)
BEGIN
	DELETE FROM ReadModel.PersonScheduleDay
	WHERE PersonId = @PersonId
	AND BelongsToDate = @BelongsToDate

	INSERT INTO ReadModel.PersonScheduleDay (PersonId,Start,[End],BelongsToDate,IsDayOff,Model,ScheduleLoadedTime) 
	VALUES (@PersonId,@Start,@End,@BelongsToDate,@IsDayOff,@Model,@ScheduleLoadedTime)
	SELECT 1 -- number of records changed
END

SELECT 0 -- number of records changed
GO