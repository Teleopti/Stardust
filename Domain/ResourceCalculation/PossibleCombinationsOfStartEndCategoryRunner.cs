using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private delegate void ExtractShiftCategoryPeriodValueDelegate(IPossibleStartEndCategory possibleStartEndCategory);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void RunTheList(IList<IPossibleStartEndCategory> possibleStartEndCategories, IList<IShiftProjectionCache> shiftProjectionList,
			DateOnly dateOnly, IPerson person, ISchedulingOptions schedulingOptions, bool useShiftCategoryFairness,
			IShiftCategoryFairnessFactors shiftCategoryFairnessFactors, IFairnessValueResult totalFairness, IFairnessValueResult agentFairness, 
			IList<IPerson> persons, IEffectiveRestriction effectiveRestriction)
		{
			IDictionary<ExtractShiftCategoryPeriodValueDelegate, IAsyncResult> runnableList = new Dictionary<ExtractShiftCategoryPeriodValueDelegate, IAsyncResult>();
			var arrayLimit = possibleStartEndCategories.Count;
			for (var i = 0; i < arrayLimit; i++)
			{
				var d = _bestGroupValueExtractorThreadFactory.GetNewBestGroupValueExtractorThread(shiftProjectionList,
				dateOnly, person, schedulingOptions, useShiftCategoryFairness, shiftCategoryFairnessFactors, totalFairness, agentFairness,
				persons, effectiveRestriction);
				ExtractShiftCategoryPeriodValueDelegate toRun = d.ExtractShiftCategoryPeriodValue;

				//For Sync
				//toRun.Invoke(possibleStartEndCategories[i]);

				//For Async
				IAsyncResult result = toRun.BeginInvoke(possibleStartEndCategories[i], null, null);
				runnableList.Add(toRun, result);
			}

			//Sync all threads
			try
			{
				foreach (KeyValuePair<ExtractShiftCategoryPeriodValueDelegate, IAsyncResult> thread in runnableList)
				{
					thread.Key.EndInvoke(thread.Value);
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine(e.Message);
				throw;
			}
			
			
		}


		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		//public void RunTheList2(IList<IPossibleStartEndCategory> possibleStartEndCategories, IList<IShiftProjectionCache> shiftProjectionList,
		//    DateOnly dateOnly, IPerson person, ISchedulingOptions schedulingOptions, bool useShiftCategoryFairness,
		//    IShiftCategoryFairnessFactors shiftCategoryFairnessFactors, IFairnessValueResult totalFairness, IFairnessValueResult agentFairness, 
		//    IList<IPerson> persons, IEffectiveRestriction effectiveRestriction)
		//{
		//    var arrayLimit = possibleStartEndCategories.Count;
		//    var doneEvents = new ManualResetEvent[arrayLimit];
		//    var periodValueExtractorThreads = new IShiftCategoryPeriodValueExtractorThread[arrayLimit];
		//    for (var i = 0; i < arrayLimit; i++)
		//    {
		//        var d = _bestGroupValueExtractorThreadFactory.GetNewBestGroupValueExtractorThread(shiftProjectionList,
		//        dateOnly, person, schedulingOptions, useShiftCategoryFairness, shiftCategoryFairnessFactors, totalFairness, agentFairness,
		//        persons,effectiveRestriction);
		//        doneEvents[i] = d.ManualResetEvent;
		//        periodValueExtractorThreads[i] = d;
		//        ThreadPool.QueueUserWorkItem(d.ExtractShiftCategoryPeriodValue, possibleStartEndCategories[i]);

		//    }

		//    //WaitHandle.WaitAll(doneEvents);
		//    // complains about STA Thread on above
		//    foreach (var manualResetEvent in doneEvents)
		//    {
		//        manualResetEvent.WaitOne();
		//    }
			
		//}

	}
}