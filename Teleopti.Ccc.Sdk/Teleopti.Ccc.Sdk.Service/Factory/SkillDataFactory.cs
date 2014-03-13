using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public class SkillDataFactory
	{
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
		private readonly IResourceCalculationPrerequisitesLoader _resourceCalculationPrerequisitesLoader;

		public SkillDataFactory(ICurrentScenario scenarioRepository, IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository, IWorkloadRepository workloadRepository, IScheduleRepository scheduleRepository, ISkillDayLoadHelper skillDayLoadHelper, IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider, IResourceCalculationPrerequisitesLoader resourceCalculationPrerequisitesLoader)
		{
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_skillRepository = skillRepository;
			_workloadRepository = workloadRepository;
			_scheduleRepository = scheduleRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_peopleAndSkillLoaderDecider = peopleAndSkillLoaderDecider;
			_resourceCalculationPrerequisitesLoader = resourceCalculationPrerequisitesLoader;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public ICollection<SkillDayDto> GetSkillData(DateOnlyDto dateOnlyDto, string timeZoneId)
		{
			ICollection<SkillDayDto> returnList = new List<SkillDayDto>();
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var requestedScenario = _scenarioRepository.Current();

				var period = PreparePeriod(timeZoneInfo, dateOnlyDto);
				var periodForResourceCalc = new DateTimePeriod(period.StartDateTime.AddDays(-1), period.EndDateTime.AddDays(1));
				var dateOnlyPeriodForResourceCalc = periodForResourceCalc.ToDateOnlyPeriod(timeZoneInfo);
					
				using (var schedulingResultStateHolder = new SchedulingResultStateHolder())
				{
					_resourceCalculationPrerequisitesLoader.Execute();
	
					var allPeople = _personRepository.FindPeopleInOrganization(dateOnlyPeriodForResourceCalc, true);
					var loader = new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository,
					                                                                 _skillRepository, _workloadRepository,
					                                                                 _scheduleRepository, schedulingResultStateHolder,
					                                                                 _peopleAndSkillLoaderDecider, _skillDayLoadHelper);
					loader.Execute(requestedScenario, periodForResourceCalc, allPeople);

					var personSkillProvider = new PersonSkillProvider();
					var resourceOptimizationHelper = new ResourceOptimizationHelper(schedulingResultStateHolder,
					                                                                new OccupiedSeatCalculator(),
					                                                                new NonBlendSkillCalculator(), personSkillProvider, new PeriodDistributionService(), 
					                                                                new CurrentTeleoptiPrincipal());
					foreach (DateOnly dateTime in dateOnlyPeriodForResourceCalc.DayCollection())
					{
						resourceOptimizationHelper.ResourceCalculateDate(dateTime, true, true);
					}
					foreach (ISkill skill in schedulingResultStateHolder.Skills)
					{
						SkillDayDto skillDayDto = new SkillDayDto();
						skillDayDto.DisplayDate = dateOnlyDto;
						IList<ISkillStaffPeriod> skillStaffPeriods =
							schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(
								new List<ISkill> { skill }, period);
						skillDayDto.Esl = SkillStaffPeriodHelper.EstimatedServiceLevel(skillStaffPeriods).Value;
						skillDayDto.SkillId = skill.Id.Value;
						skillDayDto.SkillName = skill.Name;

						var skillDataAssembler = new SkillDataAssembler(new DateTimePeriodAssembler());
						skillDayDto.SkillDataCollection = new List<SkillDataDto>(skillDataAssembler.DomainEntitiesToDtos(skillStaffPeriods));

						returnList.Add(skillDayDto);
					}
				}
			}
			return returnList;
		}

		private static DateTimePeriod PreparePeriod(TimeZoneInfo timeZoneInfo, DateOnlyDto dateOnlyDto)
		{
			DateTime fromDateTime = TimeZoneHelper.ConvertToUtc(dateOnlyDto.DateTime, timeZoneInfo);
			DateTime toDateTime = TimeZoneHelper.ConvertToUtc(dateOnlyDto.DateTime.AddDays(1), timeZoneInfo);
			return new DateTimePeriod(fromDateTime, toDateTime);
		}
	}
}
