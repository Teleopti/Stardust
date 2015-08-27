﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleRangePersister : IScheduleRangePersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleRangeConflictCollector _scheduleRangeConflictCollector;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IInitiatorIdentifier _initiatorIdentifier;

		public ScheduleRangePersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
									  IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService,
		                              IScheduleRangeConflictCollector scheduleRangeConflictCollector,
																	IScheduleDifferenceSaver scheduleDifferenceSaver,
																	IInitiatorIdentifier initiatorIdentifier)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_differenceCollectionService = differenceCollectionService;
			_scheduleRangeConflictCollector = scheduleRangeConflictCollector;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleRange scheduleRange)
		{
			var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService);
			if (diff.IsNullOrEmpty())
			{
				return Enumerable.Empty<PersistConflict>();
			}
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var conflicts = _scheduleRangeConflictCollector.GetConflicts(diff, scheduleRange);
				if (conflicts.IsNullOrEmpty())
				{
					var modified = _scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) scheduleRange);
				}
				uow.PersistAll(_initiatorIdentifier);
				return conflicts;
			}
		}
	}
} 