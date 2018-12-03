using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetStaffingUsingResourceCalculation : ILoadSkillDaysByPeriod
	{
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ISkillRepository _skillRepository;
		private readonly IResourceCalculationPrerequisitesLoader _resourceCalculationPrerequisitesLoader;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;
		private readonly IAssembler<ISkillStaffPeriod, SkillDataDto> _skillDataAssembler;
		private readonly IPersonRepository _personRepository;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IActivityRepository _activityRepository;

		public GetStaffingUsingResourceCalculation(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentScenario scenarioRepository, ISkillRepository skillRepository, IActivityRepository activityRepository, IResourceCalculationPrerequisitesLoader resourceCalculationPrerequisitesLoader, IPersonRepository personRepository, IAssembler<ISkillStaffPeriod, SkillDataDto> skillDataAssembler, ISchedulingResultStateHolder schedulingResultStateHolder, ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation, IResourceCalculation resourceOptimizationHelper, IDateTimePeriodAssembler dateTimePeriodAssembler, CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_skillRepository = skillRepository;
			_activityRepository = activityRepository;
			_personRepository = personRepository;
			_skillDataAssembler = skillDataAssembler;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_loadSchedulingStateHolderForResourceCalculation = loadSchedulingStateHolderForResourceCalculation;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_resourceCalculationPrerequisitesLoader = resourceCalculationPrerequisitesLoader;
		}

		public ICollection<SkillDayDto> GetSkillDayDto(GetSkillDaysByPeriodQueryDto query)
		{
			var dateOnlyPeriod = query.Period.ToDateOnlyPeriod();
			if (dateOnlyPeriod.DayCount() > 31)
				throw new FaultException();
			ICollection<SkillDayDto> returnList = new List<SkillDayDto>();
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var requestedScenario = _scenarioRepository.Current();

				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					_skillRepository.LoadAll();
					_activityRepository.LoadAll();
				}

				var period = dateOnlyPeriod.ToDateTimePeriod(timeZoneInfo);
				var periodForResourceCalc = new DateTimePeriod(period.StartDateTime.AddDays(-1), period.EndDateTime.AddDays(1));
				var dateOnlyPeriodForResourceCalc = periodForResourceCalc.ToDateOnlyPeriod(timeZoneInfo);
				_dateTimePeriodAssembler.TimeZone = timeZoneInfo;

				_resourceCalculationPrerequisitesLoader.Execute();

				var allPeople = _personRepository.FindAllAgentsQuiteLight(dateOnlyPeriodForResourceCalc);

				_loadSchedulingStateHolderForResourceCalculation.Execute(requestedScenario, periodForResourceCalc, allPeople,
					_schedulingResultStateHolder, p => allPeople, true);

				using(_cascadingResourceCalculationContextFactory.Create(_schedulingResultStateHolder, false, dateOnlyPeriodForResourceCalc))
				{
					foreach (var dateTime in dateOnlyPeriodForResourceCalc.DayCollection())
					{
						_resourceOptimizationHelper.ResourceCalculate(dateTime,
							_schedulingResultStateHolder.ToResourceOptimizationData(true, false));
					}					
				}

				var dayCollection = dateOnlyPeriod.DayCollection();
				foreach (ISkill skill in _schedulingResultStateHolder.VisibleSkills)
				{
					foreach (var dateOnly in dayCollection)
					{
						var skillDayDto = new SkillDayDto { DisplayDate = new DateOnlyDto { DateTime = dateOnly.Date } };

						IList<ISkillStaffPeriod> skillStaffPeriods =
							_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(
								new List<ISkill> { skill }, dateOnly.ToDateTimePeriod(timeZoneInfo));
						skillDayDto.Esl = SkillStaffPeriodHelper.EstimatedServiceLevel(skillStaffPeriods).Value;
						skillDayDto.SkillId = skill.Id.GetValueOrDefault();
						skillDayDto.SkillName = skill.Name;

						skillDayDto.SkillDataCollection = _skillDataAssembler.DomainEntitiesToDtos(skillStaffPeriods).ToList();

						returnList.Add(skillDayDto);
					}
				}
			}
			return returnList;
		}
	}
}