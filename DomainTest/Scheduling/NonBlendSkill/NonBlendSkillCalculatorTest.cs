using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.NonBlendSkill
{
	[TestFixture]
	public class NonBlendSkillCalculatorTest
	{
		private MockRepository _mocks;
		private INonBlendSkillCalculator _target;
		private INonBlendSkillImpactOnPeriodForProjection _nonBlendSkillImpactOnPeriodForProjection;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_nonBlendSkillImpactOnPeriodForProjection = _mocks.StrictMock<INonBlendSkillImpactOnPeriodForProjection>();
			_target = new NonBlendSkillCalculator(_nonBlendSkillImpactOnPeriodForProjection);
		}

		[Test]
		public void ShouldAddResourceAndLoggedOntoSkillStaffPeriod()
		{
			var relevantProjections = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateNonBlendSkill("nonBlendSkill");
			DateOnly day = new DateOnly(2010, 10, 10);
			DateTime dateTime = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(skill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(skill, skillStaffPeriodDictionary);

			using (_mocks.Record())
			{
                Expect.Call(_nonBlendSkillImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod, relevantProjections, skill)).Return(4.5d);
			}

			using (_mocks.Playback())
			{
				_target.Calculate(day, relevantProjections, relevantSkillStaffPeriods,false);
			}
			Assert.AreEqual(4.5, skillStaffPeriod.Payload.CalculatedLoggedOn);
			Assert.AreEqual(4.5, skillStaffPeriod.CalculatedResource);
		}

		[Test]
		public void ShouldNotAddResourceIfNotNonBlendSkill()
		{
			var relevantProjections = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateSkill("nonBlendSkill");
			DateOnly day = new DateOnly(2010, 10, 10);
			DateTime dateTime = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(skill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(skill, skillStaffPeriodDictionary);

			using (_mocks.Record())
			{

			}

			using (_mocks.Playback())
			{
				_target.Calculate(day, relevantProjections, relevantSkillStaffPeriods,false);
			}
		}
	}
}