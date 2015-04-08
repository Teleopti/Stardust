﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class MatrixListFactory : IMatrixListFactory
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
		private readonly IMatrixUserLockLocker _matrixUserLockLocker;
		private readonly IMatrixNotPermittedLocker _matrixNotPermittedLocker;

		public MatrixListFactory(Func<ISchedulerStateHolder> schedulerStateHolder, IScheduleMatrixListCreator scheduleMatrixListCreator, IMatrixUserLockLocker matrixUserLockLocker, IMatrixNotPermittedLocker matrixNotPermittedLocker)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleMatrixListCreator = scheduleMatrixListCreator;
			_matrixUserLockLocker = matrixUserLockLocker;
			_matrixNotPermittedLocker = matrixNotPermittedLocker;
		}

		public IList<IScheduleMatrixPro> CreateMatrixListAll(DateOnlyPeriod selectedPeriod)
		{
			var allSchedules = new List<IScheduleDay>();
			var stateHolder = _schedulerStateHolder();
			var period = stateHolder.RequestedPeriod.DateOnlyPeriod;
			period = new DateOnlyPeriod(period.StartDate.AddDays(-10), period.EndDate.AddDays(10));
			var persons = stateHolder.FilteredPersonDictionary;

			foreach (var person in persons)
			{
				var theDays = stateHolder.Schedules[person.Value].ScheduledDayCollection(period);
				allSchedules.AddRange(theDays);
			}

			return CreateMatrixList(allSchedules, selectedPeriod);
		}

		public IList<IScheduleMatrixPro> CreateMatrixList(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod)
		{
			if (scheduleDays == null)
				throw new ArgumentNullException("scheduleDays");

			IList<IScheduleMatrixPro> matrixes = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(scheduleDays);

			_matrixUserLockLocker.Execute(matrixes, selectedPeriod);
			_matrixNotPermittedLocker.Execute(matrixes);

			return matrixes;
		}
	}
}