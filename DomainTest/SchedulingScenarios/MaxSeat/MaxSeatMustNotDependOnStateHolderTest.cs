﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat.TestData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	public class MaxSeatMustNotDependOnStateHolderTest : IIsolateSystem
	{
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;
		public FakeGroupPageRepository GroupPageRepository;

		[Test]
		public void ShouldNotUseStateHolderWhenUsingHierarchy()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = new OptimizationPreferences
			{
				Extra =
				{
					UseTeams = true,
					TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy),
					UseTeamSameStartTime = true
				},
				Advanced =
				{
					UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats
				}
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);
			});
		}

		[Test]
		public void ShouldNotUseStateHolderWhenUsingUserDefinedSkillGrouping()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var groupPage = new GroupPage("_") {DescriptionKey = "UserDefined"};
			GroupPageRepository.Has(groupPage);
			var optPreferences = new OptimizationPreferences
			{
				Extra =
				{
					UseTeams = true,
					TeamGroupPage = new GroupPageLight("_", GroupPageType.UserDefined),
					UseTeamSameStartTime = true
				},
				Advanced =
				{
					UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats
				}
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);
			});
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<schedulerStateHolderThatThrows>().For<ISchedulerStateHolder, ISchedulingResultStateHolder>();
		}


		private class schedulerStateHolderThatThrows : ISchedulerStateHolder, ISchedulingResultStateHolder
		{
			public bool ConsiderShortBreaks
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public ISchedulingResultStateHolder SchedulingResultState
			{
				get { throw new NotImplementedException(); }
			}

			public IDateOnlyPeriodAsDateTimePeriod RequestedPeriod
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public IScenario RequestedScenario
			{
				get { throw new NotImplementedException(); }
			}

			public IList<IPerson> ChoosenAgents
			{
				get { throw new NotImplementedException(); }
			}

			public void ResetFilteredPersons()
			{
				throw new NotImplementedException();
			}

			public void ResetFilteredPersonsOvertimeAvailability()
			{
				throw new NotImplementedException();
			}

			public void ResetFilteredPersonsHourlyAvailability()
			{
				throw new NotImplementedException();
			}

			public void LoadSchedules(IFindSchedulesForPersons findSchedulesForPersons, IEnumerable<IPerson> personsInOrganization,
				ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateTimePeriod period)
			{
				throw new NotImplementedException();
			}

			public IDictionary<IPerson, IPersonAccountCollection> AllPersonAccounts
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public bool SkipResourceCalculation
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public ICollection<IPerson> LoadedAgents
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			IScheduleDictionary ISchedulingResultStateHolder.Schedules
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public ISkill[] Skills
			{
				get { throw new NotImplementedException(); }
			}

			public IList<ISkill> VisibleSkills
			{
				get { throw new NotImplementedException(); }
			}

			public IEnumerable<ISkillDay> SkillDaysOnDateOnly(IEnumerable<DateOnly> theDateList)
			{
				throw new NotImplementedException();
			}

			public ISkillStaffPeriodHolder SkillStaffPeriodHolder
			{
				get { throw new NotImplementedException(); }
			}

			public bool TeamLeaderMode
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public bool UseMaximumWorkday { get; set; }

			public bool UseValidation
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public INewBusinessRuleCollection GetRulesToRun()
			{
				throw new NotImplementedException();
			}

			public ISkillDay SkillDayOnSkillAndDateOnly(ISkill skill, DateOnly dateOnly)
			{
				throw new NotImplementedException();
			}

			public ISeniorityWorkDayRanks SeniorityWorkDayRanks
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public IEnumerable<ExternalStaff> ExternalStaff
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public void AddSkills(params ISkill[] skills)
			{
				throw new NotImplementedException();
			}

			public void ClearSkills()
			{
				throw new NotImplementedException();
			}

			public void RemoveSkill(ISkill skill)
			{
				throw new NotImplementedException();
			}

			public bool GuessResourceCalculationHasBeenMade()
			{
				throw new NotImplementedException();
			}

			public ResourceCalculationData ToResourceOptimizationData(bool considerShortBreaks, bool doIntraIntervalCalculation)
			{
				throw new NotImplementedException();
			}

			public int MinimumSkillIntervalLength()
			{
				throw new NotImplementedException();
			}

			IScheduleDictionary ISchedulerStateHolder.Schedules
			{
				get { throw new NotImplementedException(); }
			}

			public IDictionary<Guid, IPerson> FilteredCombinedAgentsDictionary
			{
				get { throw new NotImplementedException(); }
			}

			public IDictionary<Guid, IPerson> FilteredAgentsDictionary
			{
				get { throw new NotImplementedException(); }
			}

			public TimeZoneInfo TimeZoneInfo
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public ShiftTradeRequestStatusCheckerWithSchedule ShiftTradeRequestStatusChecker
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public ICommonStateHolder CommonStateHolder
			{
				get { throw new NotImplementedException(); }
			}

			public void LoadCommonState(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
			{
				throw new NotImplementedException();
			}

			public CommonNameDescriptionSetting CommonNameDescription
			{
				get { throw new NotImplementedException(); }
			}

			public int DefaultSegmentLength
			{
				get { throw new NotImplementedException(); }
			}

			public void SetRequestedScenario(IScenario scenario)
			{
				throw new NotImplementedException();
			}

			public void FilterPersonsOvertimeAvailability(IEnumerable<IPerson> selectedPersons)
			{
				throw new NotImplementedException();
			}

			public void FilterPersonsHourlyAvailability(IList<IPerson> selectedPersons)
			{
				throw new NotImplementedException();
			}

			public void FilterPersons(HashSet<Guid> selectedGuids)
			{
				throw new NotImplementedException();
			}

			public void ClearDaysToRecalculate()
			{
				throw new NotImplementedException();
			}

			public void LoadSettings(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
			{
				throw new NotImplementedException();
			}

			public void MarkDateToBeRecalculated(DateOnly dateToRecalculate)
			{
				throw new NotImplementedException();
			}

			public string CommonAgentName(IPerson person)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<DateOnly> DaysToRecalculate
			{
				get { throw new NotImplementedException(); }
			}

			public bool AgentFilter()
			{
				throw new NotImplementedException();
			}
		}
	}
}