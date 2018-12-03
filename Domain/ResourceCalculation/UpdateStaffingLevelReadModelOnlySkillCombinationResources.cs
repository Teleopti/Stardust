using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IUpdateStaffingLevelReadModel
	{
		void Update(DateTimePeriod period);
	}
	public class UpdateStaffingLevelReadModelOnlySkillCombinationResources : IUpdateStaffingLevelReadModel
	{
		private readonly INow _now;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateStaffingLevelReadModelOnlySkillCombinationResources));

		public UpdateStaffingLevelReadModelOnlySkillCombinationResources(INow now, 
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory, 
			LoaderForResourceCalculation loaderForResourceCalculation, IResourceCalculation resourceCalculation, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository, IStardustJobFeedback stardustJobFeedback)
		{
			_now = now;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceCalculation = resourceCalculation;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_stardustJobFeedback = stardustJobFeedback;
		}

		public void Update(DateTimePeriod period)
		{
			try
			{
				var periodDateOnly = new DateOnlyPeriod(new DateOnly(period.StartDateTime), new DateOnly(period.EndDateTime));
				_stardustJobFeedback.SendProgress($"Start running resource calculation for period {periodDateOnly}");
				var timeWhenResourceCalcDataLoaded = _now.UtcDateTime();
				_loaderForResourceCalculation.PreFillInformation(periodDateOnly);
				var resCalcData = _loaderForResourceCalculation.ResourceCalculationData(periodDateOnly, false);
				_stardustJobFeedback.SendProgress($"Preloaded data for {resCalcData.Skills.Count()} skills.");
				using (_resourceCalculationContextFactory.Create(resCalcData.Schedules, resCalcData.Skills,
					Enumerable.Empty<ExternalStaff>(), true, periodDateOnly))
				{
					_resourceCalculation.ResourceCalculate(periodDateOnly, resCalcData);
				}
				_stardustJobFeedback.SendProgress(
					$"Filtering combinations resources between {period.StartDateTime} and {period.EndDateTime}");
				var filteredCombinations =
					resCalcData.SkillCombinationHolder?.SkillCombinationResources.Where(
						x =>
							x.StartDateTime >= period.StartDateTime &&
							x.StartDateTime < period.EndDateTime);
				_skillCombinationResourceRepository.PersistSkillCombinationResource(timeWhenResourceCalcDataLoaded,
					filteredCombinations);
			}
			catch (NoDefaultScenarioException ex)
			{
				Logger.Warn(ex.Message);
				_stardustJobFeedback.SendProgress(ex.Message);
			}
		}
	}

	public class FakeUpdateStaffingLevelReadModel : IUpdateStaffingLevelReadModel
	{
		public Exception Exception { get; set; }

		public void Update(DateTimePeriod period)
		{
			if(Exception != null) throw Exception;
		}
	}
}
