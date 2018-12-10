using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[DomainTest]
	public class TeamBlockDaysOffSameDaysOffLockSyncronizerTest
	{
		public TeamBlockDaysOffSameDaysOffLockSyncronizer Target;
		public IGroupPersonBuilderWrapper GroupPersonBuilderWrapper;
		public MatrixListFactory MatrixListFactory;
		public IGroupPersonBuilderForOptimizationFactory GroupPersonBuilderForOptimizationFactory;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldLockAllIfOneIsLockedAndUseSameDaysOff()
		{
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("scenario");
			var schedulePeriod1 = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var schedulePeriod2 = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current();
			BusinessUnitRepository.Add(businessUnit);
			var sameSite = SiteFactory.CreateSiteWithOneTeam("team");
			businessUnit.AddSite(sameSite);
			var sameTeam = businessUnit.SiteCollection[0].TeamCollection[0];
			var agent1 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(dateOnly, sameTeam).WithId();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneGuard.Instance.TimeZone);
			agent1.AddSchedulePeriod(schedulePeriod1);
			var agent2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(dateOnly, sameTeam).WithId();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneGuard.Instance.TimeZone);
			agent2.AddSchedulePeriod(schedulePeriod2);
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(2));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(new DateOnlyPeriodAsDateTimePeriod(selectedPeriod, TimeZoneGuard.Instance.TimeZone).Period(), new[] { agent1, agent2 }).VisiblePeriod);
			var allPersonMatrixList = MatrixListFactory.CreateMatrixListAllForLoadedPeriod(scheduleDictionary, new[] {agent1, agent2}, selectedPeriod);
			var optimizationPreferences = new OptimizationPreferences
			{
				Extra =
				{
					UseTeams = true,
					TeamGroupPage = new GroupPageLight("team", GroupPageType.Hierarchy),
					UseTeamSameShiftCategory = true,
					UseTeamSameDaysOff = true
				}
			};
			var allTeamInfoListOnStartDate = new List<ITeamInfo>();
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			GroupPersonBuilderForOptimizationFactory.Create(new[] { agent1, agent2 }, scheduleDictionary, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var teamInfoFactory = new TeamInfoFactory(GroupPersonBuilderWrapper);			
			var teamInfo = teamInfoFactory.CreateTeamInfo(new[] { agent1, agent2 }, agent1, selectedPeriod, allPersonMatrixList);
			allTeamInfoListOnStartDate.Add(teamInfo);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent1, dateOnly).UnlockPeriod(selectedPeriod);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent1, dateOnly).LockDay(dateOnly);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent2, dateOnly).UnlockPeriod(selectedPeriod);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent2, dateOnly).LockDay(dateOnly.AddDays(2));

			Target.SyncLocks(selectedPeriod, optimizationPreferences, allTeamInfoListOnStartDate);

			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent1, dateOnly).UnlockedDays.Length.Should().Be.EqualTo(1);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent2, dateOnly).UnlockedDays.Length.Should().Be.EqualTo(1);

			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent1, dateOnly).UnlockedDays[0].Day.Should()
				.Be.EqualTo(dateOnly.AddDays(1));
		}

		[Test]
		public void ShouldNotLockAllIfOneIsLockedAndNotUseSameDaysOff()
		{
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("scenario1");
			var schedulePeriod1 = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var schedulePeriod2 = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current();
			BusinessUnitRepository.Add(businessUnit);
			var sameSite = SiteFactory.CreateSiteWithOneTeam("team");
			businessUnit.AddSite(sameSite);
			var sameTeam = businessUnit.SiteCollection[0].TeamCollection[0];
			var agent1 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(dateOnly, sameTeam).WithId();
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneGuard.Instance.TimeZone);
			agent1.AddSchedulePeriod(schedulePeriod1);
			var agent2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(dateOnly, sameTeam).WithId();
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneGuard.Instance.TimeZone);
			agent2.AddSchedulePeriod(schedulePeriod2);

			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(2));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(new DateOnlyPeriodAsDateTimePeriod(selectedPeriod, TimeZoneGuard.Instance.TimeZone).Period(), new[] { agent1, agent2 }).VisiblePeriod);
			var allPersonMatrixList = MatrixListFactory.CreateMatrixListAllForLoadedPeriod(scheduleDictionary, new[] { agent1, agent2 }, selectedPeriod);
			var optimizationPreferences = new OptimizationPreferences
			{
				Extra =
				{
					UseTeams = true,
					TeamGroupPage = new GroupPageLight("team", GroupPageType.Hierarchy),
					UseTeamSameShiftCategory = true,
					UseTeamSameDaysOff = false
				}
			};
			var allTeamInfoListOnStartDate = new List<ITeamInfo>();
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			GroupPersonBuilderForOptimizationFactory.Create(new[]{ agent1, agent2 }, scheduleDictionary, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var teamInfoFactory = new TeamInfoFactory(GroupPersonBuilderWrapper);
			var teamInfo = teamInfoFactory.CreateTeamInfo(new[] { agent1, agent2 }, agent1, selectedPeriod, allPersonMatrixList);
			allTeamInfoListOnStartDate.Add(teamInfo);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent1, dateOnly).UnlockPeriod(selectedPeriod);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent1, dateOnly).LockDay(dateOnly);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent2, dateOnly).UnlockPeriod(selectedPeriod);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent2, dateOnly).LockDay(dateOnly.AddDays(2));

			Target.SyncLocks(selectedPeriod, optimizationPreferences, allTeamInfoListOnStartDate);

			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent1, dateOnly).UnlockedDays.Length.Should().Be.EqualTo(2);
			allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent2, dateOnly).UnlockedDays.Length.Should().Be.EqualTo(2);

			var agentUnLockedDates =
				allTeamInfoListOnStartDate[0].MatrixForMemberAndDate(agent1, dateOnly).UnlockedDays.Select(d => d.Day).ToList();

			agentUnLockedDates.Should().Contain(dateOnly.AddDays(1));
			agentUnLockedDates.Should().Contain(dateOnly.AddDays(2));			
		}
	}
}