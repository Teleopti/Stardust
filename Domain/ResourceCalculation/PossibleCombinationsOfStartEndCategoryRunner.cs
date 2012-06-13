using System.Collections.Generic;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPossibleCombinationsOfStartEndCategoryRunner
	{
		void RunTheList(IList<PossibleStartEndCategory> possibleStartEndCategories, IList<IShiftProjectionCache> shiftProjectionList,
										DateOnly dateOnly, IGroupPerson person, ISchedulingOptions schedulingOptions);
	}

	public class PossibleCombinationsOfStartEndCategoryRunner : IPossibleCombinationsOfStartEndCategoryRunner
	{
		private readonly IBestGroupValueExtractorThreadFactory _bestGroupValueExtractorThreadFactory;

		public PossibleCombinationsOfStartEndCategoryRunner(IBestGroupValueExtractorThreadFactory bestGroupValueExtractorThreadFactory)
		{
			_bestGroupValueExtractorThreadFactory = bestGroupValueExtractorThreadFactory;
		}

		public void RunTheList(IList<PossibleStartEndCategory> possibleStartEndCategories, IList<IShiftProjectionCache> shiftProjectionList,
			DateOnly dateOnly, IGroupPerson person, ISchedulingOptions schedulingOptions)
		{
			var arrayLimit = possibleStartEndCategories.Count;
			var doneEvents = new ManualResetEvent[arrayLimit];
			var dummyArray = new ShiftCategoryPeriodValueExtractorThread[arrayLimit];
			for (var i = 0; i < arrayLimit; i++)
			{
				doneEvents[i] = new ManualResetEvent(false);
				var d = _bestGroupValueExtractorThreadFactory.GetNewBestGroupValueExtractorThread(shiftProjectionList,
				dateOnly, person, doneEvents[i], schedulingOptions);
				dummyArray[i] = d;
				ThreadPool.QueueUserWorkItem(d.ExtractShiftCategoryPeriodValue, possibleStartEndCategories[i]);

			}
			WaitHandle.WaitAll(doneEvents);
		}

	}
}