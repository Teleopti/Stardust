using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
	[TestFixture]
	public class OccupiedSeatCalculatorTest
	{
		private MockRepository _mocks;
		private IOccupiedSeatCalculator _target;
		private ISkillVisualLayerCollectionDictionaryCreator _skillVisualLayerCollectionDictionaryCreator;
		private ISeatImpactOnPeriodForProjection _seatImpactOnPeriodForProjection;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skillVisualLayerCollectionDictionaryCreator = _mocks.StrictMock<ISkillVisualLayerCollectionDictionaryCreator>();
			_seatImpactOnPeriodForProjection = _mocks.StrictMock<ISeatImpactOnPeriodForProjection>();
			_target = new OccupiedSeatCalculator(_skillVisualLayerCollectionDictionaryCreator, _seatImpactOnPeriodForProjection);
		}

		[Test]
		public void ShouldAddUsedSeatsToSkillStaffPeriod()
		{
			IList<IVisualLayerCollection> relevantProjections = new List<IVisualLayerCollection>();
			relevantProjections.Add(_mocks.StrictMock<IVisualLayerCollection>());
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateSiteSkill("sitSkill");
			DateOnly day = new DateOnly(2010, 10, 10);
			DateTime dateTime = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(skill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(skill, skillStaffPeriodDictionary);
			
			SkillVisualLayerCollectionDictionary skillVisualLayerCollectionDictionary = new SkillVisualLayerCollectionDictionary();
			skillVisualLayerCollectionDictionary.Add(skill, relevantProjections);

			using(_mocks.Record())
			{
				Expect.Call(
					_skillVisualLayerCollectionDictionaryCreator.CreateSiteVisualLayerCollectionDictionary(relevantProjections, day)).
					Return(skillVisualLayerCollectionDictionary);
				Expect.Call(_seatImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod, relevantProjections)).Return(4.5d);
			}

			using(_mocks.Playback())
			{
				_target.Calculate(day, relevantProjections, relevantSkillStaffPeriods);
			}
			Assert.AreEqual(4.5, skillStaffPeriod.Payload.CalculatedUsedSeats);
		}

		[Test]
		public void ShouldReturnZeroSeatsUsedIfNoWorkShifts()
		{
			IList<IVisualLayerCollection> relevantProjections = new List<IVisualLayerCollection>();
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
			ISkill skill = SkillFactory.CreateSiteSkill("sitSkill");
			DateOnly day = new DateOnly(2010, 10, 10);
			DateTime dateTime = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(skill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(skill, skillStaffPeriodDictionary);

			SkillVisualLayerCollectionDictionary skillVisualLayerCollectionDictionary = new SkillVisualLayerCollectionDictionary();

			skillStaffPeriod.Payload.CalculatedUsedSeats = 7;

			using (_mocks.Record())
			{
				Expect.Call(
					_skillVisualLayerCollectionDictionaryCreator.CreateSiteVisualLayerCollectionDictionary(relevantProjections, day)).
					Return(skillVisualLayerCollectionDictionary);
				
			}


			using (_mocks.Playback())
			{
				_target.Calculate(day, relevantProjections, relevantSkillStaffPeriods);
			}
			Assert.AreEqual(0, skillStaffPeriod.Payload.CalculatedUsedSeats);
		}
	}
}