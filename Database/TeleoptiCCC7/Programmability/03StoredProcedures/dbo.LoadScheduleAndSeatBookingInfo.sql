IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadScheduleAndSeatBookingInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE  dbo.[LoadScheduleAndSeatBookingInfo]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[LoadScheduleAndSeatBookingInfo] 
	-- teamIdList should be comma separated uids for team.	
	@startDate smalldatetime, 
	@endDate smalldatetime, 
	@teamIdList varchar(max) = null,
	@locationIdList varchar(max) = null,
	@businessUnitId uniqueidentifier
AS
BEGIN

SET NOCOUNT ON;

	DECLARE @teamids table
	(
		Id uniqueidentifier
	)

	INSERT INTO @teamids
	SELECT * FROM dbo.SplitStringString(@teamIdList)

	DECLARE @locationids table
	(
		Id uniqueidentifier
	)

	INSERT INTO @locationids
	SELECT * FROM dbo.SplitStringString(@locationIdList)

SELECT SeatBooking.StartDateTime as SeatBookingStart
      ,SeatBooking.EndDateTime as SeatBookingEnd
      ,personSchedule.BelongsToDate as BelongsToDateTime
	  ,personSchedule.IsDayOff
	  ,seat.Id as SeatId
      ,seat.Name as SeatName
      ,person.Id As PersonId
	  ,person.FirstName
	  ,person.LastName
	  ,loc.Id As LocationId
	  ,loc.Name As LocationName
	  ,team.Id as TeamId
	  ,team.Name as TeamName
	  ,sit.Id as SiteId
	  ,sit.Name as SiteName
	  ,personSchedule.Model as PersonScheduleModelSerialized
	  ,personSchedule.Start as PersonScheduleStart
	  ,[personSchedule].[End] as PersonScheduleEnd 
	  ,NumberOfRecords = Count(*) OVER()
	  
FROM ReadModel.PersonScheduleDay as personSchedule
 
  JOIN Person as person on personSchedule.PersonId = Person.Id
  JOIN Team as team on personSchedule.TeamId = team.Id
  LEFT JOIN SeatBooking as SeatBooking on ( personSchedule.PersonId = SeatBooking.Person 
		and personSchedule.BelongsToDate = SeatBooking.BelongsToDate )
  LEFT JOIN Seat as seat on SeatBooking.Seat = seat.Id
  LEFT JOIN SeatMapLocation as loc on loc.Id = seat.Parent
  LEFT JOIN [Site] as sit on sit.id = personSchedule.SiteId 
    
WHERE personSchedule.BusinessUnitId = @businessUnitId and
	  personSchedule.Start IS NOT NULL and 
	( personSchedule.BelongsToDate between @startDate and @endDate )
	
	AND (EXISTS (select Id from @teamids where personSchedule.TeamId = Id) or @teamIdList IS NULL)
	AND (EXISTS (select Id from @locationids where loc.Id = Id) or @locationIdList IS NULL)
  
  ORDER BY personSchedule.BelongsToDate ASC, SiteName, TeamName, LastName, FirstName
 
END



