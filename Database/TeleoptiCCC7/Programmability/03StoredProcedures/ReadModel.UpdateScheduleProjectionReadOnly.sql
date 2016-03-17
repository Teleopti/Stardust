IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateScheduleProjectionReadOnly]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdateScheduleProjectionReadOnly]
GO

CREATE PROCEDURE [ReadModel].[UpdateScheduleProjectionReadOnly]
@PersonId uniqueidentifier,
@ScenarioId uniqueidentifier,
@BelongsToDate datetime,
@PayloadId uniqueidentifier,
@StartDateTime datetime,
@EndDateTime datetime,
@WorkTime bigint,
@ContractTime bigint,
@Name nvarchar(50),
@ShortName nvarchar(25),
@DisplayColor int,
@PayrollCode nvarchar(20),
@InsertedOn datetime,
@ScheduleLoadedTime datetime
WITH EXECUTE AS OWNER

-- =============================================
-- Author@		Chundan Xu
-- Create date@ 2016-03-17
-- Description@	Update the projection read models for person schedule day
-- =============================================

AS
SET NOCOUNT ON

DECLARE @existingScheduleLoadedTime datetime
DECLARE @updateCount int

SET @updateCount = 0

SELECT @existingScheduleLoadedTime = ScheduleLoadedTime
FROM ReadModel.PersonScheduleProjectionLoadTime
WHERE PersonId = @PersonId
AND BelongsToDate = @BelongsToDate

IF (@existingScheduleLoadedTime IS NULL)
BEGIN
	INSERT INTO ReadModel.PersonScheduleProjectionLoadTime (PersonId,BelongsToDate,ScheduleLoadedTime) 
	VALUES (@PersonId,@BelongsToDate,@ScheduleLoadedTime)
	SET @updateCount = 1 -- number of records changed
END

IF (@existingScheduleLoadedTime <= @ScheduleLoadedTime)
BEGIN
	UPDATE ReadModel.PersonScheduleProjectionLoadTime
	SET ScheduleLoadedTime = @ScheduleLoadedTime
	WHERE PersonId = @PersonId
	AND BelongsToDate = @BelongsToDate

	SET @updateCount = 1 -- number of records changed
END

IF (@updateCount = 1)
BEGIN
	DELETE FROM ReadModel.ScheduleProjectionReadOnly 
	WHERE BelongsToDate = @BelongsToDate 
	AND ScenarioId = @ScenarioId 
	AND PersonId = @PersonId

	INSERT¡¡INTO ReadModel.ScheduleProjectionReadOnly (ScenarioId,PersonId,BelongsToDate,PayloadId,StartDateTime,EndDateTime,WorkTime,ContractTime,Name,ShortName,DisplayColor,PayrollCode,InsertedOn)
	VALUES (@ScenarioId,@PersonId,@BelongsToDate,@PayloadId,@StartDateTime,@EndDateTime,@WorkTime,@ContractTime,@Name,@ShortName,@DisplayColor,@PayrollCode,@InsertedOn)
END

SELECT @updateCount
GO