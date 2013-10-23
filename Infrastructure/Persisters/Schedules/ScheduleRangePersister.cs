﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangePersister : IScheduleRangePersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRangeConflictCollector _scheduleRangeConflictCollector;
		private readonly IScheduleRangeSaver _scheduleRangeSaver;
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;

		public ScheduleRangePersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
		                              IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService,
		                              IScheduleRangeConflictCollector scheduleRangeConflictCollector,
																	IScheduleRangeSaver scheduleRangeSaver,
																	IMessageBrokerIdentifier messageBrokerIdentifier)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_differenceCollectionService = differenceCollectionService;
			_scheduleRangeConflictCollector = scheduleRangeConflictCollector;
			_scheduleRangeSaver = scheduleRangeSaver;
			_messageBrokerIdentifier = messageBrokerIdentifier;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleRange scheduleRange)
		{
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService);
			if (diff.IsNullOrEmpty())
			{
				return Enumerable.Empty<PersistConflict>();
			}
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork(TransactionIsolationLevel.Serializable))
			{
				var period = scheduleRange.Period.ToDateOnlyPeriod(scheduleRange.Person.PermissionInformation.DefaultTimeZone());
				var conflicts = _scheduleRangeConflictCollector.GetConflicts(diff, scheduleRange.Scenario, period);
				if (conflicts.IsNullOrEmpty())
				{
					_scheduleRangeSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) scheduleRange);
				}
				uow.PersistAll(_messageBrokerIdentifier);
				return conflicts;
			}
		}
	}
} 