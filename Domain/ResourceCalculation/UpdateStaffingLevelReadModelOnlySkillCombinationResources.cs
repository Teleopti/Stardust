using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class UpdateStaffingLevelReadModelOnlySkillCombinationResources : IUpdateStaffingLevelReadModel
	{
		private readonly INow _now;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public UpdateStaffingLevelReadModelOnlySkillCombinationResources(INow now, 
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory, 
			LoaderForResourceCalculation loaderForResourceCalculation, IResourceCalculation resourceCalculation, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_now = now;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceCalculation = resourceCalculation;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}

		public void Update(DateTimePeriod period)
		{
			var periodDateOnly = new DateOnlyPeriod(new DateOnly(period.StartDateTime), new DateOnly(period.EndDateTime));
			var timeWhenResourceCalcDataLoaded = _now.UtcDateTime();
			_loaderForResourceCalculation.PreFillInformation(periodDateOnly);
			var resCalcData = _loaderForResourceCalculation.ResourceCalculationData(periodDateOnly, false);
			using (_resourceCalculationContextFactory.Create(resCalcData.Schedules, resCalcData.Skills, true, periodDateOnly))
			{
				_resourceCalculation.ResourceCalculate(periodDateOnly, resCalcData);
			}

			var filteredCombinations =
				resCalcData.SkillCombinationHolder?.SkillCombinationResources.Where(
					x =>
						x.StartDateTime >= period.StartDateTime &&
						x.StartDateTime < period.EndDateTime);
			_skillCombinationResourceRepository.PersistSkillCombinationResource(timeWhenResourceCalcDataLoaded,
																				filteredCombinations);
		}
	}
}
