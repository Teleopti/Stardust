using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class ExportForecastAndStaffingFileTest: IIsolateSystem
	{
		public ExportForecastAndStaffingFile Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		public FakeStaffingSettingsReader StaffingSettingsReader;
		public FakeUserUiCulture FakeUserUiCulture;
		public FakeUserCulture FakeUserCulture;
		public FakeUserTimeZone UserTimeZone;


		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeStaffingSettingsReader>().For<IStaffingSettingsReader>();
			isolate.UseTestDouble<FakeUserUiCulture>().For<IUserUiCulture>();
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
		}

		[Test]
		public void ShouldHandleWhenSkillIsMissing()
		{
			StaffingSettingsReader.StaffingSettings.Add(KeyNames.StaffingReadModelHistoricalHours, 8 * 24);
			Now.Is(new DateTime(2017, 8, 18));
			FakeUserCulture.Is(new CultureInfo("en-US"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			SkillRepository.Add(skill);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 18), new DateOnly(2017, 8, 19));
			var exportStaffingReturnObject = Target.ExportForecastAndStaffing(new Guid(), period.StartDate, period.EndDate, false);
			exportStaffingReturnObject.ErrorMessage.Should().Contain("Cannot find skill with id");
		}

		[Test]
		//[SetUICulture("sv-SE")]
		public void ShouldReturnErrorWhenUsingDatesEarlierThanReadModel()
		{
			StaffingSettingsReader.StaffingSettings.Add(KeyNames.StaffingReadModelHistoricalHours, 8 * 24);
			Now.Is(new DateTime(2017, 8, 18));
			FakeUserCulture.IsSwedish();
			FakeUserUiCulture.IsSwedish();
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			SkillRepository.Add(skill);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 9), new DateOnly(2017, 8, 16));
			var exportStaffingReturnObject = Target.ExportForecastAndStaffing(skill.Id.GetValueOrDefault(), period.StartDate, period.EndDate, false);
			exportStaffingReturnObject.ErrorMessage.Should().Contain("datumintervallet 2017-08-10 - 2017-09-01");
		}

		[Test]
		public void ShouldHandleDatesInPast()
		{
			StaffingSettingsReader.StaffingSettings.Add(KeyNames.StaffingReadModelHistoricalHours, 3 * 24);
			Now.Is(new DateTime(2017, 8, 18));
			FakeUserCulture.Is(new CultureInfo("en-US"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			SkillRepository.Add(skill);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var exportStaffingReturnObject = Target.ExportForecastAndStaffing(skill.Id.GetValueOrDefault(), period.StartDate, period.EndDate, false);
			exportStaffingReturnObject.ErrorMessage.Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleNoForecastAndNoStaffing()
		{
			FakeUserCulture.Is(new CultureInfo("en-US"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			SkillRepository.Add(skill);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(1);
			rows[0].Should().Be.Empty();
		}
		
		[Test]
		public void ShouldHandleNoStaffing()
		{
			FakeUserCulture.Is(new CultureInfo("en-US"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill,startdatetime,enddatetime,forecasted agents,total scheduled agents,total diff,total scheduled heads");
			rows[1].Should().Be.EqualTo("skillname,8/15/2017 8:00 AM,8/15/2017 8:15 AM,15.7,0,-15.7,0");
			rows[2].Should().Be.EqualTo("skillname,8/15/2017 8:15 AM,8/15/2017 8:30 AM,15.7,0,-15.7,0");
		}
		
		[Test]
		public void ShouldReturnInternalStaffingUsCulture()
		{
			FakeUserCulture.Is(new CultureInfo("en-US"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
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


			var heads = new List<ScheduledHeads>
			{
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Heads = 8
				},
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Heads = 6
				}
			};
			SkillCombinationResourceRepository.ScheduledHeadsForSkillList.AddRange(heads);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill,startdatetime,enddatetime,forecasted agents,total scheduled agents,total diff,total scheduled heads");
			rows[1].Should().Be.EqualTo("skillname,8/15/2017 8:00 AM,8/15/2017 8:15 AM,15.7,8,-7.7,8");
			rows[2].Should().Be.EqualTo("skillname,8/15/2017 8:15 AM,8/15/2017 8:30 AM,15.7,6,-9.7,6");
		}
		
		[Test]
		public void ShouldReturnInternalStaffingSweCulture()
		{
			FakeUserCulture.Is(new CultureInfo("sv-SE"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
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
			
			var heads = new List<ScheduledHeads>
			{
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Heads = 8
				},
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Heads = 6
				}
			};
			SkillCombinationResourceRepository.ScheduledHeadsForSkillList.AddRange(heads);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill;startdatetime;enddatetime;forecasted agents;total scheduled agents;total diff;total scheduled heads");
			rows[1].Should().Be.EqualTo("skillname;2017-08-15 08:00;2017-08-15 08:15;15,7;8;-7,7;8");
			rows[2].Should().Be.EqualTo("skillname;2017-08-15 08:15;2017-08-15 08:30;15,7;6;-9,7;6");
		}
		
		[Test]
		public void ShouldReturnInternalStaffingAndBpos()
		{
			FakeUserCulture.Is(new CultureInfo("sv-SE"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
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
			
			var heads = new List<ScheduledHeads>
			{
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Heads = 10
				},
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Heads = 10
				}
			};
			SkillCombinationResourceRepository.ScheduledHeadsForSkillList.AddRange(heads);
			
			var bpoResources = (new List<SkillCombinationResourceForBpo>
			{
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 2,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault() }
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault() }
				}
			});		
			
			SkillCombinationResourceRepository.AddBpoResources(bpoResources);
			
			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill;startdatetime;enddatetime;forecasted agents;total scheduled agents;total diff;total scheduled heads;BpoName");
			rows[1].Should().Be.EqualTo("skillname;2017-08-15 08:00;2017-08-15 08:15;15,7;10;-5,7;12;2");
			rows[2].Should().Be.EqualTo("skillname;2017-08-15 08:15;2017-08-15 08:30;15,7;10;-5,7;14;4");
		}
		
		[Test]
		public void ShouldReturnInternalStaffingAndSeveralBpos()
		{
			FakeUserCulture.Is(new CultureInfo("sv-SE"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
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
			
			var heads = new List<ScheduledHeads>
			{
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Heads = 10
				},
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Heads = 14
				}
			};
			SkillCombinationResourceRepository.ScheduledHeadsForSkillList.AddRange(heads);
			
			 var bpoResources = (new List<SkillCombinationResourceForBpo>
			{
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 2,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "Teleopti",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});
			SkillCombinationResourceRepository.AddBpoResources(bpoResources);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill;startdatetime;enddatetime;forecasted agents;total scheduled agents;total diff;total scheduled heads;BpoName;Teleopti");
			rows[1].Should().Be.EqualTo("skillname;2017-08-15 08:00;2017-08-15 08:15;15,7;10;-5,7;12;2;0");
			rows[2].Should().Be.EqualTo("skillname;2017-08-15 08:15;2017-08-15 08:30;15,7;14;-1,7;22;4;4");
		}
		
		[Test]
		public void ShouldReturnStaffingWithOnlyBpos()
		{
			FakeUserCulture.Is(new CultureInfo("sv-SE"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			
			 var bpoResources = (new List<SkillCombinationResourceForBpo>
			{
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 2,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "Teleopti",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});
			SkillCombinationResourceRepository.AddBpoResources(bpoResources);
			
			var heads = new List<ScheduledHeads>
			{
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Heads = 2
				},
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Heads = 8
				}
			};
			SkillCombinationResourceRepository.ScheduledHeadsForSkillList.AddRange(heads);
			
			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill;startdatetime;enddatetime;forecasted agents;total scheduled agents;total diff;total scheduled heads;BpoName;Teleopti");
			rows[1].Should().Be.EqualTo("skillname;2017-08-15 08:00;2017-08-15 08:15;15,7;2;-13,7;4;2;0");
			rows[2].Should().Be.EqualTo("skillname;2017-08-15 08:15;2017-08-15 08:30;15,7;8;-7,7;16;4;4");
		}
		
		[Test]
		public void ShouldReturnPositiveWhenOverstaffed()
		{
			FakeUserCulture.Is(new CultureInfo("sv-SE"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
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
				
			var heads = new List<ScheduledHeads>
			{
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Heads = 10
				},
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Heads = 17
				}
			};
			SkillCombinationResourceRepository.ScheduledHeadsForSkillList.AddRange(heads);
			
			var bpoResources = (new List<SkillCombinationResourceForBpo>
			{
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Resource = 2,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault() }
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault() }
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 7,
					Source = "Teleopti",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault() }
				}
			});
			SkillCombinationResourceRepository.AddBpoResources(bpoResources);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill;startdatetime;enddatetime;forecasted agents;total scheduled agents;total diff;total scheduled heads;BpoName;Teleopti");
			rows[1].Should().Be.EqualTo("skillname;2017-08-15 08:00;2017-08-15 08:15;15,7;10;-5,7;12;2;0");
			rows[2].Should().Be.EqualTo("skillname;2017-08-15 08:15;2017-08-15 08:30;15,7;17;1,3;28;4;7");
		}
		
		[Test]
		public void ShouldHandleShrinkage()
		{
			FakeUserCulture.Is(new CultureInfo("en-US"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);			
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15);
			skillDay.SkillStaffPeriodCollection.ForEach(ssp => ssp.Payload.Shrinkage = new Percent(0.25));
			skillDay.SkillDataPeriodCollection.ForEach(sdpc => sdpc.Shrinkage = new Percent(0.25));
			
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

			var heads = new List<ScheduledHeads>
			{
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					Heads = 8
				},
				new ScheduledHeads
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Heads = 6
				}
			};
			SkillCombinationResourceRepository.ScheduledHeadsForSkillList.AddRange(heads);

			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 15));
			var forecastedData = Target.ExportDemand(skill, period, true);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill,startdatetime,enddatetime,forecasted agents,total scheduled agents,total diff,total scheduled heads");
			rows[1].Should().Be.EqualTo("skillname,8/15/2017 8:00 AM,8/15/2017 8:15 AM,20,8,-12,8");
			rows[2].Should().Be.EqualTo("skillname,8/15/2017 8:15 AM,8/15/2017 8:30 AM,20,6,-14,6");
		}

		[Test]
		public void ShouldExportDayWhenInEstTimezone()
		{
			FakeUserCulture.Is(new CultureInfo("en-US"));
			UserTimeZone.IsNewYork();
			var skill = createSkillWithTimezone(15, "skillname", new TimePeriod(22, 0, 22, 30), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
			skill.SetId(Guid.NewGuid());

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(22, 0, 22, 30), 15);
			skillDay.SkillStaffPeriodCollection.ForEach(ssp => ssp.Payload.Shrinkage = new Percent(0.25));
			skillDay.SkillDataPeriodCollection.ForEach(sdpc => sdpc.Shrinkage = new Percent(0.25));

			var skillDay2 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 16), new TimePeriod(22, 0, 22, 30), 15);
			skillDay2.SkillStaffPeriodCollection.ForEach(ssp => ssp.Payload.Shrinkage = new Percent(0.25));
			skillDay2.SkillDataPeriodCollection.ForEach(sdpc => sdpc.Shrinkage = new Percent(0.25));


			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			SkillDayRepository.Add(skillDay2);

			
			var bpoResources = (new List<SkillCombinationResourceForBpo>
			{
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 16, 2, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 16, 2, 15, 0).Utc(),
					Resource = 8,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 16, 2, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 16, 2, 30, 0).Utc(),
					Resource = 6,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 17, 2, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 17, 2, 15, 0).Utc(),
					Resource = 8,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 17, 2, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 17, 2, 30, 0).Utc(),
					Resource = 6,
					Source = "BpoName",
					SkillCombination  = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});
			SkillCombinationResourceRepository.AddBpoResources(bpoResources);

			
			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 16));
			var forecastedData = Target.ExportDemand(skill, period, true);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(5);
			rows[0].Should().Be("skill,startdatetime,enddatetime,forecasted agents,total scheduled agents,total diff,total scheduled heads,BpoName");
			rows[1].Should().Be.EqualTo("skillname,8/15/2017 10:00 PM,8/15/2017 10:15 PM,20,8,-12,8,8");
			rows[2].Should().Be.EqualTo("skillname,8/15/2017 10:15 PM,8/15/2017 10:30 PM,20,6,-14,6,6");
			rows[3].Should().Be.EqualTo("skillname,8/16/2017 10:00 PM,8/16/2017 10:15 PM,20,8,-12,8,8");
			rows[4].Should().Be.EqualTo("skillname,8/16/2017 10:15 PM,8/16/2017 10:30 PM,20,6,-14,6,6");
		}

		[Test]
		public void ShouldReturnCorrectNumberOfIntervals()
		{
			FakeUserCulture.Is(new CultureInfo("sv-SE"));
			var skill = createSkillWithFullOpenHours(15, "skillname");
			skill.SetId(Guid.NewGuid());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var skillDay0 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 14), new TimePeriod(0, 0, 24, 0), 15.7);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(0, 0, 24, 0), 15.7);
			var skillDay2 = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 16), new TimePeriod(0, 0, 24, 0), 15.7);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay0);
			SkillDayRepository.Add(skillDay);
			SkillDayRepository.Add(skillDay2);
			
			var period = new DateOnlyPeriod(new DateOnly(2017, 8, 15), new DateOnly(2017, 8, 15));
			var forecastedData = Target.ExportDemand(skill, period);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(96+1);
		}
		
		private static ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
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

		private static ISkill createSkillWithTimezone(int intervalLength, string skillName, TimePeriod openHours, TimeZoneInfo timeZone)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = timeZone,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}

		private static ISkill createSkillWithFullOpenHours(int intervalLength, string skillName)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			var workload =  WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			workload.SetId(Guid.NewGuid());

			return skill;
		}
	}
}