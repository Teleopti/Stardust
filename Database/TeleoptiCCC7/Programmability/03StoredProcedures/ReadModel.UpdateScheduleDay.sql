IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateScheduleDay]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdateScheduleDay]
GO

CREATE PROCEDURE [ReadModel].[UpdateScheduleDay]
@PersonId uniqueidentifier,
@StartDateTime datetime,
@EndDateTime datetime,
@WorkTime bigint,
@ContractTime bigint,
@Workday bit,
@Label nvarchar(50),
@DisplayColor int,
@BelongsToDate datetime,
@NotScheduled bit 
WITH EXECUTE AS OWNER

AS
SET NOCOUNT ON

DELETE FROM ReadModel.ScheduleDay 
WHERE PersonId = @PersonId AND BelongsToDate = @BelongsToDate

INSERT INTO ReadModel.ScheduleDay 
	(PersonId,BelongsToDate,StartDateTime,EndDateTime,Workday,WorkTime,ContractTime,Label,DisplayColor,NotScheduled) 
VALUES 
	(@PersonId,@BelongsToDate,@StartDateTime,@EndDateTime,@Workday,@WorkTime,@ContractTime,@Label,@DisplayColor,@NotScheduled)

RETURN

GO