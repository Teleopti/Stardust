using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IBestGroupValueExtractorThreadFactory
	{
		IShiftCategoryPeriodValueExtractorThread GetNewBestGroupValueExtractorThread(IList<IShiftProjectionCache> shiftProjectionList,DateOnly dateOnly, IPerson person,
			ISchedulingOptions schedulingOptions, bool useShiftCategoryFairness, IShiftCategoryFairnessFactors shiftCategoryFairnessFactors,
			IFairnessValueResult totalFairness, IFairnessValueResult agentFairness, IList<IPerson> persons, IEffectiveRestriction effectiveRestriction);
	}

	public class BestGroupValueExtractorThreadFactory : IBestGroupValueExtractorThreadFactory
	{
		private readonly IBlockSchedulingWorkShiftFinderService _workShiftFinderService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
		private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
		

		public BestGroupValueExtractorThreadFactory(IBlockSchedulingWorkShiftFinderService workShiftFinderService,
														ISchedulingResultStateHolder schedulingResultStateHolder,
														IPersonSkillPeriodsDataHolderManager personSkillPeriodsDataHolderManager,
														IShiftProjectionCacheFilter shiftProjectionCacheFilter)
		{
			_workShiftFinderService = workShiftFinderService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_personSkillPeriodsDataHolderManager = personSkillPeriodsDataHolderManager;
			_shiftProjectionCacheFilter = shiftProjectionCacheFilter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IShiftCategoryPeriodValueExtractorThread GetNewBestGroupValueExtractorThread(IList<IShiftProjectionCache> shiftProjectionList,DateOnly dateOnly,
			IPerson person, ISchedulingOptions schedulingOptions, bool useShiftCategoryFairness, IShiftCategoryFairnessFactors shiftCategoryFairnessFactors,
			IFairnessValueResult totalFairness, IFairnessValueResult agentFairness, IList<IPerson> persons, IEffectiveRestriction effectiveRestriction)
		{
			return new ShiftCategoryPeriodValueExtractorThread(
				shiftProjectionList,
				schedulingOptions,
				_workShiftFinderService,
				dateOnly,
				person,
				_schedulingResultStateHolder,
				_personSkillPeriodsDataHolderManager,
				_shiftProjectionCacheFilter,
				useShiftCategoryFairness,
				shiftCategoryFairnessFactors,
				totalFairness,
				agentFairness,
				persons,effectiveRestriction); 
			
		}
	}
}