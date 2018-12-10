using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;

using SkillFactory = Teleopti.Ccc.TestCommon.FakeData.SkillFactory;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class ScheduleForecastSkillResourceCalculationTest
	{
		private ScheduleForecastSkillResourceCalculation _target;
		private MockRepository _mocks;
		private ISchedulingResultService _schedulingResultService;
		private IDictionary<ISkill, IEnumerable<ISkillDay>> _skillDaysDictionary;
		private DateTimePeriod _scheduleLoadedForPeriod;
		private readonly DateTime _updatedOnDateTime = DateTime.Now;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingResultService = _mocks.StrictMock<ISchedulingResultService>();

			ISkill skill = SkillFactory.CreateSkill("Skill A", SkillTypeFactory.CreateSkillType(), 15);
			skill.SetId(Guid.NewGuid());

			var period = new DateOnlyPeriod(2009, 9, 11, 2009, 9, 11);
			ICollection<ISkillDay> skillDayCollection = ForecastFactory.CreateSkillDayCollection(period, skill, _updatedOnDateTime);
			_scheduleLoadedForPeriod = period.ToDateTimePeriod(skill.TimeZone);
			_skillDaysDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			_skillDaysDictionary.Add(skill, skillDayCollection.ToList());

			const int intervalPerDay = 96;

			_target = new ScheduleForecastSkillResourceCalculation(new ShovelResources(new ReducePrimarySkillResources(), new AddResourcesToSubSkills(), new SkillSetPerActivityProvider(), new PrimarySkillOverstaff(), new TimeZoneGuard(), new AddBpoResourcesToContext()),
				_skillDaysDictionary,
				_schedulingResultService,
				new SkillStaffPeriodHolder(_skillDaysDictionary),
				new ScheduleDictionaryForTest(new Scenario("_"), new DateTimePeriod()),
				new[] { skill },
				intervalPerDay,
				_scheduleLoadedForPeriod,
				new CascadingResourceCalculationContextFactory(new CascadingPersonSkillProvider(), TimeZoneGuard.Instance, new AddBpoResourcesToContext()));
		}

		[Test]
		public void VerifyGetResourceDataExcludingShrinkage()
		{
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultService.SchedulingResult(_scheduleLoadedForPeriod)).Return(new SkillResourceCalculationPeriodWrapper(new SkillSkillStaffPeriodExtendedDictionary())).Repeat.Twice();
			}

			using (_mocks.Playback())
			{
				// First get 
				Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> resourceDataExcludingShrinkage =
					 _target.GetResourceDataExcludingShrinkage(DateTime.Now, null);

				KeyValuePair<IScheduleForecastSkillKey, IScheduleForecastSkill> exclShrinkage = resourceDataExcludingShrinkage.First();
				IScheduleForecastSkill scheduleForecastSkill;
				resourceDataExcludingShrinkage.TryGetValue(exclShrinkage.Key, out scheduleForecastSkill);

				Assert.IsNotNull(scheduleForecastSkill);
				Assert.Greater(scheduleForecastSkill.ForecastedResources, 0d);
				Assert.AreEqual(scheduleForecastSkill.ForecastedResourcesIncludingShrinkage, 0d);

				Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> resourceDataIncludingShrinkage =
					 _target.GetResourceDataIncludingShrinkage(DateTime.Now, null);

				Assert.AreSame(resourceDataExcludingShrinkage, resourceDataIncludingShrinkage);
				Assert.Greater(scheduleForecastSkill.ForecastedResourcesIncludingShrinkage, 0d);
			}
		}
	}
}
