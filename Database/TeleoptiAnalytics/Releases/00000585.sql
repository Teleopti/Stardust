  insert into dbo.hangfire_requeue
  values (newid(), null, 'Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonPeriodUpdater', 0, null)

  insert into dbo.hangfire_requeue
  values (newid(), null, 'Teleopti.Ccc.Domain.ApplicationLayer.Request.AnalyticsRequestUpdater', 0, null)

  insert into dbo.hangfire_requeue
  values (newid(), null, 'Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsScheduleMatchingPerson', 0, null)

  insert into dbo.hangfire_requeue
  values (newid(), null, 'Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics.AnalyticsScheduleChangeUpdater', 0, null)

  insert into dbo.hangfire_requeue
  values (newid(), null, 'Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.AnalyticsPersonGroupsHandler', 0, null)

