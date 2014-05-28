using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
	[TestFixture]
	public class SeatLimitationWorkShiftCalculator2Test
	{
		private ISeatLimitationWorkShiftCalculator2 _target;
		private MockRepository _mocks;
		private IPerson _person;
		private ISkill _skill;
		private ISeatImpactOnPeriodForProjection _seatImpactOnPeriodForProjection;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skill = SkillFactory.CreateSiteSkill("siteSkill");
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
			_person.Period(new DateOnly()).PersonMaxSeatSkillCollection.Add(new PersonSkill(_skill, new Percent(1)));
			_seatImpactOnPeriodForProjection = _mocks.StrictMock<ISeatImpactOnPeriodForProjection>();
			_target = new SeatLimitationWorkShiftCalculator2(_seatImpactOnPeriodForProjection);
		}

		[Test]
		public void ShouldReturnNegativeValueIfMaxSeatsIsBrokenAndOverstaffingAllowed()
		{
			DateTime dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
			DateTimePeriod dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
			ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, dateTime);
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);
			skillStaffPeriod1.Payload.CalculatedUsedSeats = 5;
			skillStaffPeriod1.Payload.MaxSeats = 5;
			skillStaffPeriod1.SetSkillDay(skillDay);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill);
			skillStaffPeriodDictionary.Add(dateTimePeriod, skillStaffPeriod1);
			IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
			dic.Add(_skill, skillStaffPeriodDictionary);
			IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			using(_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_seatImpactOnPeriodForProjection.SkillStaffPeriodDate(skillStaffPeriod1, _person)).Return(new DateOnly());
				Expect.Call(_seatImpactOnPeriodForProjection.CheckPersonSkill(_skill, _person, new DateOnly())).Return(true);
				Expect.Call(_seatImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod1, visualLayerCollection)).
				Return(1).Repeat.AtLeastOnce();
			}

			double? result;

			using(_mocks.Playback())
			{
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, MaxSeatsFeatureOptions.ConsiderMaxSeats);
			}
			Assert.AreEqual(-10000, result.Value);
		}

		[Test]
		public void ShouldReturnNullIfMaxSeatsIsBrokenAndOverstaffingNotAllowed()
		{
			DateTime dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
			DateTimePeriod dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
			ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, dateTime);
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);
			skillStaffPeriod1.Payload.CalculatedUsedSeats = 5;
			skillStaffPeriod1.Payload.MaxSeats = 5;
			skillStaffPeriod1.SetSkillDay(skillDay);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill);
			skillStaffPeriodDictionary.Add(dateTimePeriod, skillStaffPeriod1);
			IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
			dic.Add(_skill, skillStaffPeriodDictionary);
			IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_seatImpactOnPeriodForProjection.SkillStaffPeriodDate(skillStaffPeriod1, _person)).Return(new DateOnly());
				Expect.Call(_seatImpactOnPeriodForProjection.CheckPersonSkill(_skill, _person, new DateOnly())).Return(true);
				Expect.Call(_seatImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod1, visualLayerCollection)).
				Return(1).Repeat.AtLeastOnce();
			}

			double? result;

			using (_mocks.Playback())
			{
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak);
			}
			Assert.IsNull(result);
		}
		[Test]
		public void ShouldReturnZeroIfBelowOrAtMaxSeatLimit()
		{
			DateTime dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
			DateTimePeriod dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
			ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, dateTime);
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);
			skillStaffPeriod1.Payload.CalculatedUsedSeats = 4;
			skillStaffPeriod1.Payload.MaxSeats = 5;
			skillStaffPeriod1.SetSkillDay(skillDay);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill);
			skillStaffPeriodDictionary.Add(dateTimePeriod, skillStaffPeriod1);
			IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
			dic.Add(_skill, skillStaffPeriodDictionary);
			IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_seatImpactOnPeriodForProjection.SkillStaffPeriodDate(skillStaffPeriod1, _person)).Return(new DateOnly());
				Expect.Call(_seatImpactOnPeriodForProjection.CheckPersonSkill(_skill, _person, new DateOnly())).Return(true);
				Expect.Call(_seatImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod1, visualLayerCollection)).
					Return(1).Repeat.AtLeastOnce();
			}

			double? result;

			using (_mocks.Playback())
			{
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, MaxSeatsFeatureOptions.ConsiderMaxSeats);
			}
			Assert.AreEqual(0, result.Value);
		}

		[Test]
		public void ShouldReturnZeroIfLayerPeriodIsNull()
		{
			DateTime dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
			DateTimePeriod dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
			ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, dateTime);
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);
			skillStaffPeriod1.Payload.CalculatedUsedSeats = 5;
			skillStaffPeriod1.Payload.MaxSeats = 5;
			skillStaffPeriod1.SetSkillDay(skillDay);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill);
			skillStaffPeriodDictionary.Add(dateTimePeriod, skillStaffPeriod1);
			IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
			dic.Add(_skill, skillStaffPeriodDictionary);
			IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Period()).Return(null).Repeat.AtLeastOnce();
			}

			double? result;

			using (_mocks.Playback())
			{
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, MaxSeatsFeatureOptions.ConsiderMaxSeats);
			}
			Assert.AreEqual(0, result.Value);
		}

		[Test]
		public void ShouldReturnZeroIfLayersDoNotIntersect()
		{
			DateTime dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
			DateTimePeriod dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
			ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, dateTime);
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);
			skillStaffPeriod1.Payload.CalculatedUsedSeats = 5;
			skillStaffPeriod1.Payload.MaxSeats = 5;
			skillStaffPeriod1.SetSkillDay(skillDay);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill);
			skillStaffPeriodDictionary.Add(dateTimePeriod, skillStaffPeriod1);
			IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
			dic.Add(_skill, skillStaffPeriodDictionary);
			IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod.MovePeriod(TimeSpan.FromHours(1))).Repeat.AtLeastOnce();
			}

			double? result;

			using (_mocks.Playback())
			{
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, MaxSeatsFeatureOptions.ConsiderMaxSeats);
			}
			Assert.AreEqual(0, result.Value);
		}
	}
}