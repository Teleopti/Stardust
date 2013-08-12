using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SingleSkillMaxSeatCalculatorTest
	{
		private SingleSkillMaxSeatCalculator _singleSkillMaxSeatCalculator;
		private IList<IScheduleDay> _toRemove;
		private IList<IScheduleDay> _toAdd;
		private ISkillSkillStaffPeriodExtendedDictionary _relevantSkillStaffPeriods;
		private ISkillStaffPeriod _skillStaffPeriod;
		private IVisualLayerCollection _visualLayerCollection;
		private ISkill _skill;
		private ISkill _skillMaxSeat;
		private Activity _activity;
		private IPerson _person;
		private DateTime _dateTime;
		private MockRepository _mocks;
		private IPersonSkillProvider _personSkillProvider;
		private IScheduleDay _scheduleDay;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_personSkillProvider = new PersonSkillProvider();
			_singleSkillMaxSeatCalculator = new SingleSkillMaxSeatCalculator(_personSkillProvider);
			_toAdd = new List<IScheduleDay>();
			_toRemove = new List<IScheduleDay>();

			_dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			_skill = SkillFactory.CreateSkill("skill");
			_skill.SetId(Guid.NewGuid());
			_skillMaxSeat = SkillFactory.CreateSiteSkill("maxSeatSkill");
			_skillMaxSeat.SetId(Guid.NewGuid());
			_activity = new Activity("activity") { RequiresSeat = true };
			_activity.SetId(Guid.NewGuid());
			_skill.Activity = _activity;
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skill, _skillMaxSeat });
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person, TimeSpan.FromHours(8), TimeSpan.FromHours(9), _skill.Activity);
			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skillMaxSeat, _dateTime, 60, 0, 0);
			_relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skillMaxSeat);
			skillStaffPeriodDictionary.Add(_skillStaffPeriod.Period, _skillStaffPeriod);
			_relevantSkillStaffPeriods.Add(_skillMaxSeat, skillStaffPeriodDictionary);
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldCalculateAddAndRemove()
		{
			var projectionService = _mocks.StrictMock<IProjectionService>();
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_dateTime), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				_toAdd.Add(_scheduleDay);
				_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
				Assert.AreEqual(1, _skillStaffPeriod.CalculatedResource);
				Assert.AreEqual(1, _skillStaffPeriod.Payload.CalculatedUsedSeats);

				_toAdd.Clear();
				_toRemove.Add(_scheduleDay);
				_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
				Assert.AreEqual(0, _skillStaffPeriod.CalculatedResource);
				Assert.AreEqual(0, _skillStaffPeriod.Payload.CalculatedUsedSeats);
			}
		}

		[Test]
		public void ShouldCalculateFractions()
		{
			var projectionService = _mocks.StrictMock<IProjectionService>();
			_skill.DefaultResolution = 60;
			_skillMaxSeat.DefaultResolution = 60;
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person,
																					 TimeSpan.FromHours(8)
																							 .Add(TimeSpan.FromMinutes(15)),
																					 TimeSpan.FromHours(9)
																							 .Add(TimeSpan.FromMinutes(-15)),
																					 _skill.Activity);
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_dateTime), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				_toAdd.Add(_scheduleDay);
				_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
				Assert.AreEqual(0.5, _skillStaffPeriod.CalculatedResource);
				Assert.AreEqual(0.5, _skillStaffPeriod.Payload.CalculatedUsedSeats);
			}
		}

		[Test]
		public void ShouldCalculateFractionsOn30MinIntervals1()
		{
			var projectionService = _mocks.StrictMock<IProjectionService>();
			_skill.DefaultResolution = 30;
			_skillMaxSeat.DefaultResolution = 30;
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person,
																					 TimeSpan.FromHours(8)
																							 .Add(TimeSpan.FromMinutes(15)),
																					 TimeSpan.FromHours(9)
																							 .Add(TimeSpan.FromMinutes(15)),
																					 _skill.Activity);
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_dateTime), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, _dateTime, 30, 0, 0);
				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skillMaxSeat);
				skillStaffPeriodDictionary.Add(_skillStaffPeriod.Period, _skillStaffPeriod);
				_relevantSkillStaffPeriods.Clear();
				_relevantSkillStaffPeriods.Add(_skillMaxSeat, skillStaffPeriodDictionary);

				_toAdd.Add(_scheduleDay);
				_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
				Assert.AreEqual(0.5, _skillStaffPeriod.CalculatedResource);
				Assert.AreEqual(0.5, _skillStaffPeriod.Payload.CalculatedUsedSeats);
			}
		}

		[Test]
		public void ShouldCalculateFractionsOn30MinIntervals2()
		{
			var projectionService = _mocks.StrictMock<IProjectionService>();
			_skill.DefaultResolution = 30;
			_skillMaxSeat.DefaultResolution = 30;
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person,
																						 TimeSpan.FromHours(8)
																								 .Add(TimeSpan.FromMinutes(15)),
																						 TimeSpan.FromHours(9), _skill.Activity);
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_dateTime), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, _dateTime, 30, 0, 0);
				ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skillMaxSeat);
				skillStaffPeriodDictionary.Add(_skillStaffPeriod.Period, _skillStaffPeriod);
				_relevantSkillStaffPeriods.Clear();
				_relevantSkillStaffPeriods.Add(_skillMaxSeat, skillStaffPeriodDictionary);

				_toAdd.Add(_scheduleDay);
				_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
				Assert.AreEqual(0.5, _skillStaffPeriod.CalculatedResource);
				Assert.AreEqual(0.5, _skillStaffPeriod.Payload.CalculatedUsedSeats);
			}
		}

		[Test]
		public void ShouldNotCalculateOnEmptyInput()
		{
			_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
			Assert.AreEqual(0, _skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, _skillStaffPeriod.Payload.CalculatedUsedSeats);		
		}

		[Test]
		public void ShouldNotCalculateWhenPersonDoNotHaveMaxSeatSkill()
		{
			var projectionService = _mocks.StrictMock<IProjectionService>();
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skill });
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person, TimeSpan.FromHours(8),
																					 TimeSpan.FromHours(9), _skill.Activity);

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(_dateTime), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
				Expect.Call(projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				_toAdd.Add(_scheduleDay);
				_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
				Assert.AreEqual(0, _skillStaffPeriod.CalculatedResource);
				Assert.AreEqual(0, _skillStaffPeriod.Payload.CalculatedUsedSeats);
			}
		}	
	}
}
