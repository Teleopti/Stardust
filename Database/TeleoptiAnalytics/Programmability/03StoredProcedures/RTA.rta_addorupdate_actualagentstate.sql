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
@StateCode nvarchar(500) = NULL,
@PlatformTypeId uniqueidentifier,
@State nvarchar(500) = NULL,
@StateId uniqueidentifier = NULL,
@Scheduled nvarchar(500) = NULL,
@ScheduledId uniqueidentifier = NULL,
@StateStartTime datetime = NULL,
@ScheduledNext nvarchar(500) = NULL,
@ScheduledNextId uniqueidentifier = NULL,
@NextStart datetime = NULL,
@AlarmName nvarchar(500) = NULL,
@AlarmId uniqueidentifier = NULL,
@Color int = NULL,
@AdherenceStartTime datetime = NULL,
@AlarmStartTime datetime = NULL,
@StaffingEffect float = NULL,
@Adherence int = NULL,
@ReceivedTime datetime = NULL,
@BatchId datetime = NULL,
@OriginalDataSourceId nvarchar(50) = NULL,
@BusinessUnitId uniqueidentifier,
@SiteId uniqueidentifier,
@TeamId uniqueidentifier
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
                      StateStartTime = @StateStartTime, 
                      ScheduledNext = @ScheduledNext,
                      ScheduledNextId = @ScheduledNextId, 
                      NextStart = @NextStart, 
                      AlarmName = @AlarmName,
                      AlarmId = @AlarmId,
                      Color = @Color,
                      AdherenceStartTime = @AdherenceStartTime,
					  AlarmStartTime = @AlarmStartTime,
                      StaffingEffect = @StaffingEffect,
					  Adherence = @Adherence,
                      ReceivedTime = @ReceivedTime,
                      BatchId = @BatchId,
                      OriginalDataSourceId = @OriginalDataSourceId,
					  BusinessUnitId = @BusinessUnitId,
					  SiteId = @SiteId,
					  TeamId = @TeamId
                      WHERE PersonId = @PersonId
           
           If @@ROWCOUNT = 0     
           insert into [RTA].[ActualAgentState]
		   (PersonId, StateCode, PlatformTypeId, State, StateId, Scheduled, ScheduledId, StateStartTime,
			ScheduledNext, ScheduledNextId, NextStart, AlarmName, AlarmId, Color, AdherenceStartTime, AlarmStartTime,StaffingEffect,
			Adherence, ReceivedTime, BatchId, OriginalDataSourceId, BusinessUnitId,TeamId,SiteId)
           values(@PersonId, @StateCode, @PlatformTypeId, @State, @StateId, @Scheduled, @ScheduledId, @StateStartTime,
			@ScheduledNext, @ScheduledNextId, @NextStart, @AlarmName, @AlarmId, @Color, @AdherenceStartTime, @AlarmStartTime,@StaffingEffect,
			@Adherence, @ReceivedTime, @BatchId, @OriginalDataSourceId, @BusinessUnitId,@TeamId,@SiteId)
END

GO
