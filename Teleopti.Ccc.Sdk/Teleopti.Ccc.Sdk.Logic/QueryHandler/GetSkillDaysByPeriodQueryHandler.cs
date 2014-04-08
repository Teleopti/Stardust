using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetSkillDaysByPeriodQueryHandler : IHandleQuery<GetSkillDaysByPeriodQueryDto,ICollection<SkillDayDto>>
	{
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
		private readonly IAssembler<ISkillStaffPeriod, SkillDataDto> _skillDataAssembler;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IResourceCalculationPrerequisitesLoader _resourceCalculationPrerequisitesLoader;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IServiceLevelCalculator _serviceLevelCalculator;

		public GetSkillDaysByPeriodQueryHandler(IDateTimePeriodAssembler dateTimePeriodAssembler,
			IAssembler<ISkillStaffPeriod, SkillDataDto> skillDataAssembler, ICurrentScenario scenarioRepository,
			IPersonRepository personRepository, 
			ISkillRepository skillRepository, 
			IResourceCalculationPrerequisitesLoader resourceCalculationPrerequisitesLoader,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IResourceOptimizationHelper resourceOptimizationHelper,
			ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IServiceLevelCalculator serviceLevelCalculator)
		{
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_skillDataAssembler = skillDataAssembler;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_resourceCalculationPrerequisitesLoader = resourceCalculationPrerequisitesLoader;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_loadSchedulingStateHolderForResourceCalculation = loadSchedulingStateHolderForResourceCalculation;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_serviceLevelCalculator = serviceLevelCalculator;
		}

		public ICollection<SkillDayDto> Handle(GetSkillDaysByPeriodQueryDto query)
		{
			if(query.Period.ToDateOnlyPeriod().DayCollection().Count > 31)
				throw new FaultException();
			ICollection<SkillDayDto> returnList = new List<SkillDayDto>();
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
			using (_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var requestedScenario = _scenarioRepository.Current();
				_skillRepository.LoadAll();

				var dateOnlyPeriod = query.Period.ToDateOnlyPeriod();
				var period = dateOnlyPeriod.ToDateTimePeriod(timeZoneInfo);
				var periodForResourceCalc = new DateTimePeriod(period.StartDateTime.AddDays(-1), period.EndDateTime.AddDays(1));
				var dateOnlyPeriodForResourceCalc = periodForResourceCalc.ToDateOnlyPeriod(timeZoneInfo);
				_dateTimePeriodAssembler.TimeZone = timeZoneInfo;

				_resourceCalculationPrerequisitesLoader.Execute();

					var allPeople = _personRepository.FindPeopleInOrganization(dateOnlyPeriodForResourceCalc, true);
					
					_loadSchedulingStateHolderForResourceCalculation.Execute(requestedScenario, periodForResourceCalc, allPeople);

					foreach (DateOnly dateTime in dateOnlyPeriodForResourceCalc.DayCollection())
					{
						_resourceOptimizationHelper.ResourceCalculateDate(dateTime, true, true);
					}

					foreach (ISkill skill in _schedulingResultStateHolder.VisibleSkills)
					{
						foreach (var dateOnly in dateOnlyPeriod.DayCollection())
						{
							var skillDayDto = new SkillDayDto { DisplayDate = new DateOnlyDto { DateTime = dateOnly } };

							IList<ISkillStaffPeriod> skillStaffPeriods =
								_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(
									new List<ISkill> { skill }, new DateOnlyPeriod(dateOnly,dateOnly).ToDateTimePeriod(timeZoneInfo));
							skillDayDto.Esl = _serviceLevelCalculator.EstimatedServiceLevel(skillStaffPeriods).Value;
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