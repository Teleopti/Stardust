using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IBestGroupValueExtractorThreadFactory
	{
		IShiftCategoryPeriodValueExtractorThread GetNewBestGroupValueExtractorThread(IList<IShiftProjectionCache> shiftProjectionList,DateOnly dateOnly,
																					IGroupPerson person,ISchedulingOptions schedulingOptions);
	}

	public class BestGroupValueExtractorThreadFactory : IBestGroupValueExtractorThreadFactory
	{
		private readonly IBlockSchedulingWorkShiftFinderService _workShiftFinderService;
		private readonly ISchedulingResultStateHolder _resultStateHolder;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
		private readonly IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
		private readonly IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

		public BestGroupValueExtractorThreadFactory(IBlockSchedulingWorkShiftFinderService workShiftFinderService,
														ISchedulingResultStateHolder resultStateHolder,
														ISchedulingResultStateHolder schedulingResultStateHolder,
														IPersonSkillPeriodsDataHolderManager personSkillPeriodsDataHolderManager,
														IGroupShiftCategoryFairnessCreator groupShiftCategoryFairnessCreator,
														IShiftProjectionCacheFilter shiftProjectionCacheFilter,
														IEffectiveRestrictionCreator effectiveRestrictionCreator)
		{
			_workShiftFinderService = workShiftFinderService;
			_resultStateHolder = resultStateHolder;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_personSkillPeriodsDataHolderManager = personSkillPeriodsDataHolderManager;
			_groupShiftCategoryFairnessCreator = groupShiftCategoryFairnessCreator;
			_shiftProjectionCacheFilter = shiftProjectionCacheFilter;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		}

		public IShiftCategoryPeriodValueExtractorThread GetNewBestGroupValueExtractorThread(IList<IShiftProjectionCache> shiftProjectionList,DateOnly dateOnly,
														IGroupPerson person, ISchedulingOptions schedulingOptions)
		{
			return new ShiftCategoryPeriodValueExtractorThread(
				shiftProjectionList,
				schedulingOptions,
				_workShiftFinderService,
				dateOnly,
				person,
				_schedulingResultStateHolder,
				_resultStateHolder,
				_personSkillPeriodsDataHolderManager,
				_groupShiftCategoryFairnessCreator,
				_shiftProjectionCacheFilter,
				_effectiveRestrictionCreator); 
			
		}
	}
}