using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	[NoDefaultData]
	public class ExportBpoFileTest : IIsolateSystem
	{
		public IExportBpoFile Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		public FakeUserTimeZone UserTimeZone;
		private readonly ForecastsRowExtractor ForecastsRowExtractor = new ForecastsRowExtractor();

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
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
			var forecastedData =  Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
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
			var forecastedData = Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split( new[]{"\r\n"},StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Split(',').Length.Should().Be(7);
			rows[2].Split(',').Length.Should().Be(7);
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
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 6,
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault() }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,7.7");
			rows[2].Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,9.7");
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
			var forecastedData = Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,15.7");
			rows[2].Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,15.7");
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
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,7.7");
			rows[2].Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,15.7");
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
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 20,
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill,20170815 08:00,20170815 08:15,0,0,0,0");
			rows[2].Should().Be.EqualTo("skill,20170815 08:15,20170815 08:30,0,0,0,0");
		}

		[Test]
		public void ShouldReturnStaffingWithCorrectTimeZone()
		{
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			UserTimeZone.Is(timezone);
			timezone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
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
					StartDateTime = new DateTime(2017, 08, 15, 11, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 11, 15, 0).Utc(),
					Resource = 4,
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 11, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 11, 30, 0).Utc(),
					Resource = 10,
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("Direct sales,20170815 13:00,20170815 13:15,0,0,0,11.7");
			rows[2].Should().Be.EqualTo("Direct sales,20170815 13:15,20170815 13:30,0,0,0,5.7");
		}

		//if we have data in bpo then dont consider it
		[Test]
		public void ShouldCompareActualStaffingWithForecast()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.8645445);
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
					SkillIds = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resources = 4,
					Source = "BPO",
					SkillIds = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 4.21515151,
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 3.21151515,
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be("skill,20170815 08:00,20170815 08:15,0,0,0,11.65");
			rows[2].Should().Be("skill,20170815 08:15,20170815 08:30,0,0,0,12.65");
		}
		
		[Test]
		public void ShouldIncludeHeaderOnFirstRow()
		{
			var skill = createSkill(15, "skill", new TimePeriod(8, 0, 8, 30));
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			
			var period = new DateOnlyPeriod(new DateOnly(2017,8,15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period, new CultureInfo("en-US", false));
			var firstRow = forecastedData.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
			ForecastsRowExtractor.IsValidHeaderRow(firstRow).Should().Be.True();
		}

		protected ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			 var workload =  WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}
	}
}
