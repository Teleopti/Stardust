using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
	[TestFixture]
	public class OccupiedSeatCalculatorTest
	{
		private MockRepository _mocks;
		private IOccupiedSeatCalculator _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new OccupiedSeatCalculator();
		}

		[Test]
		public void ShouldAddLoggedOnAndUsedSeatsToSkillStaffPeriod()
		{
			var relevantProjections = _mocks.StrictMock<IResourceCalculationDataContainer>();
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateSiteSkill("sitSkill");
			DateOnly day = new DateOnly(2010, 10, 10);
			DateTime dateTime = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(skill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(skill, skillStaffPeriodDictionary);
			
			using(_mocks.Record())
			{
				Expect.Call(relevantProjections.ActivityResourcesWhereSeatRequired(skill, skillStaffPeriod.Period)).Return(4.5d);
			}

			using(_mocks.Playback())
			{
				_target.Calculate(day, relevantProjections, new SkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
			}
			Assert.AreEqual(4.5, skillStaffPeriod.Payload.CalculatedUsedSeats);
			Assert.AreEqual(4.5, skillStaffPeriod.CalculatedLoggedOn);
		}

		[Test]
		public void ShouldReturnZeroSeatsUsedIfNoWorkShifts()
		{
			IResourceCalculationDataContainer relevantProjections = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateSiteSkill("sitSkill");
			DateOnly day = new DateOnly(2010, 10, 10);
			DateTime dateTime = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(skill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(skill, skillStaffPeriodDictionary);

			skillStaffPeriod.Payload.CalculatedUsedSeats = 7;

			using (_mocks.Record())
			{
			}

			using (_mocks.Playback())
			{
				_target.Calculate(day, relevantProjections, new SkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
			}
			Assert.AreEqual(0, skillStaffPeriod.Payload.CalculatedUsedSeats);
		}
	}
}