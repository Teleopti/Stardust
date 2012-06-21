using System.Collections.Generic;
using System.Threading;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPossibleCombinationsOfStartEndCategoryRunner
	{
		void RunTheList(IList<IPossibleStartEndCategory> possibleStartEndCategories, IList<IShiftProjectionCache> shiftProjectionList,
			DateOnly dateOnly, IPerson person, ISchedulingOptions schedulingOptions, bool useShiftCategoryFairness, 
			IShiftCategoryFairnessFactors shiftCategoryFairnessFactors, IFairnessValueResult totalFairness, IFairnessValueResult agentFairness,
			IList<IPerson> persons, IEffectiveRestriction effectiveRestriction);
	}

	public class PossibleCombinationsOfStartEndCategoryRunner : IPossibleCombinationsOfStartEndCategoryRunner
	{
		private readonly IBestGroupValueExtractorThreadFactory _bestGroupValueExtractorThreadFactory;

		public PossibleCombinationsOfStartEndCategoryRunner(IBestGroupValueExtractorThreadFactory bestGroupValueExtractorThreadFactory)
		{
			_bestGroupValueExtractorThreadFactory = bestGroupValueExtractorThreadFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void RunTheList(IList<IPossibleStartEndCategory> possibleStartEndCategories, IList<IShiftProjectionCache> shiftProjectionList,
			DateOnly dateOnly, IPerson person, ISchedulingOptions schedulingOptions, bool useShiftCategoryFairness,
			IShiftCategoryFairnessFactors shiftCategoryFairnessFactors, IFairnessValueResult totalFairness, IFairnessValueResult agentFairness, 
			IList<IPerson> persons, IEffectiveRestriction effectiveRestriction)
		{
			var arrayLimit = possibleStartEndCategories.Count;
			var doneEvents = new ManualResetEvent[arrayLimit];
			var periodValueExtractorThreads = new IShiftCategoryPeriodValueExtractorThread[arrayLimit];
			for (var i = 0; i < arrayLimit; i++)
			{
				var d = _bestGroupValueExtractorThreadFactory.GetNewBestGroupValueExtractorThread(shiftProjectionList,
				dateOnly, person, schedulingOptions, useShiftCategoryFairness, shiftCategoryFairnessFactors, totalFairness, agentFairness,
				persons,effectiveRestriction);
				doneEvents[i] = d.ManualResetEvent;
				periodValueExtractorThreads[i] = d;
				ThreadPool.QueueUserWorkItem(d.ExtractShiftCategoryPeriodValue, possibleStartEndCategories[i]);

			}
			WaitHandle.WaitAll(doneEvents);
		}

	}
}