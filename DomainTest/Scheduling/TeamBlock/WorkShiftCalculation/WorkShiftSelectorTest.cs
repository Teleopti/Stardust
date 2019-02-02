using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[DomainTest]
	public class WorkShiftSelectorTest
	{
		public IWorkShiftSelector Target;
		public IGroupPersonSkillAggregator GroupPersonSkillAggregator;
		public IGroupPersonBuilderWrapper GroupPersonBuilderWrapper;
		public MatrixListFactory MatrixListFactory;
		public IGroupPersonBuilderForOptimizationFactory GroupPersonBuilderForOptimizationFactory;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldSelectShiftWithHighestValue()
		{
			var date = new DateOnly(2017, 6, 9);
			var scenario = new Scenario("_");
			var skill = SkillFactory.CreateSkill("skill", new SkillTypePhone(new Description(),ForecastSource.InboundTelephony ), 60);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, date, TimeSpan.FromHours(1),
				new Tuple<int, TimeSpan>(9, TimeSpan.FromMinutes(90)));
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				skillStaffPeriod.SetCalculatedResource65(0.5);
			}

			var timeZoneInfo = TimeZoneInfo.Utc;
			var workShift1 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(8), TimeSpan.FromHours(9), skill.Activity);
			var workShift2 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(9), TimeSpan.FromHours(10), skill.Activity); //should win
			var workShift3 = WorkShiftFactory.CreateWorkShift(TimeSpan.FromHours(10), TimeSpan.FromHours(11), skill.Activity);
			var cache1 = new ShiftProjectionCache(workShift1, new DateOnlyAsDateTimePeriod(date, timeZoneInfo));
			var cache2 = new ShiftProjectionCache(workShift2, new DateOnlyAsDateTimePeriod(date, timeZoneInfo));
			var cache3 = new ShiftProjectionCache(workShift3, new DateOnlyAsDateTimePeriod(date, timeZoneInfo));
			var caches = new List<ShiftProjectionCache> {cache1, cache2, cache3};
			
			var businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.Current();
			BusinessUnitRepository.Add(businessUnit);
			var sameSite = SiteFactory.CreateSiteWithOneTeam("team");
			businessUnit.AddSite(sameSite);
			var sameTeam = businessUnit.SiteCollection[0].TeamCollection[0];
			var agent1 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(date, sameTeam).WithId();
			agent1.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
			((IPersonPeriodModifySkills) agent1.Period(date)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var selectedPeriod = new DateOnlyPeriod(date, date.AddDays(2));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(new DateOnlyPeriodAsDateTimePeriod(selectedPeriod, timeZoneInfo).Period(), new[] { agent1 }).VisiblePeriod);
			var allPersonMatrixList = MatrixListFactory.CreateMatrixListAllForLoadedPeriod(scheduleDictionary, new[] { agent1 }, selectedPeriod);
			GroupPersonBuilderForOptimizationFactory.Create(new[] { agent1 }, scheduleDictionary, new GroupPageLight("_", GroupPageType.SingleAgent));

			var teamInfoFactory = new TeamInfoFactory(GroupPersonBuilderWrapper);
			var teamInfo = teamInfoFactory.CreateTeamInfo(new[] { agent1 }, agent1, selectedPeriod, allPersonMatrixList);
			var teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(new DateOnlyPeriod(date, date)));

			var result = Target.SelectShiftProjectionCache(GroupPersonSkillAggregator, date, caches,
				new List<ISkillDay> {skillDay}, teamBlockInfo, new SchedulingOptions(), true,
				agent1);

			result.WorkShiftStartTime().Hours.Should().Be.EqualTo(9);
		}
	}
}