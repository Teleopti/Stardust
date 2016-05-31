IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[DeleteScheduleProjectionReadOnly]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [ReadModel].[DeleteScheduleProjectionReadOnly]
GO

CREATE PROCEDURE [ReadModel].[DeleteScheduleProjectionReadOnly]
	@PersonId uniqueidentifier,
	@ScenarioId uniqueidentifier,
	@BelongsToDate datetime,
	@version int
WITH EXECUTE AS OWNER
AS
SET NOCOUNT ON

DECLARE @haveRecord uniqueidentifier
DECLARE @haveVersion int
DECLARE @do bit

SET @do = 0

SELECT @haveVersion = [Version], @haveRecord = [PersonId]
FROM ReadModel.PersonScheduleProjectionLoadTime WITH (UPDLOCK)
WHERE 
	PersonId = @PersonId AND 
	BelongsToDate = @BelongsToDate

IF (@haveRecord IS NULL)
BEGIN
	INSERT INTO ReadModel.PersonScheduleProjectionLoadTime (
		PersonId,
		BelongsToDate,
		[Version]
	) 
	VALUES (@PersonId, @BelongsToDate, @version)
	SET @do = 1
END

IF (@haveRecord IS NOT NULL AND (@haveVersion IS NULL OR @haveVersion < @version))
BEGIN
	UPDATE ReadModel.PersonScheduleProjectionLoadTime
	SET [Version] = @version
	WHERE 
		PersonId = @PersonId AND
		BelongsToDate = @BelongsToDate
	SET @do = 1
END

IF (@do = 1)
BEGIN
	DELETE FROM ReadModel.ScheduleProjectionReadOnly 
	WHERE 
		BelongsToDate = @BelongsToDate AND 
		ScenarioId = @ScenarioId AND 
		PersonId = @PersonId
END

SELECT @do

GO