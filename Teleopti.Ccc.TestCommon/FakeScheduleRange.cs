using System;
using System.Collections.Generic;
using NHibernate.Mapping;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeScheduleRange : Schedule, IScheduleRange
	{
		public FakeScheduleRange(IScheduleDictionary owner, IScheduleParameters parameters) : base(owner, parameters)
		{
		}

		public IScheduleRange UpdateCalcValues(int scheduledDaysOff, TimeSpan contractTimeHolder)
		{
			CalculatedScheduleDaysOff = scheduledDaysOff;
			CalculatedContractTimeHolder = contractTimeHolder;
			return this;
		}

		public new DateTimePeriod Period { get; private set; }
		public new IPerson Person { get; private set; }
		public new IScenario Scenario { get; private set; }
		protected override bool CheckPermission(IScheduleData persistableScheduleData)
		{
			throw new NotImplementedException();
		}

		public new object Clone()
		{
			throw new NotImplementedException();
		}

		public new IScheduleDictionary Owner { get; private set; }
		public new bool Contains(IScheduleData scheduleData)
		{
			throw new NotImplementedException();
		}

		public new bool WithinRange(DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public new DateTimePeriod? TotalPeriod()
		{
			throw new NotImplementedException();
		}

		public new IList<IBusinessRuleResponse> BusinessRuleResponseInternalCollection { get; private set; }
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

		public TimeSpan CalculatedContractTimeHolder { get; set; }
		public TimeSpan CalculatedContractTimeHolderOnPeriod(DateOnlyPeriod periodToCheck)
		{
			throw new NotImplementedException();
		}

		public TimeSpan? CalculatedTargetTimeHolder(DateOnlyPeriod periodToCheck)
		{
			return TimeSpan.FromHours(8);
		}
		public int? CalculatedTargetScheduleDaysOff(DateOnlyPeriod periodToCheck)
		{
			return 8;
		}

		public int CalculatedScheduleDaysOff { get; private set; }



		public IEnumerable<IScheduleDay> ScheduledDayCollection(DateOnlyPeriod dateOnlyPeriod)
		{
			var fac = new SchedulePartFactoryForDomain();
			return new List<IScheduleDay> { fac.CreatePart() };
		}

		public IEnumerable<IScheduleDay> ScheduledDayCollectionForStudentAvailability(DateOnlyPeriod dateOnlyPeriod)
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

		public void Reassociate(IUnitOfWork unitOfWork)
		{
			throw new NotImplementedException();
		}
	}
}


