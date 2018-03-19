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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class ExportForecastAndStaffingFileTest: ISetup
	{
		public ExportForecastAndStaffingFile Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		public FakeUserTimeZone UserTimeZone;
		public FakeLoggedOnUser FakeLoggedOnUser;

		
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
		
		[Test]
		public void ShouldHandleNoForecastAndNoStaffing()
		{
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("en-US"));
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
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("en-US"));
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
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("en-US"));
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
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
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
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
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
					SkillCombination = new[] { skill.Id.GetValueOrDefault() }
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "BpoName",
					SkillCombination = new[] { skill.Id.GetValueOrDefault() }
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
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
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
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "BpoName",
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "Teleopti",
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
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
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
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
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "BpoName",
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "Teleopti",
					SkillCombination = new[] { skill.Id.GetValueOrDefault()}
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
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
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
					SkillCombination = new[] { skill.Id.GetValueOrDefault() }
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 4,
					Source = "BpoName",
					SkillCombination = new[] { skill.Id.GetValueOrDefault() }
				},
				new SkillCombinationResourceForBpo
				{
					StartDateTime = new DateTime(2017, 08, 15, 8, 15, 0).Utc(),
					EndDateTime = new DateTime(2017, 08, 15, 8, 30, 0).Utc(),
					Resource = 7,
					Source = "Teleopti",
					SkillCombination = new[] { skill.Id.GetValueOrDefault() }
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
			FakeLoggedOnUser.CurrentUser().PermissionInformation.SetCulture(new CultureInfo("en-US"));
			var skill = createSkill(15, "skillname", new TimePeriod(8, 0, 8, 30));
			skill.SetId(Guid.NewGuid());
			
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);			
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2017, 8, 15), new TimePeriod(8, 0, 8, 30), 15);
			skillDay.CompleteSkillStaffPeriodCollection.ForEach(ssp => ssp.Payload.Shrinkage = new Percent(0.5));
			
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
			var forecastedData = Target.ExportDemand(skill, period, true);
			var rows = forecastedData.Split(new[] { "\r\n" }, StringSplitOptions.None);
			rows.Length.Should().Be(3);
			rows[0].Should().Be("skill,startdatetime,enddatetime,forecasted agents,total scheduled agents,total diff,total scheduled heads");
			rows[1].Should().Be.EqualTo("skillname,8/15/2017 8:00 AM,8/15/2017 8:15 AM,22.5,8,-14.5,8");
			rows[2].Should().Be.EqualTo("skillname,8/15/2017 8:15 AM,8/15/2017 8:30 AM,22.5,6,-16.5,6");
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
	}
}