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
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

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
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public GetSkillDaysByPeriodQueryHandler(IDateTimePeriodAssembler dateTimePeriodAssembler,
			IAssembler<ISkillStaffPeriod, SkillDataDto> skillDataAssembler, ICurrentScenario scenarioRepository,
			IPersonRepository personRepository, 
			ISkillRepository skillRepository, 
			IResourceCalculationPrerequisitesLoader resourceCalculationPrerequisitesLoader,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IResourceCalculation resourceOptimizationHelper,
			ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation,
			ISchedulingResultStateHolder schedulingResultStateHolder)
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
		}

		public ICollection<SkillDayDto> Handle(GetSkillDaysByPeriodQueryDto query)
		{
			var dateOnlyPeriod = query.Period.ToDateOnlyPeriod();
			if(dateOnlyPeriod.DayCount() > 31)
				throw new FaultException();
			ICollection<SkillDayDto> returnList = new List<SkillDayDto>();
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var requestedScenario = _scenarioRepository.Current();
				_skillRepository.LoadAll();

				var period = dateOnlyPeriod.ToDateTimePeriod(timeZoneInfo);
				var periodForResourceCalc = new DateTimePeriod(period.StartDateTime.AddDays(-1), period.EndDateTime.AddDays(1));
				var dateOnlyPeriodForResourceCalc = periodForResourceCalc.ToDateOnlyPeriod(timeZoneInfo);
				_dateTimePeriodAssembler.TimeZone = timeZoneInfo;

				_resourceCalculationPrerequisitesLoader.Execute();

				var allPeople = _personRepository.FindPeopleInOrganization(dateOnlyPeriodForResourceCalc, true);

				_loadSchedulingStateHolderForResourceCalculation.Execute(requestedScenario, periodForResourceCalc, allPeople,
					_schedulingResultStateHolder);

				foreach (DateOnly dateTime in dateOnlyPeriodForResourceCalc.DayCollection())
				{
					_resourceOptimizationHelper.ResourceCalculate(dateTime,
						_schedulingResultStateHolder.ToResourceOptimizationData(true, false));
				}

				var dayCollection = dateOnlyPeriod.DayCollection();
				foreach (ISkill skill in _schedulingResultStateHolder.VisibleSkills)
				{
					foreach (var dateOnly in dayCollection)
					{
						var skillDayDto = new SkillDayDto {DisplayDate = new DateOnlyDto {DateTime = dateOnly.Date}};

						IList<ISkillStaffPeriod> skillStaffPeriods =
							_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(
								new List<ISkill> {skill}, dateOnly.ToDateOnlyPeriod().ToDateTimePeriod(timeZoneInfo));
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