IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_addorupdate_actualagentstate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_addorupdate_actualagentstate]
GO


-- =============================================
-- Author:            Ola H
-- Create date: 2012-11-01
-- Description:       Insert or update actual agent state for RTA
-- =============================================
CREATE  PROCEDURE [RTA].[rta_addorupdate_actualagentstate]
@PersonId uniqueidentifier,
@StateCode nvarchar(500),
@PlatformTypeId uniqueidentifier,
@State nvarchar(500),
@StateId uniqueidentifier,
@Scheduled nvarchar(500),
@ScheduledId uniqueidentifier,
@StateStart datetime,
@ScheduledNext nvarchar(500),
@ScheduledNextId uniqueidentifier,
@NextStart datetime,
@AlarmName nvarchar(500),
@AlarmId uniqueidentifier,
@Color int,
@AlarmStart datetime,
@StaffingEffect float,
@ReceivedTime datetime,
@BatchId datetime,
@OriginalDataSourceId nvarchar(50),
@BusinessUnitId uniqueidentifier
AS
BEGIN
           SET NOCOUNT ON;

           UPDATE [RTA].[ActualAgentState]
           SET StateCode = @StateCode,
                      PlatformTypeId = @PlatformTypeId,
                      [State] = @State,
                      StateId = @StateId,
                      Scheduled = @Scheduled,
                      ScheduledId = @ScheduledId, 
                      StateStart = @StateStart, 
                      ScheduledNext = @ScheduledNext,
                      ScheduledNextId = @ScheduledNextId, 
                      NextStart = @NextStart, 
                      AlarmName = @AlarmName,
                      AlarmId = @AlarmId,
                      Color = @Color,
                      AlarmStart = @AlarmStart, 
                      StaffingEffect = @StaffingEffect,
                      ReceivedTime = @ReceivedTime,
                      BatchId = @BatchId,
                      OriginalDataSourceId = @OriginalDataSourceId,
					  BusinessUnitId = @BusinessUnitId
                      WHERE PersonId = @PersonId
           
           If @@ROWCOUNT = 0     
           insert into [RTA].[ActualAgentState]
		   (PersonId, StateCode, PlatformTypeId, State, StateId, Scheduled, ScheduledId, StateStart,
			ScheduledNext, ScheduledNextId, NextStart, AlarmName, AlarmId, Color, AlarmStart, StaffingEffect, 
			ReceivedTime, BatchId, OriginalDataSourceId, BusinessUnitId)
           values(@PersonId, @StateCode, @PlatformTypeId, @State, @StateId, @Scheduled, @ScheduledId, @StateStart,
			@ScheduledNext, @ScheduledNextId, @NextStart, @AlarmName, @AlarmId, @Color, @AlarmStart, @StaffingEffect, 
			@ReceivedTime, @BatchId, @OriginalDataSourceId, @BusinessUnitId)
END

GO
