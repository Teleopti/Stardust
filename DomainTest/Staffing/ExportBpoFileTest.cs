using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class ExportBpoFileTest : ISetup
	{
		public ExportBpoFile Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		public FakeUserTimeZone UserTimeZone;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
		}

		[Test]
		public void ShouldReturnSomeParseData()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			
			var period = new DateOnlyPeriod(new DateOnly(2017,8,15), new DateOnly(2017, 8, 16));
			var forecastedData =  Target.ForecastData(skill, period, new CultureInfo("en-US", false));
			Assert.False(forecastedData.Equals(""));
		}

		[Test]
		public void ShouldReturnCorrectNumberOfRequiredFields()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ForecastData(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split( new[]{"\r\n"},StringSplitOptions.None);
			rows.Length.Should().Be.EqualTo(2);
			rows.First().Split(',').Length.Should().Be.EqualTo(7);
			rows.Second().Split(',').Length.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldReturnCurrentDemand()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 6,
					SkillCombination = new[] { skill.Id.GetValueOrDefault() }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ForecastData(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.First().Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,7.7");
			rows.Second().Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,9.7");
		}

		[Test]
		public void ShouldReturnCurrentDemandIfNoStaffingInfoFound()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ForecastData(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.First().Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,15.7");
			rows.Second().Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,15.7");
		}

		[Test]
		public void ShouldReturnCurrentDemandIfSomeStaffingInfoFound()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ForecastData(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.First().Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,7.7");
			rows.Second().Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,15.7");
		}

		[Test]
		public void ShouldReturnZeroForecastDemandInCaseOfOverStaffing()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 20,
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 20,
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ForecastData(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.First().Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,0");
			rows.Second().Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,0");
		}

		[Test]
		public void ShouldReturnStaffingWithCorrectTimeZone()
		{
			UserTimeZone.Is(TimeZoneInfo.Utc);
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
			var skillName = "Direct sales";
			var openHours = new TimePeriod(8, 0, 8, 30);
			var skill =
				new Skill(skillName, "description", Color.Empty, 15, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = timezone,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 20,
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 20,
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ForecastData(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.First().Should().Be.EqualTo("skill,20170815 8:00,20170815 8:15,0,0,0,0");
			rows.Second().Should().Be.EqualTo("skill,20170815 8:15,20170815 8:30,0,0,0,0");
		}

		[Test, Ignore("WIP")]
		public void ShouldReturnStaffingForMoreThanOneDay()
		{
			Assert.Pass();
		}

		//if we have data in bpo then dont consider it
		[Test]
		public void ShouldCompareActualStaffingWithForecast()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resources = 2,
					Source = "BPO",
					SkillIds = new List<Guid>() {skill.Id.GetValueOrDefault()}
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resources = 4,
					Source = "BPO",
					SkillIds = new List<Guid>() {skill.Id.GetValueOrDefault()}
				}
			});
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 4,
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 3,
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ForecastData(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.First().Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,11.7");
			rows.Second().Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,12.7");
		}

		//private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours, bool isClosedOnWeekends, int midnigthBreakOffset)
		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			//if (midnigthBreakOffset != 0)
			//{
			//	skill.MidnightBreakOffset = TimeSpan.FromHours(midnigthBreakOffset);
			//}

			//var workload = isClosedOnWeekends
			//	? WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, openHours)
			 var workload =  WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}
	}
}
