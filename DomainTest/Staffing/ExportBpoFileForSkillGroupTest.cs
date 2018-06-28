using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class ExportBpoFileForSkillGroupTest : IIsolateSystem
	{
		public IExportBpoFile Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		public FakeUserTimeZone UserTimeZone;
		public FakeSkillGroupRepository SkillGroupRepository;
		private readonly ForecastsRowExtractor ForecastsRowExtractor = new ForecastsRowExtractor();

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
		}

		[Test]
		public void ShouldReturnSomeParseData()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			
			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			Assert.False(forecastedData.Equals(""));
		}

		[Test]
		public void ShouldReturnCorrectNumberOfRequiredFieldsForSingleSkill()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Add(skill1);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);

			SkillDayRepository.Add(skill1Day);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Split(',').Length.Should().Be(7);
			rows[2].Split(',').Length.Should().Be(7);
		}

		[Test]
		public void ShouldReturnCurrentDemand()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 15, 8, 45));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 15, 8, 45), 15.7);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(4);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,15.7");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,31.4");
			rows[3].Should().Be.EqualTo("skill1|skill2,20170815 08:30,20170815 08:45,0,0,0,15.7");
		}

		[Test]
		public void ShouldReturnCurrentDemandWithStaffing()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 6,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault() }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,17.4");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,31.4");
		}

		[Test]
		public void ShouldReturnCurrentDemandWithStaffingOnDifferentIntervals()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 6,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault() }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,23.4");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,25.4");
		}

		[Test]
		public void ShouldExportStaffingForTheGivenPeriod()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 16), new TimePeriod(8, 0, 8, 30), 15.7);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 16, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 16, 8, 30, 0).Utc(),
					Resource = 6,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault() }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 15));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,7.7");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,15.7");
		}

		[Test]
		public void ShouldExportStaffingForASkillCombination()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));

			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 14.3);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] {  skill2.Id.GetValueOrDefault() , skill1.Id.GetValueOrDefault() }
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 2.1,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault(),  }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,19.9");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,30");
		}

		[Test]
		public void ShouldExportStaffingForComplexSkillCombination()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));
			var skill3 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));

			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			SkillRepository.Add(skill3);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 16);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 14);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] {  skill2.Id.GetValueOrDefault() , skill1.Id.GetValueOrDefault() }
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 3,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault(),skill3.Id.GetValueOrDefault()  }
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 2,
					SkillCombination = new[] { skill3.Id.GetValueOrDefault(),  }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,19");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,30");
		}

		[Test]
		public void ShouldReturnZeroGapIfOverstaff()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 3);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 3);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 6,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault() }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,0");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,0");
		}

		[Test]
		public void ShouldReturnStaffingWithCorrectTimeZoneForOneSkill()
		{
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			UserTimeZone.Is(timezone);
			timezone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
			var openHours = new TimePeriod(8, 0, 8, 30);
			var skill1 =
				new Skill("skill1", "description", Color.Empty, 15, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = timezone,
					Activity = new Activity("activity_" + "skill1").WithId()
				}.WithId();
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill1, openHours);
			workload.SetId(Guid.NewGuid());

			SkillRepository.Add(skill1);
			
			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			
			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 3);

			SkillDayRepository.Add(skill1Day);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 11, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 11, 15, 0).Utc(),
					Resource = 4,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 11, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 11, 30, 0).Utc(),
					Resource = 10,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1,20170815 13:00,20170815 13:15,0,0,0,0");
			rows[2].Should().Be.EqualTo("skill1,20170815 13:15,20170815 13:30,0,0,0,0");
		}

		[Test]
		public void ShouldReturnStaffingWithCorrectTimeZoneForTwoSkill()
		{
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			UserTimeZone.Is(timezone);
			timezone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
			var openHoursSkill1 = new TimePeriod(8, 0, 8, 30);
			var openHoursSkill2 = new TimePeriod(11, 0, 11, 30);
			var skill1 =
				new Skill("skill1", "description", Color.Empty, 15, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = timezone,
					Activity = new Activity("activity_" + "skill1").WithId()
				}.WithId();
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill1, openHoursSkill1);
			workload.SetId(Guid.NewGuid());

			var skill2 = createSkill(15, "skill2", openHoursSkill2);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), openHoursSkill1, 3);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), openHoursSkill2, 3);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 11, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 11, 15, 0).Utc(),
					Resource = 4,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 11, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 11, 30, 0).Utc(),
					Resource = 10,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 13:00,20170815 13:15,0,0,0,2");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 13:15,20170815 13:30,0,0,0,0");
		}

		//if we have data in bpo then dont consider it
		[Test]
		public void ShouldCompareActualStaffingWithForecast()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 0, 8, 30));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.8645445);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.8645445);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);

			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resources = 2,
					Source = "BPO",
					SkillIds = new List<Guid>() {skill1.Id.GetValueOrDefault()}
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resources = 4,
					Source = "BPO",
					SkillIds = new List<Guid>() {skill2.Id.GetValueOrDefault()}
				}
			});
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 4.21515151,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 3.21151515,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault()}
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,27.51");
			rows[2].Should().Be("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,28.52");
		}

		[Test]
		public void ShouldExportStaffingForSkillsWithDifferentOpeningHours()
		{
			var openHoursSkill1 = new TimePeriod(8, 0, 9, 0);
			var openHoursSkill2 = new TimePeriod(8, 30, 9, 30);

			var skill1 = createSkill(15, "skill1", openHoursSkill1 );
			var skill2 = createSkill(15, "skill2", openHoursSkill2);

			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), openHoursSkill1, 14);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), openHoursSkill2, 10);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 8,
					SkillCombination = new[] {   skill1.Id.GetValueOrDefault() }
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 2.1,
					SkillCombination = new[] { skill1.Id.GetValueOrDefault(),  }
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 45, 0).Utc(),
					Resource = 30,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault(), skill1.Id.GetValueOrDefault(),  }
				}
				,
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 9, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 9, 15, 0).Utc(),
					Resource = 4,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault(),  }
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 08, 15, 9, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 9, 30, 0).Utc(),
					Resource =6,
					SkillCombination = new[] { skill2.Id.GetValueOrDefault(),  }
				}
			});

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(7);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,6");
			rows[2].Should().Be.EqualTo("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,11.9");
			rows[3].Should().Be.EqualTo("skill1|skill2,20170815 08:30,20170815 08:45,0,0,0,0");
			rows[4].Should().Be.EqualTo("skill1|skill2,20170815 08:45,20170815 09:00,0,0,0,24");
			rows[5].Should().Be.EqualTo("skill1|skill2,20170815 09:00,20170815 09:15,0,0,0,6");
			rows[6].Should().Be.EqualTo("skill1|skill2,20170815 09:15,20170815 09:30,0,0,0,4");
		}

		[Test]
		public void ShouldReturnDemandWithSkillsOnDifferentResolution()
		{
			var openHoursSkill = new TimePeriod(8, 0, 9, 0);
			var skill1 = createSkill(15, "skill1", openHoursSkill);
			var skill2 = createSkill(60, "skill2", openHoursSkill);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), openHoursSkill, 5);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), openHoursSkill, 4);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false));
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(5);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be("skill1|skill2,20170815 08:00,20170815 08:15,0,0,0,9");
			rows[2].Should().Be("skill1|skill2,20170815 08:15,20170815 08:30,0,0,0,9");
			rows[3].Should().Be("skill1|skill2,20170815 08:30,20170815 08:45,0,0,0,9");
			rows[4].Should().Be("skill1|skill2,20170815 08:45,20170815 09:00,0,0,0,9");
		}

		[Test]
		public void ShouldReturnExportWithCorrectSeperator()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 15, 8, 45));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 15, 8, 45), 15.7);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false),";");
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(4);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2;20170815 08:00;20170815 08:15;0;0;0;15.7");
			rows[2].Should().Be.EqualTo("skill1|skill2;20170815 08:15;20170815 08:30;0;0;0;31.4");
			rows[3].Should().Be.EqualTo("skill1|skill2;20170815 08:30;20170815 08:45;0;0;0;15.7");
		}

		[Test]
		public void ShouldReturnExportWithCorrectDateFormat()
		{
			var skill1 = createSkill(15, "skill1", new TimePeriod(8, 0, 8, 30));
			var skill2 = createSkill(15, "skill2", new TimePeriod(8, 15, 8, 45));
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			var intradaySkill1 = new SkillInIntraday { Id = skill1.Id.Value };
			var intradaySkill2 = new SkillInIntraday { Id = skill2.Id.Value };

			var skillGroup = new SkillGroup { Name = "SkillGroup" }.WithId();
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill2 };
			SkillGroupRepository.Add(skillGroup);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skill1Day = SkillSetupHelper.CreateSkillDayWithDemand(skill1, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			var skill2Day = SkillSetupHelper.CreateSkillDayWithDemand(skill2, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 15, 8, 45), 15.7);

			SkillDayRepository.Add(skill1Day);
			SkillDayRepository.Add(skill2Day);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skillGroup.Id.Value, period, new CultureInfo("en-US", false), ";","ddMMyyyy HH:mm");
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(4);
			rows[0].Should().Be(ForecastsRowExtractor.HeaderRow);
			rows[1].Should().Be.EqualTo("skill1|skill2;15082017 08:00;15082017 08:15;0;0;0;15.7");
			rows[2].Should().Be.EqualTo("skill1|skill2;15082017 08:15;15082017 08:30;0;0;0;31.4");
			rows[3].Should().Be.EqualTo("skill1|skill2;15082017 08:30;15082017 08:45;0;0;0;15.7");
		}

		protected ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}
	}
}