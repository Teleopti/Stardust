﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeScheduleRange: IScheduleRange
	{
		public DateTimePeriod Period { get; private set; }
		public IPerson Person { get; private set; }
		public IScenario Scenario { get; private set; }
		public object Clone()
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary Owner { get; private set; }
		public bool Contains(IScheduleData scheduleData)
		{
			throw new NotImplementedException();
		}

		public bool WithinRange(DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public DateTimePeriod? TotalPeriod()
		{
			throw new NotImplementedException();
		}

		public IList<IBusinessRuleResponse> BusinessRuleResponseInternalCollection { get; private set; }
		public void ExtractAllScheduleData(IScheduleExtractor extractor, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IScheduleDay ScheduledDay(DateOnly day)
		{
			throw new NotImplementedException();
		}

		public bool Contains(IScheduleData scheduleData, bool includeNonPermitted)
		{
			throw new NotImplementedException();
		}

		public IFairnessValueResult FairnessValue()
		{
			throw new NotImplementedException();
		}

		public TimeSpan? CalculatedContractTimeHolder { get; set; }
		public TimeSpan? CalculatedTargetTimeHolder { get; set; }
		public int? CalculatedTargetScheduleDaysOff { get; set; }
		public int? CalculatedScheduleDaysOff { get; set; }
		public IEnumerable<IScheduleDay> ScheduledDayCollection(DateOnlyPeriod dateOnlyPeriod)
		{
			throw new NotImplementedException();
		}

		public IScheduleDay ReFetch(IScheduleDay schedulePart)
		{
			throw new NotImplementedException();
		}

		public IShiftCategoryFairnessHolder CachedShiftCategoryFairness()
		{
			throw new NotImplementedException();
		}

		public IDifferenceCollection<IPersistableScheduleData> DifferenceSinceSnapshot(IDifferenceCollectionService<IPersistableScheduleData> differenceService)
		{
			throw new NotImplementedException();
		}

		public void TakeSnapshot()
		{
			throw new NotImplementedException();
		}

		public DateTimePeriod VisiblePeriodMinusFourWeeksPeriod()
		{
			throw new NotImplementedException();
		}

		public IScheduleDay ScheduledDay(DateOnly day, bool includeUnpublished)
		{
			throw new NotImplementedException();
		}

		public void ForceRecalculationOfContractTimeAndDaysOff()
		{
			throw new NotImplementedException();
		}

		public bool IsEmpty()
		{
			throw new NotImplementedException();
		}
	}
}