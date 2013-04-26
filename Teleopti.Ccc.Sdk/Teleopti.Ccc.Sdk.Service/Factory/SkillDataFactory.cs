using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	internal static class SkillDataFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		internal static ICollection<SkillDayDto> GetSkillData(DateOnlyDto dateOnlyDto, string timeZoneId)
		{
			IRepositoryFactory repositoryFactory = new RepositoryFactory();
			ICollection<SkillDayDto> returnList = new List<SkillDayDto>();
			TimeZoneInfo timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IScenarioRepository scenarioRepository = repositoryFactory.CreateScenarioRepository(unitOfWork);
				IScenario requestedScenario = scenarioRepository.LoadDefaultScenario();

				DateTimePeriod period = PreparePeriod(timeZoneInfo, dateOnlyDto);
				DateTimePeriod periodForResourceCalc = new DateTimePeriod(period.StartDateTime.AddDays(-1), period.EndDateTime.AddDays(1));

				IPersonRepository personRepository = repositoryFactory.CreatePersonRepository(unitOfWork);
				using (SchedulingResultStateHolder schedulingResultStateHolder = new SchedulingResultStateHolder())
				{
					LoadSchedulingStateHolderForResourceCalculation loader =
						new LoadSchedulingStateHolderForResourceCalculation(personRepository,
																			repositoryFactory.
																				CreatePersonAbsenceAccountRepository(
																					unitOfWork),
																			repositoryFactory.CreateSkillRepository(
																				unitOfWork),
																			repositoryFactory.CreateWorkloadRepository(
																				unitOfWork),
																			repositoryFactory.CreateScheduleRepository(
																				unitOfWork), schedulingResultStateHolder,
																			new PeopleAndSkillLoaderDecider(
																				personRepository),
																			new SkillDayLoadHelper(
																				repositoryFactory.
																					CreateSkillDayRepository
																					(unitOfWork),
																				repositoryFactory.
																					CreateMultisiteDayRepository
																					(
																						unitOfWork)));
					loader.Execute(requestedScenario, periodForResourceCalc,
								   personRepository.FindPeopleInOrganization(periodForResourceCalc.ToDateOnlyPeriod(timeZoneInfo), true));

					var resourceOptimizationHelper = new ResourceOptimizationHelper(schedulingResultStateHolder,
					                                                                new OccupiedSeatCalculator(
						                                                                new SkillVisualLayerCollectionDictionaryCreator(),
						                                                                new SeatImpactOnPeriodForProjection()),
					                                                                new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection()),
					                                                                new SingleSkillDictionary(),
					                                                                new SingleSkillMaxSeatCalculator(),
					                                                                new CurrentTeleoptiPrincipal());
					foreach (DateOnly dateTime in periodForResourceCalc.ToDateOnlyPeriod(timeZoneInfo).DayCollection())
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
