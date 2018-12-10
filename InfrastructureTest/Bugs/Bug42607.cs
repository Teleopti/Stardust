using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[TestFixture]
	[InfrastructureTest]
	[Category("BucketB")]
	public class Bug42607
	{
		public IOpenAndSplitSkillCommand Target;
		public IScenarioRepository ScenarioRepository;
		public ISkillRepository SkillRepository;
		public IWorkloadRepository WorkloadRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public ISkillDayRepository SkillDayRepository;
		public IActivityRepository ActivityRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		[Test]
		public void ShouldNotGenerateDuplicateSkillDataPeriod()
		{
			var scenario = new Scenario("scenario") {DefaultScenario = true};
			var activity = new Activity("act");
			var skillType = SkillTypeFactory.CreateSkillType();
			var skillWith15MinutesResolution = new Skill("skill", "skill", Color.Blue, 15, skillType)
			{
				TimeZone = TimeZoneInfo.Utc,
				Activity = activity
			};
			var workload = WorkloadFactory.CreateWorkload(skillWith15MinutesResolution);
			var period = new DateOnlyPeriod(2017, 1, 23, 2017, 1, 23);

			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				SkillTypeRepository.Add(skillType);
				ActivityRepository.Add(activity);
				SkillRepository.Add(skillWith15MinutesResolution);
				WorkloadRepository.Add(workload);
				setup.PersistAll();
			}

			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				Target.Execute(skillWith15MinutesResolution, period,
				new List<TimePeriod> { new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(24)) });
				setup.PersistAll();
			}

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var skillDays = SkillDayRepository.FindRange(period, skillWith15MinutesResolution, scenario);
				skillDays.Count.Should().Be.EqualTo(1);
				skillDays.First().SkillDataPeriodCollection.Count.Should().Be.EqualTo(96);
				// Just before setup.PersistAll() after the Target.Execute you can see that the newly created SkillDay 
				// have 96 items in SkillDataPeriod. But after persist it has 192. But whyyyyyy!!?????
			}
		}
	}
}